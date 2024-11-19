//
//  Logger.swift
//  GuruAnalyticsLib
//
//  Created by mayue on 2022/12/21.
//

import Foundation
import SwiftyBeaver
import CryptoSwift
import RxSwift

internal class LoggerManager {
    
    private static let password: String = "Castbox123"
    
    private lazy var logger: SwiftyBeaver.Type = {
        let logger = SwiftyBeaver.self
        logger.addDestination(consoleOutputDestination)
        logger.addDestination(fileOutputDestination)
        return logger
    }()
    
    private lazy var logFileDir: URL = {
        let baseDir = FileManager.default.urls(for: .applicationSupportDirectory, in: .userDomainMask).first!
        return baseDir.appendingPathComponent("GuruAnalytics/Logs/\(logCategoryName)/", isDirectory: true)
    }()
    
    private lazy var consoleOutputDestination: ConsoleDestination = {
        let d = ConsoleDestination()
        return d
    }()
    
    private lazy var fileOutputDestination: FileDestination = {
        let file = FileDestination()
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd"
        let dateString = dateFormatter.string(from: Date())
        file.logFileURL = logFileDir.appendingPathComponent("\(dateString).log", isDirectory: false)
        file.asynchronously = true
        return file
    }()
    
    private let logCategoryName: String
    
    internal init(logCategoryName: String) {
        self.logCategoryName = logCategoryName
    }
}

internal extension LoggerManager {
    
    func logFilesZipArchive() -> Single<URL?> {
        
        return Single.create { subscriber in
            subscriber(.success(nil))
            return Disposables.create()
        }
        .observe(on: MainScheduler.asyncInstance)
    }
    
    func logFilesDirURL() -> Single<URL?> {
        
        return Single.create { subscriber in
            
            DispatchQueue.global().async { [weak self] in
                guard let `self` = self else {
                    subscriber(.failure(
                        NSError(domain: "loggerManager", code: 0, userInfo: [NSLocalizedDescriptionKey : "manager is released"])
                    ))
                    return
                }
                
                do {
                    let filePaths = try FileManager.default.contentsOfDirectory(at: self.logFileDir,
                                                                                includingPropertiesForKeys: nil,
                                                                                options: [.skipsHiddenFiles])
                        .filter { $0.pathExtension == "log" }
                        .map { $0.path }

                    guard filePaths.count > 0 else {
                        subscriber(.success(nil))
                        return
                    }
                    subscriber(.success(self.logFileDir))
                } catch {
                    subscriber(.failure(error))
                }
                
            }
            
            return Disposables.create()
        }
        .observe(on: MainScheduler.asyncInstance)
    }
    
    func clearAllLogFiles() {
        
        DispatchQueue.global().async { [weak self] in
            guard let `self` = self else { return }
            if let files = try? FileManager.default.contentsOfDirectory(at: self.logFileDir, includingPropertiesForKeys: [], options: [.skipsHiddenFiles]) {
                files.forEach { url in
                    do {
                        try FileManager.default.removeItem(at: url)
                    } catch  {
                        cdPrint("remove file: \(url.path) \n error: \(error)")
                    }
                }
            }
        }
        
    }
    
    func verbose(_ message: Any,
                 _ file: String = #file,
                 _ function: String = #function,
                 line: Int = #line,
                 context: Any? = nil) {
        guard GuruAnalytics.loggerDebug else { return }
        logger.verbose(message, file, function, line: line, context: context)
    }
    
    func debug(_ message: Any,
               _ file: String = #file,
               _ function: String = #function,
               line: Int = #line,
               context: Any? = nil) {
        guard GuruAnalytics.loggerDebug else { return }
        logger.debug(message, file, function, line: line, context: context)
    }
    
    func info(_ message: Any,
              _ file: String = #file,
              _ function: String = #function,
              line: Int = #line,
              context: Any? = nil) {
        guard GuruAnalytics.loggerDebug else { return }
        logger.info(message, file, function, line: line, context: context)
    }
    
    func warning(_ message: Any,
                 _ file: String = #file,
                 _ function: String = #function,
                 line: Int = #line,
                 context: Any? = nil) {
        guard GuruAnalytics.loggerDebug else { return }
        logger.warning(message, file, function, line: line, context: context)
    }
    
    func error(_ message: Any,
               _ file: String = #file,
               _ function: String = #function,
               line: Int = #line,
               context: Any? = nil) {
        guard GuruAnalytics.loggerDebug else { return }
        logger.error(message, file, function, line: line, context: context)
    }
}
