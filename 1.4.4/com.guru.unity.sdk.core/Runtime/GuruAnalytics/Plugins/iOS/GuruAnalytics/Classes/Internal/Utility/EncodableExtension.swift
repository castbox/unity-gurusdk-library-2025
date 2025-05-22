//
//  EncodableExtension.swift
//  Runner
//
//  Created by 袁仕崇 on 2020/5/19.
//  Copyright © 2020 Guru. All rights reserved.
//

import Foundation

internal extension Encodable {
    func asDictionary() throws -> [String: Any] {
        let data = try JSONEncoder().encode(self)
        guard let dictionary = try JSONSerialization.jsonObject(with: data, options: .allowFragments) as? [String: Any] else {
            throw NSError()
        }
        return dictionary
    }
    
    var dictionary: [String: Any]? {
        guard let data = try? JSONEncoder().encode(self) else { return nil }
        return (try? JSONSerialization.jsonObject(with: data, options: .allowFragments)).flatMap { $0 as? [String: Any] }
    }
    
    var asString: String? {
        guard let data = try? JSONEncoder().encode(self) else { return nil }
        return String(data: data, encoding: .utf8)
    }
}

internal extension String {
    
    func jsonObject() -> [String: Any]? {

        guard let data = data(using: .utf8) else {
            return nil
        }
        guard let jsonData = try? JSONSerialization.jsonObject(with: data, options: .fragmentsAllowed) as? [String: Any] else {
            return nil
        }
        return jsonData
    }
    
    func jsonArrayObject() -> [[String: Any]]? {

        guard let data = data(using: .utf8) else {
            return nil
        }
        guard let jsonData = try? JSONSerialization.jsonObject(with: data, options: .fragmentsAllowed) as? [[String: Any]] else {
            return nil
        }
        return jsonData
    }
}
