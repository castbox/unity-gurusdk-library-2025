//
//  Manager.swift
//  GuruAnalytics_iOS
//
//  Created by 袁仕崇 on 16/11/22.
//

import Foundation
import RxCocoa
import RxSwift


internal class Manager {
    
    // MARK: - temporary, will be removed soon
    @available(*, deprecated, message: "used for debug, will be removed on any future released versions")
    private var loggedEventsCount: Int = 0
    
    @available(*, deprecated, message: "used for debug, will be removed on any future released versions")
    private func accumulateLoggedEventsCount(_ count: Int) {
        loggedEventsCount += count
    }
    
    @available(*, deprecated, message: "used for debug, will be removed on any future released versions")
    private var uploadedEventsCount: Int = 0
    
    @available(*, deprecated, message: "used for debug, will be removed on any future released versions")
    private func accumulateUploadedEventsCount(_ count: Int) {
        uploadedEventsCount += count
    }
    
    @available(*, deprecated, message: "used for debug, will be removed on any future released versions")
    internal func debug_eventsStatistics(_ callback: @escaping (_ uploadedEventsCount: Int, _ loggedEventsCount: Int) -> Void) {
        callback(uploadedEventsCount, loggedEventsCount)
    }
    
    // MARK: - internal members
    
    internal static let shared = Manager()
    
    /// 时间维度，默认每1分钟后批量上传1次
    private var scheduleInterval: TimeInterval = GuruAnalytics.uploadPeriodInSecond
    
    /// 数量维度，默认满25条批量上传1次
    private var numberOfCountPerConsume: Int = GuruAnalytics.batchLimit
    
    /// event过期时间，默认7天
    private var eventExpiredIntervel: TimeInterval = GuruAnalytics.eventExpiredSeconds
    
    private var initializeTimeout: Double = GuruAnalytics.initializeTimeout
    
    /// 根据时差计算的当前服务端时间
    internal var serverNowMs: Int64 { serverInitialMs + (Date.absoluteTimeMs - serverSyncedAtAbsoluteMs)}
    
    // MARK: - private members
    
    private typealias PropertyName = GuruAnalytics.PropertyName
    private typealias BuiltinParametersKeys = GuruAnalytics.BuiltinParametersKeys
    
    private let bag = DisposeBag()
    
    private let db = Database()
    
    private let ntwkMgr = NetworkManager()
    
    /// 生成background 任务时，将 key 和当前任务的 disposeable 一一对应
    private var taskKeyDisposableMap: [Int: Disposable] = [:]
    
    /// 从数据库中一次性拉取最多条数
    private var maxEventFetchingCount: Int = 100
    
    /// 工作队列
    private let workQueue = DispatchQueue.init(label: "com.guru.analytics.manager.work.queue", qos: .userInitiated)
    ///网络服务队列
    private lazy var rxNetworkScheduler = SerialDispatchQueueScheduler(qos: .default, internalSerialQueueName: "com.guru.analytics.manager.rx.network.queue")
    private lazy var rxConsumeScheduler = SerialDispatchQueueScheduler(qos: .default, internalSerialQueueName: "com.guru.analytics.manager.rx.consume.queue")

    private lazy var rxWorkScheduler = SerialDispatchQueueScheduler.init(queue: workQueue, internalSerialQueueName: "com.guru.analytics.manager.rx.work.queue")
    private let bgWorkQueue = DispatchQueue.init(label: "com.guru.analytics.manager.background.work.queue", qos: .background)
    private lazy var rxBgWorkScheduler = SerialDispatchQueueScheduler.init(queue: bgWorkQueue, internalSerialQueueName: "com.guru.analytics.manager.background.work.queue")
    
    /// 过期event记录已清除
    private let outdatedEventsCleared = BehaviorSubject(value: false)
    
    /// 服务端时间
    private var serverInitialMs = Date().msSince1970 {
        didSet {
            serverSyncedAtAbsoluteMs = Date.absoluteTimeMs
        }
    }
    private var serverSyncedAtAbsoluteMs = Date.absoluteTimeMs
    private let startAt = Date()
    /// 服务器时间已同步信号
    private let _serverTimeSynced = BehaviorRelay(value: false)
    private var serverNowMsSingle: Single<Int64> {
        
        guard _serverTimeSynced.value == false else {
            return .just(serverNowMs)
        }
        return _serverTimeSynced.observe(on: rxNetworkScheduler)
            .filter { $0 }
            .take(1).asSingle()
            .timeout(.seconds(10), scheduler: rxNetworkScheduler)
            .catchAndReturn(false)
            .map({ [weak self] _ in
                return self?.serverNowMs ?? 0
            })
    }
    
    /// 统计fg起始时间
    private var fgStartAtAbsoluteMs = Date.absoluteTimeMs
    private var fgAccumulateTimer: Disposable? = nil
    
    /// 内存中user property 信息
    private var userProperty: Observable<[String : String]> {
        let p = userPropertyUpdated.startWith(()).observe(on: rxWorkScheduler).flatMap { [weak self] _ -> Observable<[String : String]> in
            guard let `self` = self else { return .just([:]) }
            return .create({ subscriber in
                    subscriber.onNext(self._userProperty)
                    subscriber.onCompleted()
//                    debugPrint("userProperty thread queueName: \(Thread.current.queueName)")
                return Disposables.create()
            })
        }
        let latency = self.initializeTimeout - Date().timeIntervalSince(self.startAt)
        let intLatency = Int(latency)

        guard latency > 0 else {
            return p
        }
        
        return p.filter({ property in
            /// 需要等待以下userproperty已设置
            /// PropertyName.deviceId
            /// PropertyName.uid
            /// PropertyName.firebaseId
            guard let deviceId = property[PropertyName.deviceId.rawValue], !deviceId.isEmpty,
                  let uid = property[PropertyName.uid.rawValue], !uid.isEmpty,
                  let firebaseId = property[PropertyName.firebaseId.rawValue], !firebaseId.isEmpty else {
                return false
            }
            return true
        })
        .timeout(.milliseconds(intLatency), scheduler: rxNetworkScheduler)
        .catch { _ in
            return p
        }
    }
    private var _userProperty: [String : String] = [:] {
        didSet {
            userPropertyUpdated.onNext(())
        }
    }
    private var userPropertyUpdated = PublishSubject<Void>()
    
    /// 同步服务器时间触发器
    private let syncServerTrigger = PublishSubject<Void>()
    
    /// 轮询上传event任务
    private var pollingUploadTask: Disposable?
    
    /// 重置轮询上传触发器
    private let reschedulePollingTrigger = BehaviorSubject(value: ())

    /// 记录events相关的logger
    private lazy var eventsLogger: LoggerManager = {
        let l = LoggerManager(logCategoryName: "eventLogs")
        return l
    }()
    
    /// 将错误上报给上层的
    private typealias InternalEventReporter = ((_ eventCode: Int, _ info: String) -> Void)
    private var internalEventReporter: InternalEventReporter?
    
    private lazy var session = Entity.Session()
    private lazy var sessionNumber: Entity.SessionNumber = {
        var sessionNumber = UserDefaults.sessionNumber
        let dayGaps = Calendar.current.dateComponents([.day], from:  Date(timeIntervalSince1970: Double(sessionNumber.createdAtMs) / 1000), to: Date()).day ?? 0
        if (dayGaps > 0) {
            sessionNumber = Entity.SessionNumber.createNumber()
        }
        sessionNumber.number += 1
        UserDefaults.sessionNumber = sessionNumber
        return sessionNumber
    }()
    
    /// Check if running in app extension
    private var isRunningInAppExtension: Bool {
        return UIApplicationUtil.isExecutingInAppExtension
    }
    
    /// Get shared application if available (not in extension)
    
    private var sharedApplication: UIApplication? {
        return UIApplicationUtil.sharedApplication
    }
    
    private init() {
        
        //
        logSDKInitStart()
        
        // first open
        logFirstOpenIfNeeded()
        
        // 监听事件
        setupOberving()
        
        // 检查旧数据
        clearOutdatedEventsIfNeeded()
        
        // 设置轮询上传任务
        setupPollingUpload()
        
        // 先打一个fg
        logFirstFgEvent()
        
        ntwkMgr.networkErrorReporter = self
        
        //
        logSDKInitComplete()
        
        //
        logSessionStart()
    }
}

// MARK: - internal functions
internal extension Manager {
    
    func logEvent(_ eventName: String, parameters: [String : Any]?, priority: Entity.EventRecord.Priority = .DEFAULT) {
        _ = _logEvent(eventName, parameters: parameters, priority: priority)
            .subscribe()
            .disposed(by: bag)
    }
    
    func setUserProperty(_ value: String, forName name: String) {
        eventsLogger.verbose(#function + "name: \(name) value: \(value)")
        workQueue.async { [weak self] in
            self?._userProperty[name] = value
        }
    }
    
    func removeUserProperties(forNames names: [String]) {
        eventsLogger.verbose(#function + "names: \(names)")
        workQueue.async { [weak self] in
            guard let `self` = self else { return }
            var temp = self._userProperty
            for name in names {
                temp.removeValue(forKey: name)
            }
            self._userProperty = temp
        }
    }
    
    func setScreen(_ name: String) {
        setUserProperty(name, forName: PropertyName.screen.rawValue)
    }
    
    private func constructEvent(_ eventName: String,
                                parameters: [String : Any]?,
                                timestamp: Int64,
                                priority: Entity.EventRecord.Priority) -> Single<Entity.EventRecord> {
        
        return userProperty.take(1).observe(on: rxWorkScheduler).asSingle().flatMap { p in
                .create { [weak self] subscriber in
                    do {
                        debugPrint("userProperty thread queueName: \(Thread.current.queueName) count: \(p.count)")
                        var userProperty = p
                        var eventParam = parameters ?? [:]
                        
                        // append screen
                        if let screen = userProperty.removeValue(forKey: PropertyName.screen.rawValue) {
                            eventParam[PropertyName.screen.rawValue] = screen
                        }
                        
                        eventParam[BuiltinParametersKeys.sessionId.rawValue] = self?.session.sessionId
                        eventParam[BuiltinParametersKeys.sessionNo.rawValue] = self?.sessionNumber.number
                        
                        let userInfo = Entity.UserInfo(
                            uid: userProperty.removeValue(forKey: PropertyName.uid.rawValue),
                            deviceId: userProperty.removeValue(forKey: PropertyName.deviceId.rawValue),
                            adjustId: userProperty.removeValue(forKey: PropertyName.adjustId.rawValue),
                            adId: userProperty.removeValue(forKey: PropertyName.adId.rawValue),
                            firebaseId: userProperty.removeValue(forKey: PropertyName.firebaseId.rawValue)
                        )
                        
                        let event = try Entity.Event(timestamp: timestamp,
                                                     event: eventName,
                                                     userInfo: userInfo,
                                                     parameters: eventParam,
                                                     properties: userProperty)
                        let eventRecord = Entity.EventRecord(eventName: event.event, event: event, priority: priority)
                        subscriber(.success(eventRecord))
                    } catch {
                        subscriber(.failure(error))
                    }
                    return Disposables.create()
                }
        }
    }
    
    func eventsLogsArchive(_ callback: @escaping (URL?) -> Void) {
        eventsLogger.logFilesZipArchive()
            .subscribe(onSuccess: { url in
                callback(url)
            }, onFailure: { error in
                callback(nil)
                cdPrint("events logs archive error: \(error)")
            })
            .disposed(by: bag)
    }
    
    func eventsLogsDirURL(_ callback: @escaping (URL?) -> Void) {
        eventsLogger.logFilesDirURL()
            .subscribe(onSuccess: { url in
                callback(url)
            }, onFailure: { error in
                callback(nil)
                cdPrint("events logs archive error: \(error)")
            })
            .disposed(by: bag)
    }
    
    func registerInternalEventObserver(reportCallback: @escaping (_ eventCode: Int, _ info: String) -> Void) {
        self.internalEventReporter = reportCallback
    }
    
    func getUserProperties() -> [String : String] {
        return _userProperty
    }
}

// MARK: - private functions
private extension Manager {
    
    func setupOberving() {
        
        syncServerTrigger
            .debounce(.seconds(1), scheduler: rxConsumeScheduler)
            .subscribe(onNext: { [weak self] _ in
                self?.syncServerTime()
            })
            .disposed(by: bag)
        
        // Only observe app lifecycle notifications if not in extension
        if !isRunningInAppExtension {
            var activeNoti = NotificationCenter.default.rx.notification(UIApplication.didBecomeActiveNotification)
            
            if sharedApplication?.applicationState == .active {
                activeNoti = activeNoti.startWith(.init(name: UIApplication.didBecomeActiveNotification))
            }
            
            activeNoti
                .subscribe(onNext: { [weak self] _ in
                    self?.onAppActive()
                })
                .disposed(by: bag)
            
            NotificationCenter.default.rx.notification(UIApplication.didEnterBackgroundNotification)
                .subscribe(onNext: { [weak self] _ in
                    guard let `self` = self else { return }
                    //这里log fg和上传events任务并行关系改为前后依赖关系
                    _ = self.logForegroundDuration()
                        .catchAndReturn(())
                        .map { self.consumeEvents() }
                        .subscribe()
                    self._serverTimeSynced.accept(false)
                    self.invalidFgAccumulateTimer()
                })
                .disposed(by: bag)
        } else {
            onAppActive()
        }
    }
    
    func onAppActive() {
        syncServerTrigger.onNext(())
        // fg计时器
        setupFgAccumulateTimer()
    }
    
    func syncServerTime() {
        //有网时立即同步，无网时等待有网后同步
        ntwkMgr.reachableObservable.filter { $0 }.map { _ in }.take(1).asSingle()
            .flatMap { [weak self] _ -> Single<Int64> in
                guard let `self` = self else { return Observable.empty().asSingle()}
                return self.ntwkMgr.syncServerTime()
            }
            .observe(on: rxNetworkScheduler)
            .subscribe(onSuccess: { [weak self] ms in
                self?.serverInitialMs = ms
                self?._serverTimeSynced.accept(true)
            })
            .disposed(by: bag)
    }
    
    func logForegroundDuration() -> Single<Void> {
        return _logEvent(GuruAnalytics.fgEvent.name, parameters: [GuruAnalytics.fgEvent.paramKeyType.duration.rawValue : fgDurationMs()])
            .observe(on: MainScheduler.asyncInstance)
            .do(onSuccess: { _ in
                UserDefaults.fgAccumulatedDuration = 0
            })
    }
    
    func clearOutdatedEventsIfNeeded() {
        
        /// 1. 删除过期的数据
        serverNowMsSingle
            .flatMap({ [weak self] serverNowMs -> Single<[Entity.EventRecord]> in
                guard let `self` = self else {
                    return .error(NSError(domain: "com.guru.analytics.manager",
                                          code: 0,
                                          userInfo: [NSLocalizedDescriptionKey : "Manager released"]
                                         ))
                }
                let earlierThan: Int64 = serverNowMs - self.eventExpiredIntervel.int64Ms
                return self.db.fetchOutdatedEventRecords(earlierThan: earlierThan)
            })
            .flatMap({ [weak self] records in
                return self?.db.deleteEventRecords(records.map { $0.recordId })
                    .map { _ in records.count } ?? 
                    .error(NSError(domain: "com.guru.analytics.manager",
                                   code: 0,
                                   userInfo: [NSLocalizedDescriptionKey : "Manager released"]
                                  ))
            })
            .catch({ error in
                cdPrint("remove outdated records error: \(error)")
                return .just(0)
            })
            .subscribe(onSuccess: { [weak self] deletedCount in
                UserDefaults.deletedEventsCount += deletedCount
                self?.outdatedEventsCleared.onNext(true)
            })
            .disposed(by: bag)
    }
    
    func logFirstOpenIfNeeded() {
        
        if let t = UserDefaults.defaults?.value(forKey: UserDefaults.firstOpenTimeKey),
           let firstOpenTimeMs = t as? Int64 {
            setUserProperty("\(firstOpenTimeMs)", forName: PropertyName.firstOpenTime.rawValue)
        } else {
            /// log first open event
            logEvent(GuruAnalytics.firstOpenEvent.name, parameters: nil, priority: .EMERGENCE)
            
            /// save first open time
            /// set to userProperty
            let firstOpenAt = Date()
            
            let saveFirstOpenTime = { [weak self] (ms: Int64) -> Void in
                UserDefaults.defaults?.set(ms, forKey: UserDefaults.firstOpenTimeKey)
                self?.setUserProperty("\(ms)", forName: PropertyName.firstOpenTime.rawValue)
            }
            
            serverNowMsSingle
                .subscribe(onSuccess: { _ in
                    let latency = Date().timeIntervalSince(firstOpenAt)
                    let adjustedFirstOpenTimeMs = self.serverInitialMs - latency.int64Ms
                    saveFirstOpenTime(adjustedFirstOpenTimeMs)
                }, onFailure: { error in
                    cdPrint("waiting for server time syncing error: \(error)")
                    saveFirstOpenTime(firstOpenAt.timeIntervalSince1970.int64Ms)
                })
                .disposed(by: bag)
        }
        
    }
    
    func _logEvent(_ eventName: String, parameters: [String : Any]?, priority: Entity.EventRecord.Priority = .DEFAULT) -> Single<Void> {
        eventsLogger.verbose(#function + " eventName: \(eventName)" + " params: \(parameters?.jsonString() ?? "")")
        return { [weak self] () -> Single<Void> in
            guard let `self` = self else { return Observable<Void>.empty().asSingle() }
            return self.serverNowMsSingle
                .flatMap { self.constructEvent(eventName, parameters: parameters, timestamp: $0, priority: priority) }
                .flatMap { self.db.addEventRecords($0) }
                .do(onSuccess: { _ in
                    self.accumulateLoggedEventsCount(1)
                    UserDefaults.totalEventsCount += 1
                    self.eventsLogger.verbose("log event success")
                }, onError: { error in
                    self.eventsLogger.error("log event error: \(error)")
                })
        }()
    }

    func logSDKInitStart() {
        _logEvent(GuruAnalytics.sdkInitStartEvent.name, parameters: [
            GuruAnalytics.sdkInitStartEvent.paramKeyType.totalEvents.rawValue : UserDefaults.totalEventsCount,
            GuruAnalytics.sdkInitStartEvent.paramKeyType.deletedEvents.rawValue : UserDefaults.deletedEventsCount,
            GuruAnalytics.sdkInitStartEvent.paramKeyType.uploadedEvents.rawValue : UserDefaults.uploadedEventsCount,
        ], priority: .HIGH)
        .subscribe()
        .disposed(by: bag)
    }
    
    func logSDKInitComplete() {
        _logEvent(GuruAnalytics.sdkInitCompleteEvent.name, parameters: [
            GuruAnalytics.sdkInitCompleteEvent.paramKeyType.duration.rawValue : Date().msSince1970 - startAt.msSince1970,
        ], priority: .HIGH)
        .subscribe()
        .disposed(by: bag)
    }
    func logSessionStart() {
        _logEvent(GuruAnalytics.sessionStartEvent.name, parameters: nil, priority: .HIGH)
            .subscribe()
            .disposed(by: bag)
    }
}

// MARK: - 轮询上传相关
private extension Manager {
    
    typealias TaskCallback = (() -> Void)
    typealias Task = ((@escaping TaskCallback, Int) -> Void)
    
    func performBackgroundTask(task: @escaping Task) -> Single<Void> {
        return Single.create { [weak self] subscriber in
            var backgroundTaskID: UIBackgroundTaskIdentifier?
            
            let stopTaskHandler = {
                ///结束任务时需要找到对应的 dispose 取消当前任务
                guard let taskId = backgroundTaskID,
                      let disposable = self?.taskKeyDisposableMap[taskId.rawValue] else {
                    return
                }
                cdPrint("[performBackgroundTask] performBackgroundTask expired: \(backgroundTaskID?.rawValue ?? -1)")
                disposable.dispose()
            }
            
            // Only use background task in main app, not in extension
            if let application = self?.sharedApplication {
                // Request the task assertion and save the ID.
                backgroundTaskID = application.beginBackgroundTask(withName: "com.guru.analytics.manager.background.task", expirationHandler: {
                    // End the task if time expires.
                    self?.eventsLogger.verbose("performBackgroundTask expirationHandler: \(backgroundTaskID?.rawValue ?? -1)")
                    stopTaskHandler()
                })
            }
            
            self?.eventsLogger.verbose("performBackgroundTask start: \(backgroundTaskID?.rawValue ?? -1)")
            var taskIdValue: Int?
            if let taskID = backgroundTaskID {
                taskIdValue = taskID.rawValue
            } else if self?.isRunningInAppExtension == true {
                // If we're in an extension or couldn't start background task, just execute the task
                taskIdValue = UUID().hashValue
            }
            
            if let id = taskIdValue {
                task({
                    self?.eventsLogger.verbose("performBackgroundTask finish: \(id)")
                    subscriber(.success(()))
                }, id)
            }
            
            return Disposables.create {
                if var taskID = backgroundTaskID,
                   let application = self?.sharedApplication {
                    self?.eventsLogger.verbose("performBackgroundTask dispose: \(taskID.rawValue)")
                    application.endBackgroundTask(taskID)
                    taskID = .invalid
                }
                backgroundTaskID = nil
            }
        }
        .subscribe(on: rxBgWorkScheduler)
    }
    
    /// 上传数据库中的event
    func consumeEvents() {
        guard GuruAnalytics.enableUpload else {
            return
        }
        self.eventsLogger.verbose("consumeEvents start")
        performBackgroundTask { [weak self] callback, taskId in
            
            guard let `self` = self else { return }
            cdPrint("consumeEvents start background task")
            // 等待清理过期记录完成
            let disposable = outdatedEventsCleared
                .filter { $0 }
                .take(1)
                .observe(on: rxBgWorkScheduler)
                .asSingle()
                .flatMap { _ -> Single<[Entity.EventRecord]> in
                    self.eventsLogger.verbose("consumeEvents fetchEventRecordsToUpload")
                    ///step1:  拉取数据库记录
                    return self.db.fetchEventRecordsToUpload(limit: self.maxEventFetchingCount)
                }
                .map { records -> [[Entity.EventRecord]] in
                    /// step2:  将event数组分割成若干批次，numberOfCountPerConsume个一批
                    /// self.eventsLogger.verbose("consumeEvents fetchEventRecordsToUpload")
                    self.eventsLogger.verbose("consumeEvents fetchEventRecordsToUpload result: \(records.count)")
                    return records.chunked(into: self.numberOfCountPerConsume)
                }
                .flatMap({ batches -> Single<[[Entity.EventRecord]]> in
                    
                    guard batches.count > 0 else { return .just([]) }
                    
                    /// 监听网络信号
                    return self.ntwkMgr.reachableObservable.filter { $0 }
                        .take(1).asSingle()
                        .map { _ in batches }
                })
                .map { batches -> [Single<[String]>] in
                    /// step3: 转为批次上传任务
                    self.eventsLogger.verbose("consumeEvents uploadEvents")
                    return batches.map { records in
                        return self.ntwkMgr.uploadEvents(records)
                            .do(onSuccess: { t in
                                self.eventsLogger.verbose("consumeEvents upload events succeed: \(t.eventsJson)")
                            })
                            .catch({ error in
                                self.eventsLogger.error("consumeEvents upload events error: \(error)")
                                // 上传失败，移除对应的缓存ID
                                let recordIds = records.map { $0.recordId }
                                return self.db.resetTransitionStatus(for: recordIds)
                                    .map { _ in ([], "") }
                            })
                            .map { $0.recordIDs }
                    }
                }
                .flatMap { uploadBatches -> Single<[String]> in
                    guard uploadBatches.count > 0 else { return .just([]) }
                    /// 合并上传结果
                    return Observable.from(uploadBatches)
                        .merge()
                        .toArray().map { batches -> [String] in batches.flatMap { $0 } }
                }
                .flatMap { recordIDs -> Single<Void> in
                    self.accumulateUploadedEventsCount(recordIDs.count)
                    UserDefaults.uploadedEventsCount += recordIDs.count
                    /// step4:  删除数据库中对应记录
                    return self.db.deleteEventRecords(recordIDs)
                        .catch { error in
                            cdPrint("consumeEvents delete events from DB error: \(error)")
                            return .just(())
                        }
                }
                .observe(on: self.rxBgWorkScheduler)
                .subscribe(onFailure: { error in
                    cdPrint("consumeEvents error: \(error)")
                }, onDisposed: { [weak self] in
                    self?.taskKeyDisposableMap.removeValue(forKey: taskId)
                    cdPrint("consumeEvents onDisposed")
                    callback()
                })
            
            taskKeyDisposableMap[taskId] = disposable
        }
        .subscribe()
        .disposed(by: bag)
        
    }
    
    func startPollingUpload() {
        pollingUploadTask?.dispose()
        pollingUploadTask = nil
        
        // 每scheduleInterval时间间隔启动一次，立即启动一次
        let timer = Observable<Int>.timer(.seconds(0), period: .milliseconds(Int(scheduleInterval.int64Ms)),
                                          scheduler: rxConsumeScheduler)
            .do(onNext: { _ in
                cdPrint("consumeEvents timer")
            })
        
        // 每满numberOfCountPerConsume个数启动一次，立即启动一次
        let counter = db.uploadableEventRecordCountOb()
            .distinctUntilChanged()
            .compactMap({ [weak self] count -> Int? in
                cdPrint("consumeEvents uploadableEventRecordCountOb count: \(count) numberOfCountPerConsume: \(self?.numberOfCountPerConsume)")
                guard let `self` = self,
                      count >= self.numberOfCountPerConsume else { return nil }
                return count
            })
            .map { _ in }
            .startWith(())
        
        pollingUploadTask = Observable.combineLatest(timer, counter)
            .throttle(.seconds(1), scheduler: rxConsumeScheduler)
            .flatMap({ [weak self] t -> Single<(Int, Void)> in
                guard let `self` = self else { return .just(t) }
                return Observable.combineLatest(self.db.hasFgEventRecord().asObservable(), self.db.uploadableEventRecordCount().asObservable())
                    .take(1).asSingle()
                    .flatMap({ (hasFgEventInDb, eventsCount) -> Single<(Int, Void)> in
                        guard !hasFgEventInDb, eventsCount > 0 else {
                            return .just(t)
                        }
                        return self.logForegroundDuration().catchAndReturn(()).map({ _ in t })
                })
            })
            .subscribe(onNext: { [weak self] (timer, counter) in
                self?.consumeEvents()
            })
    }
    
    func setupPollingUpload() {
        reschedulePollingTrigger
            .debounce(.seconds(1), scheduler: rxConsumeScheduler)
            .subscribe(onNext: { [weak self] _ in
                self?.startPollingUpload()
            })
            .disposed(by: bag)
    }
    
    func logFirstFgEvent() {
        _ = Single.just(()).delay(.milliseconds(500), scheduler: MainScheduler.asyncInstance)
            .flatMap({ [weak self] _ in
                self?.logForegroundDuration() ?? .just(())
            })
            .subscribe()
    }
}

// MARK: - fg相关
private extension Manager {
    
    func setupFgAccumulateTimer() {
        invalidFgAccumulateTimer()
        fgStartAtAbsoluteMs = Date.absoluteTimeMs
        fgAccumulateTimer = Observable<Int>.timer(.seconds(0), period: .seconds(1), scheduler: MainScheduler.asyncInstance)
            .subscribe(onNext: { [weak self] _ in
                guard let `self` = self else { return }
                UserDefaults.fgAccumulatedDuration = self.fgDurationMs()
            }, onDisposed: {
                cdPrint("fg accumulate timer disposed")
            })
    }
    
    func invalidFgAccumulateTimer() {
        fgAccumulateTimer?.dispose()
        fgAccumulateTimer = nil
    }
    
    /// 前台停留时长
    func fgDurationMs() -> Int64 {
        let slice = Date.absoluteTimeMs - fgStartAtAbsoluteMs
        fgStartAtAbsoluteMs = Date.absoluteTimeMs
//        cdPrint("accumulate fg duration: \(slice)")
        let totalDuration = UserDefaults.fgAccumulatedDuration + slice
//        cdPrint("total fg duration: \(totalDuration)")
        return totalDuration
    }
}

extension Manager: GuruAnalyticsNetworkErrorReportDelegate {
    func reportError(networkError: GuruAnalyticsNetworkError) {
        
        enum UserInfoKey: String, Encodable {
            case httpCode = "h_c"
            case errorCode = "e_c"
            case url, msg
        }
        
        let errorCode = networkError.internalErrorCategory.rawValue
        let userInfo = (networkError.originError as NSError).userInfo
        var info: [UserInfoKey : String] = [
            .url : (userInfo[NSURLErrorFailingURLStringErrorKey] as? String) ?? "",
            .msg : networkError.originError.localizedDescription,
        ]
        
        if let httpCode = networkError.httpStatusCode {
            info[.httpCode] = "\(httpCode)"
        } else {
            info[.errorCode] = "\((networkError.originError as NSError).code)"
        }
        
        info = info.compactMapValues { $0.isEmpty ? nil : $0 }
        
        let jsonString = info.asString ?? ""
        DispatchQueue.main.async { [weak self] in
            self?.internalEventReporter?(errorCode, jsonString)
        }
    }
}
