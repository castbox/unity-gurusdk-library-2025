// ==============================================
//  U3DAnalytics 1.12.0
//  Native Framework Version 0.3.6
//  update date: 2024-08-08  -- by HuYufei
//
//  Created by HuYufei on 2022/11/17.
//  Copyright © 2022 guru. All rights reserved.
// ==============================================

#import <Foundation/Foundation.h>
#import "UnityAppController+UnityInterface.h"
#import <GuruAnalyticsLib/GuruAnalyticsLib-Swift.h>

@interface U3DAnalytics : NSObject

//+ (void)requestGDPR: (NSString *)deviceId :(int) debugGeo;
//+ (UIViewController *) getUnityViewController;

@end

static NSString *gameObjectName = @"GuruCallback";
static NSString *callbackName =@"OnCallback";

static GuruAnalytics *_analytics;


@implementation U3DAnalytics

// Const value define
NSString * const Version = @"1.13.1";

static const double kUploadPeriodInSecond = 60.0;
static const int kBatchLimit = 15;
static const double kEventExpiredSeconds = 7 * 24 * 60 * 60;
static const double kInitializeTimeout = 5.0;

static double tch001MaxValue = 0.01;
static double tch02MaxValue = 0.2;
static bool enableErrorLog = false;

static int eventCountAll;
static int eventCountUploaded;

NSString * const TchAdRevRoas001 = @"tch_ad_rev_roas_001";
NSString * const TchAdRevRoas02 = @"tch_ad_rev_roas_02";
NSString * const TchError = @"tch_error";



+(UnityAppController *)GetAppController {
    return (UnityAppController*)[UIApplication sharedApplication].delegate;
}

+(UIViewController *) getUnityViewController {
    return UnityGetGLViewController();
}

// 字符串转换
+(const char*) stringToChar: (NSString *) str{
    return [str cStringUsingEncoding:NSASCIIStringEncoding];
}


+(NSString *) charToString: (const char *) c{
    return [[NSString alloc] initWithUTF8String:c];
}

// 获取最终字符串
+(char*) finalChar: (NSString *) string
{
    if(string == NULL){
        return NULL;
    }
    
    const char *tmpChar = [string cStringUsingEncoding:NSASCIIStringEncoding];
    
    if(tmpChar == NULL){
        return NULL;
    }
    char* res = (char*)malloc(strlen(tmpChar) + 1);
    strcpy(res, tmpChar);
    
    return res;
}

// 设置太极02的预设值
+(void) setTch02MaxValue: (const double) value{
    tch02MaxValue = value;
}

// 设置是否启用日志错误上报
+(void) setEnableErrorLog: (bool) value{
    if(value){
        // 注册事件
        enableErrorLog = true;
        [GuruAnalytics registerInternalEventObserverWithReportCallback:^(NSInteger code, NSString * info){
            [U3DAnalytics onEventCallback:code andInfo:info];
        }];
        return;
    }
    // 否则只赋值
    enableErrorLog = value;
}


// 事件上报回调
+(void) getEventsStatistics {
    [GuruAnalytics debug_eventsStatistics:^(NSInteger uploadedEventsCount, NSInteger loggedEventsCount) {
                // 上报事件总量
                eventCountAll = (int)uploadedEventsCount;
                // 上报成功数量
                eventCountUploaded = (int)loggedEventsCount;
            }];
}

// 设置 BaseUrl
+(void) setBaseUrl: (const char *) baseUrl{
    if (baseUrl != nullptr && strlen(baseUrl) == 0) {
        return; // baseUrl 为空
    }
    [GuruAnalytics setEventsUploadEndPointWithHost:[U3DAnalytics charToString:baseUrl]];
}

// 事件上报回调
+(void) onEventCallback: (NSInteger)code  andInfo:(NSString *) info {
    if(enableErrorLog == false) {
        return; // 开关关闭不报
    }
    if(info == nil){
        return; // 空字符串不报
    }
    NSString *msg = [U3DAnalytics buildLogEventString: code  andMessage:info];
    if(msg != NULL){
        return;
    }
    [U3DAnalytics sendMessageToUnity: msg];
}

// 构建数据
+(NSString *) buildLogEventString: (NSInteger)status andMessage: (NSString *)msg{
    NSString *jsonString = [NSString stringWithFormat: @"{\"action\":\"logger_error\",\"data\":{\"code\":%d,\"msg\":\"%@\"}}", (int)status, msg];
    NSLog(@"[ANI][Error]: %@", jsonString);
    return jsonString;
}

// 构建数据字典
+(NSDictionary<NSString*, id> *) buildDataDict: (NSString *) str{
    
    NSArray *raw = [str componentsSeparatedByString:@","];
    if( raw == nil || raw.count == 0) return nil;
    
    NSMutableDictionary* dict = [[NSMutableDictionary alloc] init]; //must init before using
    
    for(NSString *s in raw ){
        NSArray *kvp = [s componentsSeparatedByString:@":"];
        if(kvp == nil || kvp.count  < 2){
            continue;
        }
        
        NSString *k = kvp[0];
        NSString *t = [kvp[1] substringToIndex:1];
        NSString *v = [kvp[1] substringFromIndex:1];
        
        // NSLog(@"---[iOS] parse kvp  key:%@  type:%@ value:%@", k, t, v);
        
        //TODO 解析字符值
        if([t isEqual: @"i"]){
            // int
            [dict setValue:@([v integerValue]) forKey:k];
        } else if ([t isEqual: @"d"]){
            // double
            [dict setValue:@([v doubleValue]) forKey:k];
        } else {
            // String
            [dict setValue:v forKey:k];
        }
    }
    return dict;
}

// 构建Json格式的数据字典
+(NSMutableDictionary *) buildDataWithJson: (NSString *) json andKey: (NSString *) key{
    
    if( json == nil || json.length == 0) return nil;
    
    NSData *jsonData = [json dataUsingEncoding:NSUTF8StringEncoding];
    NSError *err;
    NSMutableDictionary *dict = [NSJSONSerialization JSONObjectWithData:jsonData
                                                        options:NSJSONReadingMutableContainers
                                                          error:&err];
    
    if(err || dict == nil)
    {
       NSLog(@"json解析失败：%@",err);
       [U3DAnalytics onTchErrorEvent:@"json_error" andRaw:json andOther:@""];
       return nil;
    }
    
    // ---------- 太极数据校验 -------------
    if(key == TchAdRevRoas001){
        // tch 001 参数修复
        [U3DAnalytics fixTchParams: dict andJson:json andMaxValue:tch001MaxValue];
    } else if(key == TchAdRevRoas02){
        // tch 02 参数修复
        [U3DAnalytics fixTchParams: dict andJson:json andMaxValue:tch02MaxValue];
    }
    
    return dict;
}

// 修复自打点数据
+(void) fixTchParams: (NSMutableDictionary *)dict andJson: (NSString *)json andMaxValue: (double) targetValue {
    // --- 保存 raw数据 ---
    [dict setValue: json forKey:@"raw"];
    
    id _platform = [dict objectForKey:@"ad_platform"];
    id _value = [dict objectForKey:@"value"];
    
    if([U3DAnalytics isNullObject: _platform]) {
        // 不存在 ad_paltform
        [U3DAnalytics onTchErrorEvent:@"no_ad_platform" andRaw:json andOther:@""];
    } else {
        if(![[_platform stringValue] isEqual:@"appstore"]){
            return;
        }
        
        // 非IAP订单
        if([U3DAnalytics isNullObject: _value] ){
            [dict setValue: [NSNumber numberWithDouble: targetValue] forKey:@"value"];
            [U3DAnalytics onTchErrorEvent:@"no_value" andRaw:json andOther:@""];
        } else {
            if([_value doubleValue] < targetValue){
                [dict setValue: [NSNumber numberWithDouble: targetValue] forKey:@"value"];
                [U3DAnalytics onTchErrorEvent:@"value_error"
                                       andRaw:json andOther: [_value stringValue]];
            }
        }
        
    }
}

// 向Unity发送数据
+(void) sendMessageToUnity: (NSString *)msg
{
    if (msg == NULL) {
        return;
    }
    // NSLog(@"--- unityInitSDK222: %@:%@", gameObjectName, callbackName);
    if(gameObjectName != nil && callbackName != nil){
        char *t1 = [U3DAnalytics finalChar: gameObjectName];
        char *t2 = [U3DAnalytics finalChar: callbackName];
        char *t3 = [U3DAnalytics finalChar: msg];
        
        UnitySendMessage(t1, t2, t3);
    }
}



// 上报 tch_error 事件
+(void) onTchErrorEvent:(NSString *) evtName andRaw: (NSString *)raw andOther: (NSString *) other{
    NSString *json = [NSString stringWithFormat:@"{\"event\":\"%@\", \"raw\":\"%@\", \"other\":\"%@\"}", evtName, raw, other];
    
    [GuruAnalytics logEvent:TchError
                 parameters:[U3DAnalytics buildDataWithJson:json andKey:evtName]];
}

// 对象判空
+ (BOOL)isNullObject:(__kindof id) obj{
    if(!obj){
        return YES;
    }
    
    if(obj == NULL){
        return YES;
    }
    
    if([obj isEqual:[NSNull null]]){
        return YES;
    }
    
    return NO;
}

//---------------------------


@end

//============================ UNITY PUBLIC API ============================

extern "C" {
 
    // 初始化自打点
    void unityInitAnalytics(const char *appId, const char *deviceInfo, bool isDebug, const char *baseUrl, const char *uploadIpAddressStr, const char *sdkVersion)
    {
//        NSLog(@"--- [iOS] init Analytics libs");
        [GuruAnalytics initializeLibWithUploadPeriodInSecond:kUploadPeriodInSecond
                                                  batchLimit:kBatchLimit
                                         eventExpiredSeconds:kEventExpiredSeconds
                                           initializeTimeout:kInitializeTimeout
                                                  saasXAPPID:[U3DAnalytics charToString:appId]
                                             saasXDEVICEINFO:[U3DAnalytics charToString:deviceInfo]
                                                 loggerDebug:isDebug
                                              guruSDKVersion:[U3DAnalytics charToString:sdkVersion]];
                                                 
        
        // 设置 baseUrl
        [U3DAnalytics setBaseUrl:baseUrl];
        
        // 设置 uploadIpAddress
        // TODO: 当前的版本并不支持 uploadIpAddress, 后面的版本将 uploadIpAddressStr 转化为 Array<NSString> 传入接口
    }
    
    // 初始化回调对象和参数
    void unityInitCallback(const char *gameObject, const char *method){
        // NSLog(@"--- unityInitSDK111: %s:%s", gameObject, method);
        gameObjectName = [NSString stringWithUTF8String:gameObject];
        callbackName = [NSString stringWithUTF8String:method];
    }

    // 设置用户ID
    void unitySetUserID(const char *uid){
        [GuruAnalytics setUserID:[U3DAnalytics charToString:uid]];
    }

    // 设置Screen
    void unitySetScreen(const char *screenName){
        [GuruAnalytics setScreen:[U3DAnalytics charToString:screenName]];
    }

    // 设置用户ADID
    void unitySetAdId(const char *adId){
        [GuruAnalytics setAdId:[U3DAnalytics charToString:adId]];
    }

    // 设置用户AdjustID
    void unitySetAdjustID(const char *adjustId){
        [GuruAnalytics setAdjustId:[U3DAnalytics charToString:adjustId]];
    }

    // 设置用户FirebaseID
    void unitySetFirebaseId(const char *firebaseId){
        [GuruAnalytics setFirebaseId:[U3DAnalytics charToString:firebaseId]];
    }
    
    // 设置用户 AppsFlyerID
    void unitySetAppsflyerId(const char *appsFlyerId){
        [GuruAnalytics setAppFlyersId:[U3DAnalytics charToString:appsFlyerId]];
    }
    
    // 设置用户设备ID
    void unitySetDeviceId(const char *did){
        [GuruAnalytics setDeviceId:[U3DAnalytics charToString:did]];
    }


    // 设置用户属性
    void unitySetUserProperty(const char *key, const char *value){
        [GuruAnalytics setUserProperty:[U3DAnalytics charToString:value]
                               forName:[U3DAnalytics charToString:key]];
    }
    
    // 上报事件
    void unityLogEvent(const char *key, const char *data){
        NSString *evtName = [U3DAnalytics charToString:key];
        NSString *json = [U3DAnalytics charToString:data];
        
        [GuruAnalytics logEvent: evtName
                     parameters:[U3DAnalytics buildDataWithJson:json andKey:evtName]]; // JSON转换
    }
    
    // 设置 Tch02 的上限值(即将废弃)
    void unitySetTch02Value(const double value){
        [U3DAnalytics setTch02MaxValue:value];
    }

    // 打点事件成功率
    void unityReportEventRate(void){
        [U3DAnalytics getEventsStatistics];
        // 上报事件总量
        [GuruAnalytics setUserProperty:[NSString stringWithFormat:@"%d", eventCountAll]
                               forName:@"lgd"];
        // 上报成功数量
        [GuruAnalytics setUserProperty:[NSString stringWithFormat:@"%d", eventCountUploaded]
                               forName:@"uld"];
    
    
//         [GuruAnalytics debug_eventsStatistics:^(NSInteger uploadedEventsCount, NSInteger loggedEventsCount) {
//
//             // 上报事件总量
//             [GuruAnalytics setUserProperty:[NSString stringWithFormat:@"%ld", loggedEventsCount]
//                                    forName:@"lgd"];
//             // 上报成功数量
//             [GuruAnalytics setUserProperty:[NSString stringWithFormat:@"%ld", uploadedEventsCount]
//                                    forName:@"uld"];
//         }];
    }

    // 注册内部日志 Error 监听
    void unitySetEnableErrorLog(bool value){
        [U3DAnalytics setEnableErrorLog:value];
    }
    
    // 上报事件数量总数
    int unityGetEventsCountAll(){
        return eventCountAll;
    }
    
    // 上报事件数量成功数量
    int unityGetEventsCountUploaded(){
        return eventCountUploaded;
    }
}
