//
//  Utilities.swift
//  GuruAnalytics_iOS
//
//  Created by mayue on 2022/11/4.
//

import Foundation
import RxSwift

internal extension TimeInterval {
    
    var int64Ms: Int64 {
        return Int64(self * 1000)
    }
    
}

internal extension Date {
    
    var msSince1970: Int64 {
        timeIntervalSince1970.int64Ms
    }
    
    static var absoluteTimeMs: Int64 {
        return CACurrentMediaTime().int64Ms
    }
    
}

internal extension Dictionary {
    
    func jsonString(prettify: Bool = false) -> String? {
        guard JSONSerialization.isValidJSONObject(self) else { return nil }
        let options = (prettify == true) ? JSONSerialization.WritingOptions.prettyPrinted : JSONSerialization.WritingOptions()
        guard let jsonData = try? JSONSerialization.data(withJSONObject: self, options: options) else { return nil }
        return String(data: jsonData, encoding: .utf8)
    }
    
}

internal extension String {
    func convertToDictionary() -> [String: Any]? {
        if let data = data(using: .utf8) {
            return (try? JSONSerialization.jsonObject(with: data, options: [])) as? [String: Any]
        }
        return nil
    }
    
    mutating func deletePrefix(_ prefix: String) {
        guard hasPrefix(prefix) else { return }
        if #available(iOS 16.0, *) {
            trimPrefix(prefix)
        } else {
            removeFirst(prefix.count)
        }
    }
    
    mutating func trimmed(in set: CharacterSet) {
        self = trimmingCharacters(in: set)
    }
}

internal extension Array {
    func chunked(into size: Int) -> [[Element]] {
        return stride(from: 0, to: count, by: size).map {
            Array(self[$0 ..< Swift.min($0 + size, count)])
        }
    }
}

internal class SafeValue<T> {
    
    private var _value: T
    
    private let queue = DispatchQueue(label: "com.guru.analytics.safe.value.reader.writer.queue", attributes: .concurrent)
    private let group = DispatchGroup()
    
    internal init(_ value: T) {
        _value = value
    }
    
    internal func setValue(_ value: T) {
        queue.async(group: group, execute: .init(flags: .barrier, block: { [weak self] in
            self?._value = value
        }))
    }
    
    internal func getValue(_ valueBlock: @escaping ((T) -> Void)) {
        queue.async(group: group, execute: .init(block: { [weak self] in
            guard let `self` = self else { return }
            valueBlock(self._value)
        }))
    }
    
    internal var singleValue: Single<T> {
        return Single.create { [weak self] subscriber in
            
            self?.getValue { value in
                subscriber(.success(value))
            }
            
            return Disposables.create()
        }
    }
}

internal extension SafeValue where T == Dictionary<String, String> {
    
    func mergeValue(_ value: T) -> Single<Void> {
        return .create { [weak self] subscriber in
            guard let `self` = self else {
                subscriber(.failure(
                    NSError(domain: "safevalue", code: 0, userInfo: [NSLocalizedDescriptionKey : "safevalue object is released"])
                ))
                return Disposables.create()
            }
            self.getValue { currentValue in
                let newValue = currentValue.merging(value) { _, new in new }
                self.setValue(newValue)
                subscriber(.success(()))
            }
            
            return Disposables.create()
        }
    }
}

internal extension SafeValue where T == Array<String> {
    
    func appendValue(_ value: T) {
        getValue { [weak self] v in
            var currentValue = v
            currentValue.append(contentsOf: value)
            self?.setValue(currentValue)
        }
    }
    
    func removeAll(where shouldBeRemoved: @escaping (Array<String>.Element) -> Bool) {
        getValue { [weak self] v in
            var currentValue = v
            currentValue.removeAll(where: shouldBeRemoved)
            self?.setValue(currentValue)
        }
    }
    
}

internal extension Character {
    
    var isAlphabetic: Bool {
        return (self >= "a" && self <= "z") || (self >= "A" && self <= "Z")
    }
    
    var isDigit: Bool {
        return self >= "0" && self <= "9"
    }
}
