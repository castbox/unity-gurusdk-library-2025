//
//  JSONDecoder.Extension.swift
//  Moya-Cuddle
//
//  Created by Wilson-Yuan on 2019/12/25.
//  Copyright © 2019 Guru. All rights reserved.
//

import Foundation

internal extension JSONDecoder {
    func decodeAnyData<T>(_ type: T.Type, from data: Any) throws -> T where T: Decodable {
        var unwrappedData = Data()
        if let data = data as? Data {
            unwrappedData = data
        }
        else if let data = data as? [String: Any] {
            unwrappedData = try JSONSerialization.data(withJSONObject: data, options: .prettyPrinted)
        }
        else if let data = data as? [[String: Any]] {
            unwrappedData = try JSONSerialization.data(withJSONObject: data, options: .prettyPrinted)
        }
        else {
            fatalError("error format of data ")
        }
        return try decode(type, from: unwrappedData)
    }
}
