//
//  UserDefaults.swift
//  GuruAnalytics
//
//  Created by mayue on 2022/11/21.
//

import Foundation

internal enum UserDefaults {
    
    static let defaults = Foundation.UserDefaults(suiteName: "com.guru.guru_analytics_lib")
    
    static var eventsServerHost: String? {
        
        get {
            return defaults?.value(forKey: eventsServerHostKey) as? String
        }
        
        set {
            var host = newValue
            let h_sch = "http://"
            let hs_sch = "https://"
            host?.deletePrefix(h_sch)
            host?.deletePrefix(hs_sch)
            host?.trimmed(in: .whitespacesAndNewlines.union(.init(charactersIn: "/")))
            defaults?.set(host, forKey: eventsServerHostKey)
        }
        
    }
    
    static var fgAccumulatedDuration: Int64 {
        get {
            return defaults?.value(forKey: fgDurationKey) as? Int64 ?? 0
        }
        
        set {
            defaults?.set(newValue, forKey: fgDurationKey)
        }
    }
    
    static var totalEventsCount: Int {
        get {
            return defaults?.value(forKey: totalEventsCountKey) as? Int ?? 0
        }
        
        set {
            defaults?.set(newValue, forKey: totalEventsCountKey)
        }
    }
    
    static var deletedEventsCount: Int {
        get {
            return defaults?.value(forKey: deletedEventsCountKey) as? Int ?? 0
        }
        
        set {
            defaults?.set(newValue, forKey: deletedEventsCountKey)
        }
    }
    
    static var uploadedEventsCount: Int {
        get {
            return defaults?.value(forKey: uploadedEventsCountKey) as? Int ?? 0
        }
        
        set {
            defaults?.set(newValue, forKey: uploadedEventsCountKey)
        }
    }
    
    static var sessionNumber: Entity.SessionNumber {
        get {
            let jsonString = defaults?.value(forKey: sessionNumberKey) as? String ?? ""
            let sessionNumber = JSONDecoder().decodeObject(Entity.SessionNumber.self, from: jsonString)
            ?? Entity.SessionNumber.createNumber()
            return sessionNumber
        }
        set {
            let jsonString = newValue.asString
            defaults?.setValue(jsonString, forKey: sessionNumberKey)
        }
    }
}

extension UserDefaults {
    
    static var firstOpenTimeKey: String {
        return "app.first.open.timestamp"
    }
    
    static var dbVersionKey: String {
        return "db.version"
    }
    
    static var hostsMapKey: String {
        return "hosts.map"
    }
    
    static var eventsServerHostKey: String {
        return "events.server.host"
    }
    
    static var fgDurationKey: String {
        return "fg.duration.ms"
    }
    
    static var totalEventsCountKey: String {
        return "events.recorded.total.count"
    }
    
    static var deletedEventsCountKey: String {
        return "events.deleted.count"
    }
    
    static var uploadedEventsCountKey: String {
        return "events.uploaded.count"
    }
    
    static var sessionNumberKey: String {
        return "session.number"
    }
    
}
