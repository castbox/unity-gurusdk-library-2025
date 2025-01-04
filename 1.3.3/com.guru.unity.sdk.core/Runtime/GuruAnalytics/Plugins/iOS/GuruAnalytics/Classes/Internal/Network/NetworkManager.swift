//
//  Network.swift
//  GuruAnalytics_iOS
//
//  Created by mayue on 2022/11/3.
//  Copyright © 2022 Guru Network Limited. All rights reserved.
//

import Foundation
import Alamofire
import RxSwift
import RxRelay
import Gzip

internal class NetworkManager {
    
    private static let ipErrorUserInfoKey = "failed_ip"
    
    internal var isReachable: Bool {
        return _reachableObservable.value
    }
    
    internal var reachableObservable: Observable<Bool> {
        return _reachableObservable.asObservable()
    }
    
    private let _reachableObservable = BehaviorRelay(value: false)
    
    private let reachablity = NetworkReachabilityManager()
    
    private let networkQueue = DispatchQueue.init(label: "com.guru.analytics.network.queue", qos: .userInitiated)
    private lazy var rxWorkScheduler = SerialDispatchQueueScheduler.init(queue: networkQueue, internalSerialQueueName: "com.guru.analytics.network.rx.work.queue")
    
    private lazy var session: Session = {
        let trustManager = CertificatePinnerServerTrustManager()
        trustManager.evaluator.hostWhiteList = hostsMap
        return Session(serverTrustManager: trustManager)
    }()
    
    private var hostsMap: [String : [String]] {
        get {
            return UserDefaults.defaults?.value(forKey: UserDefaults.hostsMapKey) as? [String : [String]] ?? [:]
        }
        
        set {
            UserDefaults.defaults?.set(newValue, forKey: UserDefaults.hostsMapKey)
            (session.serverTrustManager as? CertificatePinnerServerTrustManager)?.evaluator.hostWhiteList = newValue
            checkHostMap(newValue)
        }
    }
    
    internal weak var networkErrorReporter: GuruAnalyticsNetworkErrorReportDelegate?
    
    internal init() {
        
        reachablity?.startListening(onQueue: networkQueue, onUpdatePerforming: { [weak self] status in
            var reachable: Bool
            switch status {
            case .reachable(_):
                reachable = true
            case .notReachable, .unknown:
                reachable = false
            }
            self?._reachableObservable.accept(reachable)
        })
        
        APIService.Backend.allCases.forEach({ service in
            _ = lookupHostRemote(service.host).subscribe()
        })
    }
    
    /// 上报event请求
    /// - Parameter events: event record数组
    /// - Returns: 上报成功的event record ID数组
    internal func uploadEvents(_ events: [Entity.EventRecord]) -> Single<(recordIDs: [String], eventsJson: String)> {
        guard !events.isEmpty else {
            return .just(([], ""))
        }
        
        let service = APIService.Backend.event
        
        return lookupHostLocal(service.host)
            .flatMap { ip in
                
                Single.create { [weak self] subscriber in
                    guard let `self` = self else {
                        subscriber(.failure(
                            NSError(domain: "networkManager", code: 0, userInfo: [NSLocalizedDescriptionKey : "manager is released"])
                        ))
                        return Disposables.create()
                    }
                    var postJson = [String : Any]()
                    postJson["version"] = service.version
                    postJson["deviceInfo"] = Constants.deviceInfo
                    let eventJsonArray = events.compactMap { $0.eventJson.jsonObject() }
                    postJson["events"] = eventJsonArray
                    
                    do {
                        let jsonData = try JSONSerialization.data(withJSONObject: postJson)
                        let jsonString = String(data: jsonData, encoding: .utf8) ?? ""
                        let gzippedJsonData = try jsonData.gzipped()
                        let httpBody = gzippedJsonData
                        
                        var urlRequest: URLRequest
                        var urlC = service.urlComponents
                        let session: Session
                        
                        if let ip = ip {
                            session = self.session
                            urlC.host = ip
                            urlRequest = try URLRequest(url: urlC, method: service.method, headers: service.headers)
                            urlRequest.setValue(service.host, forHTTPHeaderField: "host")
                            
                        } else {
                            session = AF
                            urlRequest = try URLRequest(url: urlC, method: service.method, headers: service.headers)
                        }
                        urlRequest.setValue(GuruAnalytics.saasXAPPID, forHTTPHeaderField: "X-APP-ID")
                        urlRequest.setValue(GuruAnalytics.saasXDEVICEINFO, forHTTPHeaderField: "X-DEVICE-INFO")
                        urlRequest.httpBody = httpBody
                        
                        var emptyResponseCodes = DataResponseSerializer.defaultEmptyResponseCodes
                        emptyResponseCodes.insert(200)
                        
                        let request = session.request(urlRequest).validate(statusCode: [200])
                            .responseData(
                                queue: self.networkQueue,
                                emptyResponseCodes: emptyResponseCodes,
                                completionHandler: { response in
                                    cdPrint("\(#function): request: \(urlRequest) \nheader:\(urlRequest.headers) \nhttpbody: \(jsonString) \nresponse: \(response)")
                                    switch response.result {
                                    case .failure(let error):
                                        subscriber(.failure(self.mapError(error, for: ip)))
                                        cdPrint("\(#function) error: \(error)")
                                    case .success:
                                        subscriber(.success((events.map { $0.recordId }, jsonString)))
                                    }
                                })
                        
                        return Disposables.create {
                            request.cancel()
                        }
                    } catch {
                        cdPrint("construct request failed: \(error)")
                        subscriber(.failure(error))
                        return Disposables.create()
                    }
                }
            }
            .do(onError: { [weak self] error in
                self?.reportError(error: error, internalErrorCategory: .serverAPIError)
            })
            .catch { [weak self] error in
                
                guard let `self` = self else { throw error }
                
                return try self.errorCatcher(error, for: service.host) {
                    self.uploadEvents(events)
                }
            }
            .subscribe(on: rxWorkScheduler)
    }
    
    /// 同步服务器时间请求
    /// - Returns: 毫秒整数
    internal func syncServerTime() -> Single<Int64> {
        let service = APIService.Backend.systemTime
        
        return lookupHostLocal(service.host)
            .flatMap { ip in
                
                Single.create { [weak self] subscriber in
                    
                    guard let `self` = self else {
                        subscriber(.failure(
                            NSError(domain: "networkManager", code: 0, userInfo: [NSLocalizedDescriptionKey : "manager is released"])
                        ))
                        return Disposables.create()
                    }
                    
                    do {
                        let start = Date()
                        var urlC = service.urlComponents
                        let session: Session
                        var urlReq: URLRequest
                        
                        if let ip = ip {
                            session = self.session
                            urlC.host = ip
                            urlReq = try URLRequest(url: urlC, method: service.method, headers: service.headers)
                            urlReq.setValue(service.host, forHTTPHeaderField: "host")
                        } else {
                            session = AF
                            urlReq = try URLRequest(url: urlC, method: service.method, headers: service.headers)
                        }
                        urlReq.setValue(GuruAnalytics.saasXAPPID, forHTTPHeaderField: "X-APP-ID")
                        urlReq.setValue(GuruAnalytics.saasXDEVICEINFO, forHTTPHeaderField: "X-DEVICE-INFO")
                        
                        let request = session.request(urlReq).validate(statusCode: [200])
                            .responseDecodable(of: Entity.SystemTimeResult.self,
                                               queue: self.networkQueue,
                                               completionHandler: { response in
                                cdPrint("\(#function): request: \(urlReq) \nheaders:\(urlReq.headers) \nresponse: \(response)")
                                switch response.result {
                                case .success(let data):
                                    let timespan = Date().timeIntervalSince(start).int64Ms
                                    let systemTime = data.data - timespan / 2
                                    subscriber(.success(systemTime))
                                case .failure(let error):
                                    cdPrint("\(#function) error: \(error)")
                                    subscriber(.failure(self.mapError(error, for: ip)))
                                }
                            })
                        
                        return Disposables.create {
                            request.cancel()
                        }
                        
                    } catch {
                        cdPrint("construct request failed: \(error)")
                        subscriber(.failure(error))
                        return Disposables.create()
                    }
                    
                }
            }
            .do(onError: { [weak self] error in
                self?.reportError(error: error, internalErrorCategory: .serverAPIError)
            })
            .catch { [weak self] error in
                
                guard let `self` = self else { throw error }
                
                return try self.errorCatcher(error, for: service.host) {
                    self.syncServerTime()
                }
            }
            .subscribe(on: rxWorkScheduler)
    }
    
    private func _lookupHostRemote(_ host: String) -> Single<[IpAdress]> {
        return Single.create { subscriber in
            
            do {
                var urlC = URLComponents()
                urlC.scheme = "https"
                urlC.host = "dns.google"
                urlC.path = "/resolve"
                urlC.queryItems = [.init(name: "name", value: "\(host)")]
                
                let urlReq = try URLRequest(url: urlC, method: .get)
                
                let request = AF.request(urlReq)
                    .validate(statusCode: [200])
                    .responseData(completionHandler: { response in
                        switch response.result {
                        case .success(let data):
                            
                            do {
                                guard let dict = try JSONSerialization.jsonObject(with: data, options: .allowFragments) as? [String : Any],
                                      let answerDictArr = dict["Answer"] as? [[String : Any]] else {
                                    let customError = NSError(domain: "com.guru.analytics.network.layer", code: 0,
                                                              userInfo: [NSLocalizedDescriptionKey : "dns.google service returned unexpected data"])
                                    subscriber(.failure(customError))
                                    return
                                }
                                
                                let ips = try JSONDecoder().decodeAnyData([IpAdress].self, from: answerDictArr)
                                subscriber(.success(ips))
                                cdPrint("\(#function) success request: \(urlReq) \nresponse: \(ips)")
                            } catch {
                                subscriber(.failure(error))
                            }
                            
                        case .failure(let error):
                            cdPrint("\(#function) error: \(error) request: \(urlReq)")
                            subscriber(.failure(error))
                        }
                    })
                return Disposables.create {
                    request.cancel()
                }
            } catch {
                cdPrint("construct request failed: \(error)")
                subscriber(.failure(error))
                return Disposables.create()
            }
        }
        .subscribe(on: rxWorkScheduler)
    }
    
    private func lookupHostRemote(_ host: String) -> Single<[String]> {
        return _lookupHostRemote(host)
            .map { ipList -> [String] in
                ipList.compactMap { ip in
                    guard ip.type == 1 else { return nil }
                    return ip.data
                }
            }
            .do(onSuccess: { [weak self] ipList in
                self?.hostsMap[host] = ipList
            }, onError: { [weak self] error in
                self?.reportError(error: error, internalErrorCategory: .googleDNSServiceError)
            })
    }
    
    private func lookupHostLocal(_ host: String) -> Single<String?> {
        return Single.create { [weak self] subscriber in
            
            guard let `self` = self else {
                subscriber(.failure(
                    NSError(domain: "networkManager", code: 0, userInfo: [NSLocalizedDescriptionKey : "manager is released"])
                ))
                return Disposables.create()
            }
            
            subscriber(.success(self.hostsMap[host]?.first))
            
            return Disposables.create()
        }
        .subscribe(on: rxWorkScheduler)
    }
    
    private func mapError(_ error: AFError, for ip: String?) -> Error {
        
        guard let ip = ip else { return error }
        
        var e = (error.underlyingError ?? error) as NSError
        var userInfo = e.userInfo
        userInfo[Self.ipErrorUserInfoKey] = ip
        e = NSError(domain: e.domain, code: e.code, userInfo: userInfo)
        return e
    }
    
    private func errorCatcher<T>(_ error: Error, for host: String, returnValue: (() -> Single<T>) ) throws -> Single<T> {
        
        let e = error as NSError
        guard let ip = e.userInfo[Self.ipErrorUserInfoKey] as? String else {
            throw error
        }
        //FIX: https://console.firebase.google.com/u/1/project/ball-sort-dd4d0/crashlytics/app/ios:ball.sort.puzzle.color.sorting.bubble.games/issues/c1f6d36aeb7c105a32015504776adff5?time=last-ninety-days&sessionEventKey=27d699688a594f96a7b17003a3c49c84_1900062047348716162
        if var hosts = hostsMap[host] {
            hosts.removeAll(where: { $0 == ip })
            hostsMap[host] = hosts
        }
        return returnValue()
    }
    
    private func checkHostMap(_ hostMap: [String : [String]]) {
        
        hostMap.forEach { key, value in
            guard value.count <= 0 else { return }
            _ = lookupHostRemote(key).subscribe()
        }
        
    }
    
    private func reportError(error: Error, internalErrorCategory: GuruAnalyticsNetworkLayerErrorCategory) {
        let customError: GuruAnalyticsNetworkError
        if let aferror = error.asAFError {
            
            if case let AFError.responseValidationFailed(reason) = aferror,
               case let AFError.ResponseValidationFailureReason.unacceptableStatusCode(httpStatusCode) = reason {
                customError = GuruAnalyticsNetworkError(httpStatusCode: httpStatusCode, internalErrorCategory: internalErrorCategory, originError: aferror.underlyingError ?? error)
            } else {
                customError = GuruAnalyticsNetworkError(internalErrorCategory: internalErrorCategory, originError: aferror.underlyingError ?? error)
            }
            
        } else {
            customError = GuruAnalyticsNetworkError(internalErrorCategory: internalErrorCategory, originError: error)
        }
        
        networkErrorReporter?.reportError(networkError: customError)
    }
    
}

internal final class CertificatePinnerTrustEvaluator: ServerTrustEvaluating {
    
    private let dftEvaluator = DefaultTrustEvaluator()
    
    init() {}
    
    var hostWhiteList: [String : [String]] = [:]
    
    func evaluate(_ trust: SecTrust, forHost host: String) throws {
        
        let originHostName: String = hostWhiteList.first { _, value in
            value.contains { $0 == host }
        }?.key ?? host
        
        try dftEvaluator.evaluate(trust, forHost: originHostName)
        
        cdPrint(#function + " \(trust) forHost: \(host) originHostName: \(originHostName)")
    }
}

internal class CertificatePinnerServerTrustManager: ServerTrustManager {
    
    let evaluator = CertificatePinnerTrustEvaluator()
    
    init() {
        super.init(allHostsMustBeEvaluated: true, evaluators: [:])
    }
    
    override func serverTrustEvaluator(forHost host: String) throws -> ServerTrustEvaluating? {
        
        return evaluator
    }
}

extension NetworkManager {
    struct IpAdress: Codable {
        let name: String
        let type: Int
        let TTL: Int
        let data: String
    }
}
