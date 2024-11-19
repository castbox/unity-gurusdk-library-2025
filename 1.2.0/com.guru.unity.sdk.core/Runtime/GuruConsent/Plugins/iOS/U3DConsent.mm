// ==============================================
//  U3DConsent.mm
//  UnityFramework
//
//  Created by EricHu on 2022/11/17.
//  Copyright © 2022 guru. All rights reserved.
// ==============================================

#import <Foundation/Foundation.h>
#import <GuruConsent/GuruConsent-Swift.h>
#import "UnityAppController+UnityInterface.h"

@interface U3DConsent : NSObject

//+ (void)requestGDPR: (NSString *)deviceId :(int) debugGeo;
//+ (UIViewController *) getUnityViewController;

@end

static NSString *gameobjectName;
static NSString *callbackName;


@implementation U3DConsent


+(UnityAppController *)GetAppController
{
    return (UnityAppController*)[UIApplication sharedApplication].delegate;
}


+(UIViewController *) getUnityViewController {
    return UnityGetGLViewController();
}


// 请求GDPR的接口
+(void) requestGDPR: (NSString *)deviceId :(int)debugGeography
{
//     NSLog(@"--- deviceId: %@", deviceId);
    if(deviceId.length){
        NSLog(@"--- set debug device Id: %@", deviceId);
        GuruConsentDebugSettings *debug = [[GuruConsentDebugSettings alloc] init];
        debug.testDeviceIdentifiers = @[deviceId];
        debug.geography = (GuruConsentDebugSettingsGeography)debugGeography;
        GuruConsent.debug = debug;
    }
    
    
    // 开始请求
    [GuruConsent startFrom: [U3DConsent getUnityViewController]
                   success:^(enum GuruConsentGDPRStatus status) {
            
        if (@available(iOS 14, *)) {
//            NSLog(@"ATT 结果: %lu",
//                  (unsigned long)ATTrackingManager.trackingAuthorizationStatus);
        }
        
        NSString *msg = @"";
        
        switch (status) {
            case GuruConsentGDPRStatusUnknown:
                NSLog(@"--- GuruConsentGDPRStatusUnknown");
                msg = @"GuruConsentGDPRStatusUnknown";
                break;
                
            case GuruConsentGDPRStatusRequired:
                NSLog(@"--- GuruConsentGDPRStatusRequired");
                msg = @"GuruConsentGDPRStatusRequired";
                break;
                
            case GuruConsentGDPRStatusNotRequired:
                NSLog(@"--- GuruConsentGDPRStatusNotRequired");
                msg = @"GuruConsentGDPRStatusNotRequired";
                break;
                    
            case GuruConsentGDPRStatusObtained:
                NSLog(@"--- GuruConsentGDPRStatusObtained");
                msg = @"GuruConsentGDPRStatusObtained";
                break;
                
            default:
                break;
        }
        NSLog(@"GDPR 结果: %ld", (long)status);
        
        // 发送数据
        [U3DConsent sendMessage: [U3DConsent buildDataString: (int)status andMessage:msg]];
            
    } failure:^(NSError * _Nonnull error) {
        NSLog(@"失败: %@", error);
        [U3DConsent sendMessage: [U3DConsent buildDataString: -100 andMessage:@"request failed"]];
    }];
}


+(char*) finalChar: (NSString *) string
{
    if (string == NULL) return NULL;
        
    const char *tmpChar = [string cStringUsingEncoding:NSASCIIStringEncoding];
    if (tmpChar == NULL) return NULL;
    
    char* res = (char*)malloc(strlen(tmpChar) + 1);
    strcpy(res, tmpChar);
    
    return res;
}


// 构建数据
+(NSString *) buildDataString: (int)status andMessage: (NSString *)msg{
    
    NSString *jsonString = [NSString stringWithFormat: @"{\"action\":\"gdpr\",\"data\":{\"status\":%d,\"msg\":\"%@\"}}", status, msg];
    
//    NSDictionary *dict = @{@"action" : @"gdpr"};
//    [dict setValue:@{@"status" : [NSString stringWithFormat:@"%d",status], @"msg": msg} forKey:@"data"];
//
//    NSData *data = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:nil];
//    NSString *jsonString = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
    
    return jsonString;
}


// 向Unity发送数据
+(void) sendMessage: (NSString *)msg
{
    // NSLog(@"--- unityInitSDK222: %@:%@", gameobjectName, callbackName);
    if(gameobjectName != nil && callbackName != nil){
        char *t1 = [U3DConsent finalChar: gameobjectName];
        char *t2 = [U3DConsent finalChar: callbackName];
        char *t3 = [U3DConsent finalChar: msg];
        
        UnitySendMessage(t1, t2, t3);
    }
}
@end


extern "C" {
 
    // 请求GDPR
    void unityRequestGDPR(const char * value, int debugGeo){
        [U3DConsent requestGDPR:[NSString stringWithUTF8String:value] :debugGeo];
    }
    
    // 初始化SDK
    void unityInitSDK(const char *gameobject, const char *method){
        // NSLog(@"--- unityInitSDK111: %s:%s", gameobject, method);
        gameobjectName = [NSString stringWithUTF8String:gameobject];
        callbackName = [NSString stringWithUTF8String:method];
    }
    
    // 获取 TFC 提交状态码
    char* unityGetTCFValue(){
        NSString *purposeConsents = [NSUserDefaults.standardUserDefaults
                                     stringForKey:@"IABTCF_PurposeConsents"];
        return [U3DConsent finalChar: purposeConsents];
    }
    
    // 获取 regionCode
    char* unityGetRegionCode(){
        NSString *code;
        if (@available(iOS 17.0, *)) {
            code = [NSLocale currentLocale].regionCode;
        } else {
            code = [NSLocale currentLocale].countryCode;
        }
        if(code == Nil) code = @"";
        NSLog(@"Get Country Code: %@", code);
        return [U3DConsent finalChar:code];
    }

}
