//
//  GuruAnalytics.swift
//  GuruAnalytics_iOS
//
//  Created by mayue on 2022/11/4.
//

import Foundation

public class GuruAnalytics: NSObject {
    
    internal static var uploadPeriodInSecond: Double = 60
    internal static var batchLimit: Int = 25
    internal static var eventExpiredSeconds: Double = 7 * 24 * 60 * 60
    internal static var initializeTimeout: Double = 5
    internal static var saasXAPPID = ""
    internal static var saasXDEVICEINFO = ""
    internal static var loggerDebug = true
    internal static var enableUpload = true
    
    /// 初始化设置
    /// - Parameters:
    ///   - uploadPeriodInSecond: 批量上传周期，单位秒
    ///   - batchLimit: 批量条数
    ///   - eventExpiredSeconds: 数据过期时间，单位秒
    ///   - initializeTimeout: 初始化后等待user id/device id/firebase pseudo id等属性超时时间，单位秒
    ///   - saasXAPPID: 中台接口header中的X-APP-ID
    ///   - saasXDEVICEINFO: 中台接口header中的X-DEVICE-INFO
    ///   - loggerDebug: 开启控制台输出debug信息
    @objc
    public class func initializeLib(uploadPeriodInSecond: Double = 60,
                                    batchLimit: Int = 25,
                                    eventExpiredSeconds: Double = 7 * 24 * 60 * 60,
                                    initializeTimeout: Double = 5,
                                    saasXAPPID: String,
                                    saasXDEVICEINFO: String,
                                    loggerDebug: Bool = true,
                                    guruSDKVersion: String) {
        Self.uploadPeriodInSecond = uploadPeriodInSecond
        Self.batchLimit = batchLimit
        Self.eventExpiredSeconds = eventExpiredSeconds
        Self.initializeTimeout = initializeTimeout
        Self.saasXAPPID = saasXAPPID
        Self.saasXDEVICEINFO = saasXDEVICEINFO
        Self.loggerDebug = loggerDebug
        Constants.guruSDKVersion = guruSDKVersion
        _ = Manager.shared
    }
    
    /// 记录event
    @objc
    public class func logEvent(_ name: String, parameters: [String : Any]?) {
        Manager.shared.logEvent(name, parameters: parameters)
    }
    
    /// 中台ID。只在未获取到uid时可以为空
    @objc
    public class func setUserID(_ userID: String?) {
        setUserProperty(userID, forName: .uid)
    }
    
    /// 设备ID（用户的设备ID，iOS取用户的IDFV或UUID，Android取androidID）
    @objc
    public class func setDeviceId(_ deviceId: String?) {
        setUserProperty(deviceId, forName: .deviceId)
    }
    
    /// adjust_id。只在未获取到adjust时可以为空
    @objc
    public class func setAdjustId(_ adjustId: String?) {
        setUserProperty(adjustId, forName: .adjustId)
    }
    
    /// 广告 ID/广告标识符 (IDFA)
    @objc
    public class func setAdId(_ adId: String?) {
        setUserProperty(adId, forName: .adId)
    }
    
    /// 用户的pseudo_id
    @objc
    public class func setFirebaseId(_ firebaseId: String?) {
        setUserProperty(firebaseId, forName: .firebaseId)
    }
    
    /// 设置appsflyerId
    @objc
    public class func setAppFlyersId(_ appFlyersId: String?) {
        setUserProperty(appFlyersId, forName: .appsflyerId)
    }
    
    /// screen name
    @objc
    public class func setScreen(_ name: String) {
        Manager.shared.setScreen(name)
    }
    
    /// 设置userproperty
    @objc
    public class func setUserProperty(_ value: String?, forName name: String) {
        Manager.shared.setUserProperty(value ?? "", forName: name)
    }
    
    /// 移除userproperty
    @objc
    public class func removeUserProperties(forNames names: [String]) {
        Manager.shared.removeUserProperties(forNames: names)
    }
    
    /// 获取events相关日志文件zip包
    /// zip解压密码：Castbox123
    @available(*, deprecated, renamed: "eventsLogsDirectory", message: "废弃，使用eventsLogsDirectory方法获取日志文件目录URL")
    @objc
    public class func eventsLogsArchive(_ callback: @escaping (_ url: URL?) -> Void) {
        Manager.shared.eventsLogsArchive(callback)
    }
    
    /// 获取events相关日志文件目录
    @objc
    public class func eventsLogsDirectory(_ callback: @escaping (_ url: URL?) -> Void) {
        Manager.shared.eventsLogsDirURL(callback)
    }
    
    /// 更新events上报服务器域名
    /// host: 服务器域名，例如：“abc.bbb.com”,  "https://abc.bbb.com", "http://abc.bbb.com"
    @objc
    public class func setEventsUploadEndPoint(host: String?) {
        UserDefaults.eventsServerHost = host
    }
    
    /// 获取events统计数据
    /// - Parameter callback: 数据回调
    ///   - callback parameters:
    ///     - uploadedEventsCount: 上传后端成功event条数
    ///     - loggedEventsCount: 已记录event总条数
    @objc
    @available(*, deprecated, message: "used for debug, will be removed on any future released versions")
    public class func debug_eventsStatistics(_ callback: @escaping (_ uploadedEventsCount: Int, _ loggedEventsCount: Int) -> Void) {
        Manager.shared.debug_eventsStatistics(callback)
    }
    
    /// 将内部事件信息上报给应用层
    /// - Parameter reportCallback: 数据回调
    ///   - callback parameters:
    ///     - eventCode: 事件代码
    ///     - info: 事件相关信息
    @objc
    public class func registerInternalEventObserver(reportCallback: @escaping (_ eventCode: Int, _ info: String) -> Void) {
        Manager.shared.registerInternalEventObserver(reportCallback: reportCallback)
    }
    
    /// 获取当前user property
    @objc
    public class func getUserProperties() -> [String : String] {
        return Manager.shared.getUserProperties()
    }

    /// 设置上传开关，默认为true
    /// true - 开启上传
    /// false - 关闭上传
    @objc
    public class func setEnableUpload(isOn: Bool = true) -> Void {
        enableUpload = isOn
    }

    /// 设置中台库版本
    @objc
    public class func setGuruSDKVersion(_ version: String) -> Void {
        Constants.guruSDKVersion = version
    }
}
