//
//  Constants.swift
//  AgoraChatRoom
//
//  Created by LXH on 2019/11/27.
//  Copyright © 2019 CavanSu. All rights reserved.
//

import UIKit

internal struct Constants {
    
    private static let appVersion: String = {
        guard let infoDict = Bundle.main.infoDictionary,
              let currentVersion = infoDict["CFBundleShortVersionString"] as? String else {
            return ""
        }
        return currentVersion
    }()
    
    private static let appBundleIdentifier: String = {
        guard let infoDictionary = Bundle.main.infoDictionary,
              let shortVersion = infoDictionary["CFBundleIdentifier"] as? String else {
            return ""
        }
        return shortVersion
    }()
    
    private static let preferredLocale: Locale = {
        guard let preferredIdentifier = Locale.preferredLanguages.first else {
            return Locale.current
        }
        return Locale(identifier: preferredIdentifier)
    }()
    
    private static let countryCode: String = {
        return preferredLocale.regionCode?.uppercased() ?? ""
    }()
    
    private static let timeZone: String = {
        return TimeZone.current.identifier
    }()
    
    private static let languageCode: String = {
        return preferredLocale.languageCode ?? ""
    }()
    
    private static let localeCode: String = {
        return preferredLocale.identifier
    }()
    
    private static let modelName: String = {
        return platform().deviceType.rawValue
    }()
    
    private static let model: String = {
        return hardwareString()
    }()
    
    private static let systemVersion: String = {
        return UIDevice.current.systemVersion
    }()
    
    private static let screenSize: (w: CGFloat, h: CGFloat) = {
        return (UIScreen.main.bounds.width, UIScreen.main.bounds.height)
    }()
    
    /// 时区偏移毫秒数
    private static let tzOffset: Int64 = {
        return Int64(TimeZone.current.secondsFromGMT(for: Date())) * 1000
    }()
    
    static var deviceInfo: [String : Any] {
        return [
            "country": countryCode,
            "platform": "IOS",
            "appId" : appBundleIdentifier,
            "version" : appVersion,
            "tzOffset": tzOffset,
            "deviceType" : modelName,
            "brand": "Apple",
            "model": model,
            "screenH": Int(screenSize.h),
            "screenW": Int(screenSize.w),
            "osVersion": systemVersion,
            "language" : languageCode
        ]
    }
    
    /// This method returns the hardware type
    ///
    ///
    /// - returns: raw `String` of device type, e.g. iPhone5,1
    ///
    private static func hardwareString() -> String {
        var name: [Int32] = [CTL_HW, HW_MACHINE]
        var size: Int = 2
        sysctl(&name, 2, nil, &size, nil, 0)
        var hw_machine = [CChar](repeating: 0, count: Int(size))
        sysctl(&name, 2, &hw_machine, &size, nil, 0)
        
        var hardware: String = String(cString: hw_machine)
        
        // Check for simulator
        if hardware == "x86_64" || hardware == "i386" || hardware == "arm64" {
            if let deviceID = ProcessInfo.processInfo.environment["SIMULATOR_MODEL_IDENTIFIER"] {
                hardware = deviceID
            }
        }
        
        return hardware
    }
    
    /// This method returns the Platform enum depending upon harware string
    ///
    ///
    /// - returns: `Platform` type of the device
    ///
    static func platform() -> Platform {
        
        let hardware = hardwareString()
        
        if (hardware.hasPrefix("iPhone"))    { return .iPhone }
        if (hardware.hasPrefix("iPod"))      { return .iPodTouch }
        if (hardware.hasPrefix("iPad"))      { return .iPad }
        if (hardware.hasPrefix("Watch"))     { return .appleWatch }
        if (hardware.hasPrefix("AppleTV"))   { return .appleTV }
        
        return .unknown
    }
    
    enum Platform {
        case iPhone
        case iPodTouch
        case iPad
        case appleWatch
        case appleTV
        case unknown
        
        enum DeviceType: String {
            case mobile, tablet, desktop, smartTV, watch, other
        }
        
        var deviceType: DeviceType {
            switch self {
            case .iPad:
                return .tablet
            case .iPhone, .iPodTouch:
                return .mobile
            case .appleTV:
                return .smartTV
            case .appleWatch:
                return .watch
            case .unknown:
                return .other
            }
        }
    }
    
}
