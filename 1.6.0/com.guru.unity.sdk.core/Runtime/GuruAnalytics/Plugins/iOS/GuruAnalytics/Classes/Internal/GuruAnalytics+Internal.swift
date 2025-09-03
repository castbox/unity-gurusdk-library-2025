//
//  GuruAnalytics+Internal.swift
//  Pods
//
//  Created by mayue on 2022/11/18.
//

import Foundation

internal extension GuruAnalytics {
    
    ///built-in user property keys
    enum PropertyName: String {
        case deviceId
        case uid
        case adjustId
        case adId
        case firebaseId
        case screen = "screen_name"
        case firstOpenTime = "first_open_time"
        case appsflyerId
    }
    
    ///built-in events
    static let fgEvent: EventProto = {
        var e = EventProto(paramKeyType: FgEventParametersKeys.self, name: "guru_engagement")
        return e
    }()
    
    static let firstOpenEvent: EventProto = {
        var e = EventProto(paramKeyType: FgEventParametersKeys.self, name: "first_open")
        return e
    }()
    
    static let sdkInitStartEvent: EventProto = {
        var e = EventProto(paramKeyType: SDKEventParametersKeys.self, name: "guru_sdk_init_start")
        return e
    }()
    
    static let sdkInitCompleteEvent: EventProto = {
        var e = EventProto(paramKeyType: SDKEventParametersKeys.self, name: "guru_sdk_init_complete")
        return e
    }()
    
    static let sessionStartEvent: EventProto = {
        var e = EventProto(paramKeyType: DefaultEventParametersKeys.self, name: "session_start")
        return e
    }()
    
    class func setUserProperty(_ value: String?, forName name: PropertyName) {
        setUserProperty(value, forName: name.rawValue)
    }
    
}

internal extension GuruAnalytics {
    
    struct EventProto<ParametersKeys> {
        var paramKeyType: ParametersKeys.Type
        var name: String
    }
    
    enum FgEventParametersKeys: String {
        case duration
    }
    
    enum SDKEventParametersKeys: String {
        case totalEvents = "total_events"
        case deletedEvents = "deleted_events"
        case uploadedEvents = "uploaded_events"
        case duration
    }
    
    enum DefaultEventParametersKeys {
    }
}

internal extension GuruAnalytics {
    
    ///built-in event parameters
    enum BuiltinParametersKeys: String, CaseIterable {
        case screenName = "screen_name"
        case sessionNo = "session_number"
        case sessionId = "session_id"
    }
}
