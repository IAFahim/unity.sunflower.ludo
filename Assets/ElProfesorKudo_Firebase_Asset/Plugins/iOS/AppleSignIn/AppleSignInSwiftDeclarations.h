#import <Foundation/Foundation.h>

@interface AppleSignInBridge : NSObject

// Instance Method
- (void)signIn;
- (void)signOut;

// Class Method
+ (AppleSignInBridge *)shared;

@end