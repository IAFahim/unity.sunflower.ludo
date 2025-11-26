#import <Foundation/Foundation.h>
#import <UnityFramework-Swift.h> /

extern "C" {

    void _AppleSignIniOS_SignIn() {
        NSLog(@"[Unity-iOS Wrapper] _AppleSignIniOS_SignIn called from C#, dispatching to Swift.");
        dispatch_async(dispatch_get_main_queue(), ^{
            [[AppleSignInBridge shared] signIn];
        });
    }

    void _AppleSignIniOS_SignOut() {
        NSLog(@"[Unity-iOS Wrapper] _AppleSignIniOS_SignOut called from C#, dispatching to Swift.");
        dispatch_async(dispatch_get_main_queue(), ^{
            [[AppleSignInBridge shared] signOut];
        });
    }

}