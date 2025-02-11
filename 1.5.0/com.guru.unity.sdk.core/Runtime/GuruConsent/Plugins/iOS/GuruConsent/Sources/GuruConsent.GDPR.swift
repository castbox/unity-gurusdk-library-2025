//
//  GuruConsent.GDPR.swift
//  GuruConsent
//
//  Created by 李响 on 2022/11/11.
//

import UIKit
import UserMessagingPlatform

public extension GuruConsent {
    
    /// 当前状态
    @objc
    public static var status: GDPRStatus {
        return .init(
            rawValue: UMPConsentInformation.sharedInstance.consentStatus.rawValue
        ) ?? .unknown
    }
    
    /// 是否可以请求广告 (status为.obtained或.notRequired)
    @objc
    public static var canRequestAds: Bool {
        return UMPConsentInformation.sharedInstance.canRequestAds
    }
    
    /// 隐私选项状态
    /// 初始状态为.unknown  如果status为.notRequired 该状态也是.notRequired
    /// status为.obtained(弹窗同意后) 会变成.required, 代表可以让用户修改之前同意的隐私选项
    /// 调用`func openPrivacyOptions(from: with:)`方法可以再次打开弹窗
    /// 弹窗第二次打开后 用户可以重新勾选隐私选项并同意, 操作完成后状态会变为.unknown
    @objc
    public static var privacyOptionsRequirementStatus: GDPRPrivacyOptionsRequirementStatus {
        return .init(
            rawValue: UMPConsentInformation.sharedInstance.privacyOptionsRequirementStatus.rawValue
        ) ?? .unknown
    }
    
    private static var form: UMPConsentForm?
    
    internal static func request(with completion: @escaping ((Swift.Result<GDPRFormStatus, Error>) -> Void)) {
        let parameters = UMPRequestParameters()
        // 设置未满同意年龄的标签。此处false表示用户达到年龄
        parameters.tagForUnderAgeOfConsent = tagForUnderAgeOfConsent
        // 设置调试设置
        if let debug = GuruConsent.debug {
            let debugSettings = UMPDebugSettings()
            debugSettings.testDeviceIdentifiers = debug.testDeviceIdentifiers
            debugSettings.geography = .init(rawValue: debug.geography.rawValue) ?? .disabled
            parameters.debugSettings = debugSettings
        }
        
        // 请求最新同意信息
        UMPConsentInformation.sharedInstance.requestConsentInfoUpdate(
            with: parameters,
            completionHandler: { error in
                if let error = error {
                    // 请求同意信息失败
                    completion(.failure(error))
                    
                } else {
                    let status = UMPConsentInformation.sharedInstance.formStatus
                    completion(.success(.init(rawValue: status.rawValue) ?? .unknown))
                }
            }
        )
    }
    
    static func loadForm(with completion: @escaping ((Swift.Result<GDPRStatus, Error>) -> Void)) {
        // 加载表单
        UMPConsentForm.load { form, error in
            if let error = error {
                // 表单加载失败
                completion(.failure(error))
                
            } else {
                self.form = form
                let status = UMPConsentInformation.sharedInstance.consentStatus
                completion(.success(.init(rawValue: status.rawValue) ?? .unknown))
            }
        }
    }
    
    static func openForm(from controller: UIViewController, with completion: @escaping ((Swift.Result<GDPRStatus, Error>) -> Void)) {
        guard let form = form else {
            completion(.failure(NSError(domain: "Form Empty.", code: -1)))
            return
        }
        // 打开弹窗
        form.present(
            from: controller,
            completionHandler: { error in
                if let error = error {
                    // 弹窗失败
                    completion(.failure(error))
                    
                } else {
                    // 是否已同意
                    completion(.success(status))
                }
            }
        )
    }
    
    /// 重置
    @objc
    public static func reset() {
        UMPConsentInformation.sharedInstance.reset()
    }
    
    /// 打开隐私选项弹窗 (privacyOptionsRequirementStatus必须为.required)
    /// - Parameters:
    ///   - controller: 视图控制器
    ///   - completion: 完成回调
    @objc
    public static func openPrivacyOptions(from controller: UIViewController, with completion: @escaping ((Error?) -> Void)) {
        UMPConsentForm.presentPrivacyOptionsForm(from: controller) { error in
            completion(error)
        }
    }
}
