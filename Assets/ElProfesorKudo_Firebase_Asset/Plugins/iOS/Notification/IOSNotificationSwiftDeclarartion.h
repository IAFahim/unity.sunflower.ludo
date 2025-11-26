#import <Foundation/Foundation.h>

@interface IOSNotificationBridge : NSObject

+ (IOSNotificationBridge *)shared;

- (void)sendNotification:(NSString *)title body:(NSString *)body sound:(NSString *)sound;
- (void)requestPermission;

@end
