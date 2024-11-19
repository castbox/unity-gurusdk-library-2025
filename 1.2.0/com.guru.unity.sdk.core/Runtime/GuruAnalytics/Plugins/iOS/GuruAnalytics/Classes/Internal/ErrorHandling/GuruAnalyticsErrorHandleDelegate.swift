//
//  GuruAnalyticsErrorHandleDelegate.swift
//  Alamofire
//
//  Created by mayue on 2023/10/27.
//

import Foundation

internal enum GuruAnalyticsNetworkLayerErrorCategory: Int {
    case unknown = -100
    case serverAPIError = 101
    case responseParsingError = 102
    case googleDNSServiceError = 106
}

@objc internal protocol GuruAnalyticsNetworkErrorReportDelegate {
    func reportError(networkError: GuruAnalyticsNetworkError) -> Void
}

internal class GuruAnalyticsNetworkError: NSError {
    private(set) var httpStatusCode: Int?
    private(set) var originError: Error
    private(set) var internalErrorCategory: GuruAnalyticsNetworkLayerErrorCategory
    
    init(httpStatusCode: Int? = nil, internalErrorCategory: GuruAnalyticsNetworkLayerErrorCategory, originError: Error) {
        self.httpStatusCode = httpStatusCode
        self.originError = originError
        self.internalErrorCategory = internalErrorCategory
        super.init(domain: "com.guru.analytics.network.layer", code: internalErrorCategory.rawValue, userInfo: (originError as NSError).userInfo)
    }
    
    required init?(coder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
}
