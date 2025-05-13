//
//  Untitled.swift
//  Pods
//
//  Created by mayue on 2025/1/14.
//

extension NSNumber {
    var valueType: CFNumberType {
        return CFNumberGetType(self as CFNumber)
    }
    
    var numricValue: Any {
        switch valueType {
        case .sInt8Type,
                .sInt16Type,
                .sInt32Type,
                .charType,
                .shortType,
                .intType,
                .longType,
                .cfIndexType,
                .nsIntegerType:
            return intValue;
            
        case
                .sInt64Type,
                .longLongType:
            return int64Value;
            
        case .float32Type,
                .float64Type,
                .floatType,
                .doubleType,
                .cgFloatType,
                .maxType:
            return doubleValue;
            
        @unknown default:
            return doubleValue;
        }
    }
}
