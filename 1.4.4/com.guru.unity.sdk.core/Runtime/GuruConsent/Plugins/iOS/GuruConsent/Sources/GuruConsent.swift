//
//  GuruConsent.swift
//  GuruConsent
//
//  Created by 李响 on 2022/11/11.
//

import UIKit

@objc
public class GuruConsent: NSObject {
    
    /// CDPR状态
    @objc(GuruConsentGDPRStatus)
    public enum GDPRStatus: Int {
        case unknown        ///< Unknown consent status.
        case required       ///< User consent required but not yet obtained.
        case notRequired    ///< Consent not required.
        case obtained       ///< User consent obtained, personalized vs non-personalized undefined.
    }
    
    /// CDPR表单状态
    internal enum GDPRFormStatus: Int {
        case unknown
        case available
        case unavailable
    }
    
    /// GDPR隐私选项所需状态
    @objc(GuruConsentGDPRPrivacyOptionsRequirementStatus)
    public enum GDPRPrivacyOptionsRequirementStatus: Int {
        case unknown        ///< Requirement unknown.
        case required       ///< A way must be provided for the user to modify their privacy options.
        case notRequired    ///< User does not need to modify their privacy options. Either consent is not required, or the consent type does not require modification.
    }
    
    /// 调试设置
    @objc(GuruConsentDebugSettings)
    public class DebugSettings: NSObject {
        
        @objc(GuruConsentDebugSettingsGeography)
        public enum Geography: Int {
            case disabled
            case EEA
            case notEEA
        }
        
        /// 测试设备ID
        @objc
        public var testDeviceIdentifiers: [String]
        /// 地理位置
        @objc
        public var geography: Geography
        
        @objc
        public override init() {
            testDeviceIdentifiers = []
            geography = .disabled
        }
    }
    
    /// 调试设置
    @objc
    public static var debug: DebugSettings?
    
    /// 设置未满同意年龄的标签 默认为false 表示用户达到年龄
    @objc
    public static var tagForUnderAgeOfConsent: Bool = false
    
    /// 是否已同意 (status为.obtained)
    @objc
    public static var isObtained: Bool {
        return status == .obtained
    }
    
    /// 开始 OC
    /// ATT未授权过(非EEA地区) 会弹出ATT引导弹窗 在点击继续按钮时弹出ATT权限弹窗
    /// ATT未授权过(EEA地区) 会弹出GDPR弹窗 在点击同意时弹出ATT权限弹窗
    /// - Parameters:
    ///   - controller: 视图控制器
    ///   - completion: 完成回调
    @objc
    public static func start(from controller: UIViewController, success: @escaping ((GDPRStatus) -> Void), failure: @escaping (Error) -> Void) {
        start(from: controller) { result in
            switch result {
            case .success(let value):
                success(value)
                
            case .failure(let error):
                failure(error)
            }
        }
    }
    
    /// 开始 Swift
    /// ATT未授权过(非EEA地区) 会弹出ATT引导弹窗 在点击继续按钮时弹出ATT权限弹窗
    /// ATT未授权过(EEA地区) 会弹出GDPR弹窗 在点击同意时弹出ATT权限弹窗
    /// - Parameters:
    ///   - controller: 视图控制器
    ///   - completion: 完成回调
    public static func start(from controller: UIViewController, with completion: @escaping ((Swift.Result<GDPRStatus, Error>) -> Void)) {
        request { result in
            switch result {
            case .success(let value):
                if value == .available {
                    // 加载表单
                    loadForm { result in
                        switch result {
                        case .success(let value):
                            switch value {
                            case .required:
                                // 打开表单
                                openForm(from: controller, with: completion)
                                
                            default:
                                completion(.success(value))
                            }
                            
                        case .failure(let error):
                            // 表单加载失败
                            completion(.failure(error))
                        }
                    }
                    
                } else {
                    // 无表单需要加载
                    completion(.success(.notRequired))
                }
                    
            case .failure(let error):
                // 请求同意信息失败
                completion(.failure(error))
            }
        }
    }
    
    /// ----------------------------------------
    
    @objc
    public static func prepare(success: @escaping ((GDPRStatus) -> Void), failure: @escaping (Error) -> Void) {
        prepare { result in
            switch result {
            case .success(let value):
                success(value)
                
            case .failure(let error):
                failure(error)
            }
        }
    }
    
    @objc
    public static func present(from controller: UIViewController, success: @escaping ((GDPRStatus) -> Void), failure: @escaping (Error) -> Void) {
        present(from: controller) { result in
            switch result {
            case .success(let value):
                success(value)
                
            case .failure(let error):
                failure(error)
            }
        }
    }
    
    /// 预加载 (需在加载成功后 手动调用打开表单)
    /// - Parameter completion: 完成回调
    public static func prepare(with completion: @escaping ((Swift.Result<GDPRStatus, Error>) -> Void)) {
        request { result in
            switch result {
            case .success(let value):
                if value == .available {
                    // 加载表单
                    loadForm { result in
                        switch result {
                        case .success(let value):
                            // 表单加载成功
                            completion(.success(value))
                            
                        case .failure(let error):
                            // 表单加载失败
                            completion(.failure(error))
                        }
                    }
                    
                } else {
                    // 无表单需要加载
                    completion(.success(.notRequired))
                }
                
            case .failure(let error):
                // 请求同意信息失败
                completion(.failure(error))
            }
        }
    }
    
    /// 打开表单 请确保status为.required 否则无效
    /// - Parameters:
    ///   - controller: 视图控制器
    ///   - completion: 完成回调
    public static func present(from controller: UIViewController, with completion: @escaping ((Swift.Result<GDPRStatus, Error>) -> Void)) {
        openForm(from: controller, with: completion)
    }
}
