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
}
