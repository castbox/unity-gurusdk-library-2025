//
//  APIService.swift
//  GuruAnalytics_iOS
//
//  Created by mayue on 2022/11/8.
//

import Foundation
import Alamofire

internal enum APIService {}

extension APIService {
    enum Backend: CaseIterable {
        case event
        case systemTime
    }
}

extension APIService.Backend {
    
    var scheme: String {
        return "https"
    }
    
    var host: String {
        switch self {
        case .systemTime:
            return "saas.castbox.fm"
        case .event:
            return UserDefaults.eventsServerHost ?? "collect.saas.castbox.fm"
        }
    }
    
    var urlComponents: URLComponents {
        var urlC = URLComponents()
        urlC.host = self.host
        urlC.scheme = self.scheme
        urlC.path = self.path
        return urlC
    }
    
    var path: String {
        switch self {
        case .event:
            return "/event"
        case .systemTime:
            return "/tool/api/v1/system/time"
        }
    }
    
    var method: HTTPMethod {
        switch self {
        case .event:
            return .post
        case .systemTime:
            return .get
        }
    }
    
    var headers: HTTPHeaders {
        HTTPHeaders(
            ["Content-Type": "application/json",
             "Content-Encoding": "gzip",
             "x_event_type": "event"]
        )
    }
    
    var version: Int {
        /// 接口版本
        switch self {
        case .event:
            return 10
        case .systemTime:
            return 0
        }
    }
}
