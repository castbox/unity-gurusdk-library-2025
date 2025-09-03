//
//  DBEntities.swift
//  Alamofire
//
//  Created by mayue on 2022/11/4.
//

import Foundation
import CryptoSwift

internal enum Entity {
    
}

extension Entity {
    struct EventRecord: Codable {
        
        enum Priority: Int, Codable {
            case EMERGENCE = 0
            case HIGH = 5
            case DEFAULT = 10
            case LOW = 15
        }
        
        enum TransitionStatus: Int, Codable {
            case idle = 0
            case instransition = 1
        }
        
        let recordId: String
        let eventName: String
        let eventJson: String
        ///单位毫秒
        let timestamp: Int64
        let priority: Priority
        let transitionStatus: TransitionStatus
        
        init(eventName: String, event: Entity.Event, priority: Priority = .DEFAULT, transitionStatus: TransitionStatus = .idle) {
            let now = Date()
            let eventJson = event.asString ?? ""
            if eventJson.isEmpty {
                cdPrint("[WARNING] error for convert event to json")
            }
            self.recordId = "\(eventName)\(eventJson)\(now.timeIntervalSince1970)\(Int.random(in: Int.min...Int.max))".md5()
            self.eventName = eventName
            self.eventJson = eventJson
            self.timestamp = event.timestamp
            self.priority = priority
            self.transitionStatus = transitionStatus
        }
        
        init(recordId: String, eventName: String, eventJson: String, timestamp: Int64, priority: Int, transitionStatus: Int) {
            self.recordId = recordId
            self.eventName = eventName
            self.eventJson = eventJson
            self.timestamp = timestamp
            self.priority = .init(rawValue: priority) ?? .DEFAULT
            self.transitionStatus = .init(rawValue: transitionStatus) ?? .idle
        }
        
        enum CodingKeys: String, CodingKey {
            case recordId
            case eventName
            case eventJson
            case timestamp
            case priority
            case transitionStatus
        }
        
        static func createTableSql(with name: String) -> String {
            return """
                CREATE TABLE IF NOT EXISTS \(name)(
                \(CodingKeys.recordId.rawValue) TEXT UNIQUE NOT NULL PRIMARY KEY,
                \(CodingKeys.eventName.rawValue) TEXT NOT NULL,
                \(CodingKeys.eventJson.rawValue) TEXT NOT NULL,
                \(CodingKeys.timestamp.rawValue) INTEGER,
                \(CodingKeys.priority.rawValue) INTEGER,
                \(CodingKeys.transitionStatus.rawValue) INTEGER);
                """
        }
        
        func insertSql(to tableName: String) -> String {
            return "INSERT INTO \(tableName) VALUES ('\(recordId)', '\(eventName)', '\(eventJson)', '\(timestamp)', '\(priority.rawValue)', '\(transitionStatus.rawValue)')"
        }
    }
    
}

extension Entity {
    struct Event: Codable {
        ///客户端中记录此事件的时间（采用世界协调时间，毫秒为单位）
        let timestamp: Int64
        let event: String
        let userInfo: UserInfo
        let param: [String: EventValue]
        let properties: [String: String]
        let eventId: String
        
        enum CodingKeys: String, CodingKey {
            case timestamp
            case userInfo = "info"
            case event, param, properties
            case eventId
        }
        
        init(timestamp: Int64, event: String, userInfo: UserInfo, parameters: [String : Any], properties: [String : String]) throws {
            guard let normalizedEvent = Self.normalizeKey(event),
                  normalizedEvent == event else {
                cdPrint("drop event because of illegal event name: \(event)")
                cdPrint("standard: https://developers.google.com/android/reference/com/google/firebase/analytics/FirebaseAnalytics.Event")
                throw NSError(domain: "cunstrcting event error", code: 0, userInfo: [NSLocalizedDescriptionKey : "illegal event name: \(event)"])
            }
            self.eventId = UUID().uuidString.lowercased()
            self.timestamp = timestamp
            self.event = normalizedEvent
            self.userInfo = userInfo
            self.param = Self.normalizeParameters(parameters)
            self.properties = properties
        }
        
        static let maxParametersCount = 25
        static let maxKeyLength = 40
        static let maxParameterStringValueLength = 128
        
        static func normalizeParameters(_ parameters: [String : Any]) -> [String : EventValue] {
            var params = [String : EventValue]()
            var allParams = parameters;
            
            GuruAnalytics.BuiltinParametersKeys.allCases.forEach { paramKey in
                if let value = allParams.removeValue(forKey: paramKey.rawValue) {
                    params[paramKey.rawValue] = normalizeValue(value);
                }
            }
            
            var count = 0
            allParams.sorted(by: { $0.key < $1.key }).forEach({ key, value in
                
                guard count < maxParametersCount else {
                    cdPrint("too many parameters")
                    cdPrint("standard: https://developers.google.com/android/reference/com/google/firebase/analytics/FirebaseAnalytics.Event")
                    return
                }
                
                guard let normalizedKey = normalizeKey(key),
                      normalizedKey == key else {
                    cdPrint("drop event parameter because of illegal key: \(key)")
                    cdPrint("standard: https://developers.google.com/android/reference/com/google/firebase/analytics/FirebaseAnalytics.Event")
                    return
                }
                
                params[normalizedKey] = normalizeValue(value)
                
                count += 1
            })
            
            return params
        }
        
        static func normalizeValue(_ value: Any) -> EventValue {
            
            var preprocessedValue = value
            
            if let val = preprocessedValue as? NSNumber {
                preprocessedValue = val.numricValue
            }
            
            let eventValue: EventValue
            if let value = preprocessedValue as? String {
                eventValue = Entity.EventValue(stringValue: normalizeStringValue(String(value.prefix(maxParameterStringValueLength))))
            } else if let value = preprocessedValue as? Int {
                eventValue = Entity.EventValue(longValue: Int64(value))
            } else if let value = preprocessedValue as? Int64 {
                eventValue = Entity.EventValue(longValue: value)
            } else if let value = preprocessedValue as? Double {
                eventValue = Entity.EventValue(doubleValue: value)
            } else {
                eventValue = Entity.EventValue(stringValue: normalizeStringValue(String("\(value)".prefix(maxParameterStringValueLength))))
            }
            return eventValue
        }
        
        static func normalizeStringValue(_ value: String) -> String {
            let normalizedString = value.replacingOccurrences(of: "'", with: "''")
            cdPrint("original string value: \(value)")
            cdPrint("normalized string value: \(normalizedString)")
            return normalizedString
        }
        
        static func normalizeKey(_ key: String) -> String? {
            var mutableKey = key
            
            while let first = mutableKey.first,
                    !first.isLetter {
                _ = mutableKey.removeFirst()
            }
            
            var normalizedKey = ""
            var count = 0
            mutableKey.forEach { c in
                guard count < maxKeyLength,
                      c.isAlphabetic || c.isDigit || c == "_" else { return }
                normalizedKey.append(c)
                count += 1
            }
            
            return normalizedKey.isEmpty ? nil : normalizedKey
        }
    }
    
    ///用户信息
    struct UserInfo: Codable {
        ///中台ID。只在未获取到uid时可以为空
        let uid: String?
        ///设备ID（用户的设备ID，iOS取用户的IDFV或UUID，Android取androidID）
        let deviceId: String?
        ///adjust_id。只在未获取到adjust时可以为空
        let adjustId: String?
        ///广告 ID/广告标识符 (IDFA)
        let adId: String?
        ///用户的pseudo_id
        let firebaseId: String?
        ///appsFlyerId
        let appsflyerId: String?
        
        ///IDFV
        let vendorId: String? = UIDevice().identifierForVendor?.uuidString
        
        enum CodingKeys: String, CodingKey {
            case deviceId
            case uid
            case adjustId
            case adId
            case firebaseId
            case vendorId
            case appsflyerId
        }
    }
    
    // 参数对应的值
    struct EventValue: Codable {
        let stringValue: String?    // 事件参数的字符串值
        let longValue: Int64?         // 事件参数的整数值
        let doubleValue: Double?    // 事件参数的小数值。注意：APP序列化成JSON时，注意不要序列化成科学计数法
        
        init(stringValue: String? = nil, longValue: Int64? = nil, doubleValue: Double? = nil) {
            self.stringValue = stringValue
            self.longValue = longValue
            self.doubleValue = doubleValue
        }
        
        enum CodingKeys: String, CodingKey {
            case stringValue = "s"
            case longValue = "i"
            case doubleValue = "d"
        }
    }
}

extension Entity {
    struct SystemTimeResult: Codable {
        let data: Int64
    }
}

extension Entity {
    struct Session {
        let id: UUID = UUID()
        
        var sessionId: Int {
            return id.uuidString.hash
        }
    }
    
    struct SessionNumber: Codable {
        var number: Int
        let createdAtMs: Int64
        
        static func createNumber() -> SessionNumber {
            return SessionNumber(number: 0, createdAtMs: Date().msSince1970)
        }
    }
}

