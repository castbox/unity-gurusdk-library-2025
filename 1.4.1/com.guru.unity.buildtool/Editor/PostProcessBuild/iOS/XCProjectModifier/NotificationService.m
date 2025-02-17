//
//  NotificationService.m
//  U3D2FCM
//
//  Created by Michael on 2020/11/27.
//

#import "NotificationService.h"
#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>

 
@interface NotificationService ()

@property (nonatomic, strong) void (^contentHandler)(UNNotificationContent *contentToDeliver);
@property (nonatomic, strong) UNMutableNotificationContent *bestAttemptContent;

@end



@implementation NotificationService


- (void)didReceiveNotificationRequest:(UNNotificationRequest *)request withContentHandler:(void (^)(UNNotificationContent * _Nonnull))contentHandler {
    self.contentHandler = contentHandler;
    self.bestAttemptContent = [request.content mutableCopy];
    
    NSLog(@"EventPush-NotificationService-didReceiveNotificationRequest");

    NSString *packageName = @"";
    NSArray *arr = [[[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleIdentifier"] componentsSeparatedByString:@"."];
    for(int i=0;i<arr.count-1;i++){
        if(i==arr.count-2){
            packageName = [packageName stringByAppendingString:arr[i]];
        }else{
            packageName = [packageName stringByAppendingString: [NSString stringWithFormat:@"%@.",arr[i]]];
        }
    }
    NSLog(@"EventPush-NotificationService-packageName: %@", packageName);
    NSUserDefaults *defaults = [[NSUserDefaults alloc] initWithSuiteName:  [NSString stringWithFormat: @"group.%@", packageName]];
    NSString *appCountry= [defaults stringForKey:@"appCountry"];
    NSString *appIdentifier = [defaults stringForKey:@"appIdentifier"];
    NSString *appVersion = [defaults stringForKey:@"appVersion"];
    NSString *deviceCountry = [defaults stringForKey:@"deviceCountry"];
    NSString *deviceId = [defaults stringForKey:@"deviceId"];
    NSString *deviceToken = [defaults stringForKey:@"deviceToken"];
    NSString *eventUrl = [defaults stringForKey:@"eventUrl"];
    NSString *IPM_X_APP_ID = [defaults stringForKey:@"IPM_X_APP_ID"];
    NSString *IPM_TOKEN = [defaults stringForKey:@"IPM_TOKEN"];
    NSString *IPM_UID = [defaults stringForKey:@"IPM_UID"];

    //timezone
    NSTimeZone *zone = [NSTimeZone localTimeZone];
    NSString *timezone = [zone name];
    //model
    UIDevice *currentDevice = [UIDevice currentDevice];
    NSString *model = [currentDevice model];
    //language
    NSArray *languageArray = [NSLocale preferredLanguages];
    NSString *language = [languageArray objectAtIndex:0];
    //locale
    NSLocale *localeObj = [NSLocale currentLocale];
    NSString *locale = [localeObj localeIdentifier];
    
    NSDate *currentDate = [[NSDate alloc] init];
    NSDateFormatter *dateFormatter = [[NSDateFormatter alloc] init];
    [dateFormatter setTimeZone:[NSTimeZone timeZoneWithAbbreviation:@"UTC"]];
    [dateFormatter setDateFormat:@"yyyy-MM-dd'T'HH:mm:ss'Z'"];
    NSString *appEventTime = [dateFormatter stringFromDate:currentDate];

    NSString *deviceData = [NSString stringWithFormat: @"{\"androidId\":null,\"appCountry\":\"%@\",\"appIdentifier\":\"%@\",\"appVersion\":\"%@\",\"brand\":null,\"deviceCoordinates\":{\"latitude\":0,\"longitude\":0},\"deviceCountry\":\"%@\",\"deviceId\":\"%@\",\"deviceToken\":\"%@\",\"deviceType\":\"iOS\",\"gpsCoordinates\":{\"latitude\":0,\"longitude\":0},\"groups\":null,\"language\":\"%@\",\"locale\":\"%@\",\"model\":\"%@\",\"pushDeviceType\":\"iOS\",\"pushNotificationEnable\":true,\"pushNotifications\":null,\"pushType\":\"FCM\",\"timezone\":\"%@\",\"uid\":\"%@\"}", appCountry,appIdentifier,appVersion,deviceCountry,deviceId,deviceToken,language,locale,model,timezone,IPM_UID];
    NSString *postData = [NSString stringWithFormat: @"{\"appEventTime\":\"%@\",\"deviceData\":%@, \"eventType\":\"DeviceReceive\",\"serverParams\":\"{\\\"itemIndex\\\":0,\\\"pushEventId\\\":\\\"test123\\\",\\\"serverPushTime\\\":\\\"2020-11-27T08:48:39Z\\\",\\\"silent\\\":true,\\\"taskName\\\":\\\"pushTest-dof\\\"}\" }",appEventTime,deviceData];


    NSLog(@"EventPush-NotificationService-PlayerPrefs: %@", postData);
    NSLog(@"EventPush-NotificationService-eventUrl: %@", eventUrl);

    NSURL *url = [NSURL URLWithString:eventUrl];
    NSMutableURLRequest *httpRequest = [NSMutableURLRequest requestWithURL:url cachePolicy:NSURLRequestUseProtocolCachePolicy timeoutInterval:60.0];
    NSData *requestData = [postData dataUsingEncoding:NSUTF8StringEncoding];
    [httpRequest setHTTPMethod:@"POST"];
    [httpRequest setValue:IPM_X_APP_ID forHTTPHeaderField:@"X-APP-ID"];
    [httpRequest setValue:IPM_TOKEN forHTTPHeaderField:@"X-ACCESS-TOKEN"];
    [httpRequest setValue:IPM_UID forHTTPHeaderField:@"X-UID"];
    [httpRequest setValue:@"application/json" forHTTPHeaderField:@"Content-Type"];
    
    [httpRequest setValue:[NSString stringWithFormat:@"%d", [requestData length]] forHTTPHeaderField:@"Content-Length"];
    [httpRequest setHTTPBody: requestData];

    NSURLSessionConfiguration *config = [NSURLSessionConfiguration defaultSessionConfiguration];
    NSURLSession *session = [NSURLSession sessionWithConfiguration:config];
    __block NSURLSessionDataTask *task = [session dataTaskWithRequest:httpRequest completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
        if (error!=nil)
        {
            [task suspend];
        }
        else
        {
            NSString *requestReply = [[NSString alloc] initWithData:data encoding:NSASCIIStringEncoding];
            NSLog(@"EventPush-NotificationService-response: %@", requestReply);
            [task suspend];
        }
    }];
    [task resume];
    
    // Modify the notification content here...
    //self.bestAttemptContent.title = [NSString stringWithFormat:@"%@ [modified]", self.bestAttemptContent.title];
    
    self.contentHandler(self.bestAttemptContent);
}

- (void)serviceExtensionTimeWillExpire {
    // Called just before the extension will be terminated by the system.
    // Use this as an opportunity to deliver your "best attempt" at modified content, otherwise the original push payload will be used.
    self.contentHandler(self.bestAttemptContent);
}


@end


