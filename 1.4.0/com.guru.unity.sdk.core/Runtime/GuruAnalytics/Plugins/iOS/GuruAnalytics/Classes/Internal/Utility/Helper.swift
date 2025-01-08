//
//  Helper.swift
//  GuruAnalytics_iOS
//
//  Created by mayue on 2022/11/4.
//

import Foundation

internal func cdPrint(_ items: Any..., context: String? = nil, separator: String = " ", terminator: String = "\n") {
#if DEBUG
    guard GuruAnalytics.loggerDebug else { return }
    let date = Date()
    let df = DateFormatter()
    df.dateFormat = "HH:mm:ss.SSSS"
    let dateString = df.string(from: date)
    
    print("\(dateString) [GuruAnalytics] Thread: \(Thread.current.queueName) \(context ?? "") ", terminator: "")
    for item in items {
        print(item, terminator: " ")
    }
    print("", terminator: terminator)
#else
#endif
}
