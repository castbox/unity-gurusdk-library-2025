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
    
    private func decodeObject<T>(_ type: T.Type, from data: Data) -> T? where T: Decodable {
        
        guard data.count > 0 else { return nil }
        
        var object: T? = nil
        
        do {
            object = try decode(type, from: data)
        } catch {
            cdPrint("JSONDecoder decode error: \(error)")
        }
        
        return object
    }
    
    func decodeObject<T>(_ type: T.Type, from jsonString: String) -> T? where T: Decodable {
        guard let jsonData = jsonString.data(using: .utf8) else { return nil }
        return decodeObject(type, from: jsonData)
    }
    
    func decodeObject<T>(_ type: T.Type, from dictionary: [String : Any]) -> T? where T: Decodable {
        
        var data: Data?
        
        do {
            data = try JSONSerialization.data(withJSONObject: dictionary, options: .prettyPrinted)
        } catch let error {
            cdPrint(error)
        }
        
        guard let jsonData = data else { return nil }
        return decodeObject(type, from: jsonData)
    }
}
