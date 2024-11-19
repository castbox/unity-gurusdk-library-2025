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
    }
    
    ///built-in events
    static let fgEvent: EventProto = {
        var e = EventProto(paramKeyType: FgEventParametersKeys.self, name: "fg")
        return e
    }()
    
    static let firstOpenEvent: EventProto = {
        var e = EventProto(paramKeyType: FgEventParametersKeys.self, name: "first_open")
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
    
}
