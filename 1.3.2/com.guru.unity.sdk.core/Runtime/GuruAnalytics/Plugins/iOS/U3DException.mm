//
//  JJException.m
//  UnityFramework
//
//  Created by Castbox on 2023/2/28.
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import "UnityAppController+UnityInterface.h"
#import <JJException/JJException.h>
#import <FirebaseCrashlytics/FirebaseCrashlytics.h>

@interface U3DException : NSObject<JJExceptionHandle>


@end

@implementation U3DException

static U3DException *_instance;

+ (instancetype)sharedInstance {
    static dispatch_once_t oneToken;
    dispatch_once(&oneToken,^{
        _instance = [[self alloc]init];
    });
    return _instance;
}

+ (instancetype)allocWithZone:(NSZone *)zone{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _instance = [super allocWithZone:zone];
    });
    return _instance;
}

- (void)start {
    [JJException configExceptionCategory:JJExceptionGuardUnrecognizedSelector];
    [JJException startGuardException];
    [JJException registerExceptionHandle:self];
}

#pragma mark - Exception Delegate

- (void)handleCrashException:(NSString*)exceptionMessage extraInfo:(NSDictionary*)info{
    NSLog(@"handleCrashException: %@ info: %@", exceptionMessage, info);
    NSArray *messages = [exceptionMessage componentsSeparatedByString:@"\n"];
    NSString *domain = @"[U3DException]-handler exception";
    if (messages.count > 2) {
//        messages.objectat
        domain = [messages objectAtIndex:2];
    }
    NSError *error = [[NSError alloc] initWithDomain:domain code:-1 userInfo:@{NSLocalizedDescriptionKey: exceptionMessage}];
    [[FIRCrashlytics crashlytics] recordError:error];
}

@end


extern "C" {
    void unityInitException() {
        [[U3DException sharedInstance] start];
    }
    
    void unityTestUnrecognizedSelectorCrash() {
        [[U3DException sharedInstance] performSelector:NSSelectorFromString(@"testUnrecognizedSelectorCrash")];
    }
}
