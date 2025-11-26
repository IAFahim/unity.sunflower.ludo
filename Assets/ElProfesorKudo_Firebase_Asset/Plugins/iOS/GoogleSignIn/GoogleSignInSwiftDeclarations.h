#import <Foundation/Foundation.h>

@interface GoogleSignInBridge : NSObject

// Declares the static methods (class methods) of the Swift class
+ (void)forceLinking;
+ (void)signIn;
+ (void)signOut;
+ (void)restoreSignIn;

@end
