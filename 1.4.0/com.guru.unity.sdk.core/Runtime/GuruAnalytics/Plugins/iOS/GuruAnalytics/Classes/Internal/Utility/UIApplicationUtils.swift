//
//  UIApplicationUtils.swift
//  GuruAnalyticsLib
//
//  Created by 袁仕崇 on 17/12/24.
//

import Foundation
import UIKit

public enum UIApplicationUtil {
    /// Boolean describing whether the application is executing within an app extension.
    public static var isExecutingInAppExtension: Bool {
        let mainBundlePath = Bundle.main.bundlePath
        if mainBundlePath.count == 0 {
            return false
        }
        return mainBundlePath.hasSuffix("appex")
    }
    
    public static var sharedApplication: UIApplication? {
        guard !isExecutingInAppExtension else { return nil }
        return UIApplication.shared
    }
    
    /// Returns the current `UIViewController` for a parent controller.
    /// - Parameter parent: The parent `UIViewController`. If none provided, will attempt to discover the most
    /// relevant controller.
    @available(iOSApplicationExtension, unavailable)
    public static func currentViewController(forParent parent: UIViewController? = nil) -> UIViewController? {
        // return the current view controller of the parent
        if let parent = parent {
            return currentViewController(withRootViewController: parent)
        }
        
        // if this is an app extension, return nil
        guard !isExecutingInAppExtension else { return nil }
        
        for window in sharedApplication!.windows where window.isKeyWindow {
            return currentViewController(withRootViewController: window.rootViewController)
        }
        return nil
    }
    
    /// Attempt to find the top-most view controller for a given root view controller.
    /// - Parameter root: The root `UIViewController`.
    @available(iOSApplicationExtension, unavailable)
    public static func currentViewController(withRootViewController root: UIViewController?) -> UIViewController? {
        if let tabBarController = root as? UITabBarController {
            return currentViewController(withRootViewController: tabBarController.selectedViewController)
        } else if let navController = root as? UINavigationController {
            return currentViewController(withRootViewController: navController.visibleViewController)
        } else if let presented = root?.presentedViewController {
            return currentViewController(withRootViewController: presented)
        } else {
            return root
        }
    }
}
