#import <Foundation/Foundation.h>
#import <UnityFramework/UnityFramework-Swift.h>

extern "C" {

    void _IOSNotification_RequestPermission() {
        NSLog(@"[Unity-iOS Wrapper] RequestPermission called");
        dispatch_async(dispatch_get_main_queue(), ^{
            [[IOSNotificationBridge shared] requestPermission];
        });
    }

    void _IOSNotification_Send(const char* title, const char* body, const char* sound) {
        NSString* nsTitle = title ? [NSString stringWithUTF8String:title] : @"";
        NSString* nsBody  = body ? [NSString stringWithUTF8String:body] : @"";
        NSString* nsSound = (sound && strlen(sound) > 0) ? [NSString stringWithUTF8String:sound] : nil;

        NSLog(@"[Unity-iOS Wrapper] SendNotification called with title=%@ body=%@", nsTitle, nsBody);

        dispatch_async(dispatch_get_main_queue(), ^{
            [[IOSNotificationBridge shared] sendNotification:nsTitle body:nsBody sound:nsSound];
        });
    }
}
