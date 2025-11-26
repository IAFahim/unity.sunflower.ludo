#import <Foundation/Foundation.h>
#import <UnityFramework-Swift.h>
// C-style functions exposed to Unity via P/Invoke.
// The "extern "C"" is essential for C# to call these functions.
extern "C" {

    void _ForceSwift() {
        [GoogleSignInBridge forceLinking];
    }
    // Function to start the Google Sign-In connection via your Swift code.
    void _GoogleSignIniOS_SignIn() {
        NSLog(@"[Unity-iOS Wrapper] _GoogleSignIniOS_SignIn called from C#, dispatching to Swift.");
        // Ensure you call the Swift code on the main iOS thread.
        dispatch_async(dispatch_get_main_queue(), ^{
            [GoogleSignInBridge signIn]; // Calls the static 'signIn' method of your Swift class
        });
    }

    // Function to sign out the user from Google Sign-In.
    void _GoogleSignIniOS_SignOut() {
        NSLog(@"[Unity-iOS Wrapper] _GoogleSignIniOS_SignOut called from C#, dispatching to Swift.");
        dispatch_async(dispatch_get_main_queue(), ^{
            [GoogleSignInBridge signOut]; // Calls the static 'signOut' method of your Swift class
        });
    }

    // Function to attempt a silent Google Sign-In.
    void _GoogleSignIniOS_RestoreSignIn() {
        NSLog(@"[Unity-iOS Wrapper] _GoogleSignIniOS_RestoreSignIn called from C#, dispatching to Swift.");
        dispatch_async(dispatch_get_main_queue(), ^{
            [GoogleSignInBridge restoreSignIn]; // Calls the static 'restoreSignIn' method of your Swift class
        });
    }

}
