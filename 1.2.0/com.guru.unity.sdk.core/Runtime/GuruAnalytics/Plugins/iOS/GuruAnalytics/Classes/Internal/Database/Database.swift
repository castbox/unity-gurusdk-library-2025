//
//  Database.swift
//  GuruAnalytics_iOS
//
//  Created by mayue on 2022/11/4.
//  Copyright © 2022 Guru Network Limited. All rights reserved.
//

import Foundation
import RxSwift
import RxCocoa
import FMDB

internal class Database {
    
    typealias PropertyName = GuruAnalytics.PropertyName
    
    enum TableName: String, CaseIterable {
        case event = "event"
    }
    
    private let dbIOQueue = DispatchQueue.init(label: "com.guru.analytics.db.io.queue", qos: .userInitiated)
    
    private let dbQueueRelay = BehaviorRelay<FMDatabaseQueue?>(value: nil)
    private let bag = DisposeBag()
    
    /// 更新数据库表结构后，需要更新数据库版本
    private let currentDBVersion = DBVersionHistory.v_3
    
    private var dbVersion: Database.DBVersionHistory {
        get {
            if let v = UserDefaults.defaults?.value(forKey: UserDefaults.dbVersionKey) as? String,
               let dbV = Database.DBVersionHistory.init(rawValue: v) {
                return dbV
            } else {
                return .initialVersion
            }
            
        }
        set {
            UserDefaults.defaults?.set(newValue.rawValue, forKey: UserDefaults.dbVersionKey)
        }
    }
    
    internal init() {
        
        dbIOQueue.async { [weak self] in
            
            guard let `self` = self else { return }
            
            let applicationSupportPath = NSSearchPathForDirectoriesInDomains(.applicationSupportDirectory,
                                                                             .userDomainMask,
                                                                             true).last! + "/GuruAnalytics"
            
            if !FileManager.default.fileExists(atPath: applicationSupportPath) {
                do {
                    try FileManager.default.createDirectory(atPath: applicationSupportPath, withIntermediateDirectories: true)
                } catch {
                    assertionFailure("create db path error: \(error)")
                }
            }
            
            let dbPath = applicationSupportPath + "/analytics.db"
            let queue = FMDatabaseQueue(url: URL(fileURLWithPath: dbPath))!
            cdPrint("database path: \(queue.path ?? "")")
            
            self.createEventTable(in: queue)
                .filter { $0 }
                .flatMap { _ in
                    self.migrateDB(in: queue).asMaybe()
                }
                .flatMap({ _ in
                    self.resetAllTransitionStatus(in: queue).asMaybe()
                })
                .subscribe(onSuccess: { _ in
                    self.dbQueueRelay.accept(queue)
                })
                .disposed(by: self.bag)
        }
        
    }
}

internal extension Database {
    
    func addEventRecords(_ events: Entity.EventRecord) -> Single<Void> {
        cdPrint(#function)
        return mapTransactionToSingle { (db) in
            try db.executeUpdate(events.insertSql(to: TableName.event.rawValue), values: nil)
        }
        .do(onSuccess: { [weak self] (_) in
            guard let `self` = self else { return }
            NotificationCenter.default.post(name: self.tableUpdateNotification(TableName.event.rawValue), object: nil)
        })
    }
    
    func fetchEventRecordsToUpload(limit: Int) -> Single<[Entity.EventRecord]> {
        return mapTransactionToSingle { (db) in
            let querySQL: String =
"""
SELECT * FROM \(TableName.event.rawValue)
WHERE \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) IS NULL
OR \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) != \(Entity.EventRecord.TransitionStatus.instransition.rawValue)
ORDER BY \(Entity.EventRecord.CodingKeys.priority.rawValue) ASC, \(Entity.EventRecord.CodingKeys.timestamp.rawValue) ASC
LIMIT \(limit)
"""
            cdPrint(#function + "query sql: \(querySQL)")
            let results = try db.executeQuery(querySQL, values: nil) //[ASC | DESC]
            var t: [Entity.EventRecord] = []
            while results.next() {
                guard let recordId = results.string(forColumnIndex: 0),
                      let eventName = results.string(forColumnIndex: 1),
                      let eventJson = results.string(forColumnIndex: 2) else {
                    continue
                }
                
                let priority: Int = results.columnIsNull(Entity.EventRecord.CodingKeys.priority.rawValue) ?
                Entity.EventRecord.Priority.DEFAULT.rawValue : Int(results.int(forColumn: Entity.EventRecord.CodingKeys.priority.rawValue))
                
                let ts: Int = results.columnIsNull(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) ?
                Entity.EventRecord.TransitionStatus.idle.rawValue : Int(results.int(forColumn: Entity.EventRecord.CodingKeys.transitionStatus.rawValue))
                
                let record = Entity.EventRecord(recordId: recordId, eventName: eventName, eventJson: eventJson,
                                                timestamp: results.longLongInt(forColumn: Entity.EventRecord.CodingKeys.timestamp.rawValue),
                                                priority: priority, transitionStatus: ts)
                t.append(record)
            }
            
            results.close()
            
            try t.forEach { record in
                let updateSQL =
"""
UPDATE \(TableName.event.rawValue)
SET \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) = \(Entity.EventRecord.TransitionStatus.instransition.rawValue)
WHERE \(Entity.EventRecord.CodingKeys.recordId.rawValue) = '\(record.recordId)'
"""
                try db.executeUpdate(updateSQL, values: nil)
            }
            
            return t
        }
    }
    
    func deleteEventRecords(_ recordIds: [String]) -> Single<Void> {
        guard !recordIds.isEmpty else {
            return .just(())
        }
        cdPrint(#function + "\(recordIds)")
        return mapTransactionToSingle { db in
            try recordIds.forEach { item in
                try db.executeUpdate("DELETE FROM \(TableName.event.rawValue) WHERE \(Entity.EventRecord.CodingKeys.recordId.rawValue) = '\(item)'", values: nil)
            }
        }
        .do(onSuccess: { [weak self] (_) in
            guard let `self` = self else { return }
            NotificationCenter.default.post(name: self.tableUpdateNotification(TableName.event.rawValue), object: nil)
        }, onError: { error in
            cdPrint("\(#function) error: \(error)")
        })
    }
    
    func removeOutdatedEventRecords(earlierThan: Int64) -> Single<Void> {
        return mapTransactionToSingle { db in
            let sql = """
DELETE FROM \(TableName.event.rawValue)
WHERE \(Entity.EventRecord.CodingKeys.timestamp.rawValue) < \(earlierThan)
"""
            try db.executeUpdate(sql, values: nil)
        }
        .do(onSuccess: { [weak self] (_) in
            guard let `self` = self else { return }
            NotificationCenter.default.post(name: self.tableUpdateNotification(TableName.event.rawValue), object: nil)
        }, onError: { error in
            cdPrint("\(#function) error: \(error)")
        })
    }
    
    func resetTransitionStatus(for recordIds: [String]) -> Single<Void> {
        guard !recordIds.isEmpty else {
            return .just(())
        }
        cdPrint(#function + "\(recordIds)")
        return mapTransactionToSingle { db in
            try recordIds.forEach { item in
                let updateSQL =
"""
UPDATE \(TableName.event.rawValue)
SET \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) = \(Entity.EventRecord.TransitionStatus.idle.rawValue)
WHERE \(Entity.EventRecord.CodingKeys.recordId.rawValue) = '\(item)'
"""
                try db.executeUpdate(updateSQL, values: nil)
            }
        }
        .do(onSuccess: { [weak self] (_) in
            guard let `self` = self else { return }
            NotificationCenter.default.post(name: self.tableUpdateNotification(TableName.event.rawValue), object: nil)
        }, onError: { error in
            cdPrint("\(#function) error: \(error)")
        })
    }
    
    func uploadableEventRecordCount() -> Single<Int> {
        return mapTransactionToSingle { db in
            let querySQL =
"""
SELECT count(*) as Count FROM \(TableName.event.rawValue)
WHERE \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) IS NULL
OR \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) != \(Entity.EventRecord.TransitionStatus.instransition.rawValue)
"""
            let result = try db.executeQuery(querySQL, values: nil)
            var count = 0
            while result.next() {
                count = Int(result.int(forColumn: "Count"))
            }
            result.parentDB = nil
            result.close()
            return count
        }
    }
    
    func uploadableEventRecordCountOb() -> Observable<Int> {
        return NotificationCenter.default.rx.notification(tableUpdateNotification(TableName.event.rawValue))
            .startWith(Notification(name: tableUpdateNotification(TableName.event.rawValue)))
            .flatMap({ [weak self] (_) -> Observable<Int> in
                guard let `self` = self else {
                    return Observable.empty()
                }
                return self.uploadableEventRecordCount().asObservable()
            })
    }
    
    func hasFgEventRecord() -> Single<Bool> {
        return mapTransactionToSingle { db in
            let querySQL =
"""
SELECT count(*) as Count FROM \(TableName.event.rawValue)
WHERE \(Entity.EventRecord.CodingKeys.eventName.rawValue) == '\(GuruAnalytics.fgEvent.name)'
"""
            let result = try db.executeQuery(querySQL, values: nil)
            var count = 0
            while result.next() {
                count = Int(result.int(forColumn: "Count"))
            }
            result.parentDB = nil
            result.close()
            return count > 0
        }
    }
    
}

private extension Database {
    func createEventTable(in queue: FMDatabaseQueue) -> Single<Bool> {
        return mapTransactionToSingle(queue: queue) { db in
            db.executeStatements(Entity.EventRecord.createTableSql(with: TableName.event.rawValue))
        }
        .do(onSuccess: { result in
            cdPrint("createEventTable result: \(result)")
        }, onError: { error in
            cdPrint("createEventTable error: \(error)")
        })
    }
    
    func mapTransactionToSingle<T>(_ transaction: @escaping ((FMDatabase) throws -> T)) -> Single<T> {
        return dbQueueRelay.compactMap({ $0 })
            .take(1)
            .asSingle()
            .flatMap { [unowned self] queue -> Single<T> in
                return self.mapTransactionToSingle(queue: queue, transaction)
            }
    }
    
    func mapTransactionToSingle<T>(queue: FMDatabaseQueue, _ transaction: @escaping ((FMDatabase) throws -> T)) -> Single<T> {
        return Single<T>.create { [weak self] (subscriber) -> Disposable in
            self?.dbIOQueue.async {
                queue.inDeferredTransaction { (db, rollback) in
                    do {
                        let data = try transaction(db)
                        subscriber(.success(data))
                    } catch {
                        rollback.pointee = true
                        cdPrint("inDeferredTransaction failed: \(error.localizedDescription)")
                        subscriber(.failure(error))
                    }
                }
            }
            return Disposables.create()
        }
    }
    
    func tableUpdateNotification(_ tableName: String) -> Notification.Name {
        return Notification.Name("Guru.Analytics.DB.Table.update-\(tableName)")
    }
    
    func migrateDB(in queue: FMDatabaseQueue) -> Single<Void> {
        
        return mapTransactionToSingle(queue: queue) { [weak self] db in
            
            guard let `self` = self else { return }
            
            while let nextVersion = self.dbVersion.nextVersion,
                  self.dbVersion < self.currentDBVersion {
                switch nextVersion {
                case .v_1:
                    ()
                case .v_2:
                    /// v_1 -> v_2
                    /// event表增加priority列
                    if !db.columnExists(Entity.EventRecord.CodingKeys.priority.rawValue, inTableWithName: TableName.event.rawValue) {
                        db.executeStatements("""
ALTER TABLE \(TableName.event.rawValue)
ADD \(Entity.EventRecord.CodingKeys.priority.rawValue) Integer DEFAULT \(Entity.EventRecord.Priority.DEFAULT.rawValue)
""")
                    }
                    
                case .v_3:
                    /// v_2 -> v_3
                    /// event表增加transitionStatus列
                    if !db.columnExists(Entity.EventRecord.CodingKeys.transitionStatus.rawValue, inTableWithName: TableName.event.rawValue) {
                        db.executeStatements("""
ALTER TABLE \(TableName.event.rawValue)
ADD \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) Integer DEFAULT \(Entity.EventRecord.TransitionStatus.idle.rawValue)
""")
                    }
                    
                }
                self.dbVersion = nextVersion
                
            }
            
        }
        .do(onError: { error in
            cdPrint("migrate db error: \(error)")
        })
        
    }
    
    func resetAllTransitionStatus(in queue: FMDatabaseQueue) -> Single<Void> {
        return mapTransactionToSingle(queue: queue) { db in
            let updateSQL =
"""
UPDATE \(TableName.event.rawValue)
SET \(Entity.EventRecord.CodingKeys.transitionStatus.rawValue) = \(Entity.EventRecord.TransitionStatus.idle.rawValue)
"""
            try db.executeUpdate(updateSQL, values: nil)
        }
        .do(onSuccess: { [weak self] (_) in
            guard let `self` = self else { return }
            NotificationCenter.default.post(name: self.tableUpdateNotification(TableName.event.rawValue), object: nil)
        }, onError: { error in
            cdPrint("\(#function) error: \(error)")
        })
    }

}

fileprivate extension Array where Element == String {
    
    var joinedStringForSQL: String {
        return self.map { "'\($0)'" }.joined(separator: ",")
    }
    
}

private extension Database {
    
    enum DBVersionHistory: String, Comparable {
        case v_1
        case v_2
        case v_3
    }
}

extension Database.DBVersionHistory {
    
    static func < (lhs: Database.DBVersionHistory, rhs: Database.DBVersionHistory) -> Bool {
        return lhs.versionNumber < rhs.versionNumber
    }
    
    
    var versionNumber: Int {
        return Int(String(self.rawValue.split(separator: "_")[1])) ?? 1
    }
    
    var nextVersion: Self? {
        return .init(rawValue: "v_\(versionNumber + 1)")
    }
    
    static let initialVersion: Self = .v_1
}
