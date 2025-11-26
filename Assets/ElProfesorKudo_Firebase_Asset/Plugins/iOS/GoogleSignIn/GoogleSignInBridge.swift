import Foundation
import GoogleSignIn
import FirebaseAuth
import FirebaseCore
import UIKit

/*@_silgen_name("UnitySendMessage")
func UnitySendMessage(_ objectName: UnsafePointer<CChar>, _ methodName: UnsafePointer<CChar>, _ message: UnsafePointer<CChar>)

// Utility function to send messages to Unity more easily
func sendUnityMessage(_ objectName: String, _ methodName: String, _ message: String) {
    if let objectNameC = objectName.cString(using: .utf8),
       let methodNameC = methodName.cString(using: .utf8),
       let messageC = message.cString(using: .utf8) {
        UnitySendMessage(objectNameC, methodNameC, messageC)
    } else {
        print("Failed to convert string to CChar for UnitySendMessage.")
    }
}*/

@objc public class GoogleSignInBridge: NSObject {
    
    private static let kWebClientID = "" // <-- Your ID Client WEB
    private static let gameObjectReceiverName = "" // <-- Name of the gameobject receiving the message


    @objc public static func forceLinking() {
        // Don't do anything just force code Swift.
    }

    @objc public static func signIn() {
        print("GoogleSignInBridge: signIn called")

        guard let presentingVC = UIApplication.shared.windows.first?.rootViewController else {
            print("GoogleSignInBridge Error: Could not find root view controller.")
            UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", "No root view controller found.")
            return
        }

        Task { // asynchrone code (async/await)
            do {
                // Call async signIn, return GIDSignInResult
                let signInResult = try await GIDSignIn.sharedInstance.signIn(
                    withPresenting: presentingVC,
                    hint: nil,
                    additionalScopes: []
                )

                // Access tokens directly via the GIDSignInResult's user object
                // user.idToken and user.accessToken are of type GIDToken?
                guard let idTokenString = signInResult.user.idToken?.tokenString else {
                    print("GoogleSignInBridge Error: ID Token missing from GIDSignInResult.")
                    UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", "ID Token missing.")
                    return
                }

                // accessToken est is a GIDToken?
                let accessTokenString = signInResult.user.accessToken.tokenString // .tokenString is a String or nil if the token doesn't exist

                let credential = GoogleAuthProvider.credential(
                    withIDToken: idTokenString,
                    accessToken: accessTokenString
                )

                Auth.auth().signIn(with: credential) { authResult, error in
                    if let error = error {
                        print("Firebase Auth Error: \(error.localizedDescription)")
                        UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", error.localizedDescription)
                        return
                    }

                    if let uid = authResult?.user.uid {
                        print("GoogleSignInBridge: Signed in with UID \(uid)")
                        UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInSuccess", uid)
                    } else {
                        print("GoogleSignInBridge Error: Firebase UID missing.")
                        UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", "Firebase UID missing.")
                    }
                }
            } catch {
                print("GoogleSignInBridge Error during sign-in: \(error.localizedDescription)")
                UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", error.localizedDescription)
            }
        }
    }

    @objc public static func signOut() {
        print("GoogleSignInBridge: signOut called")
        GIDSignIn.sharedInstance.signOut()
        UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignOutSuccess", "User signed out from Google.")
        print("GoogleSignInBridge: User signed out from Google.")
    }

    @objc public static func restoreSignIn() {
        print("GoogleSignInBridge: restoreSignIn called")

        Task {
            do {
                let user = try await GIDSignIn.sharedInstance.restorePreviousSignIn()

                guard let idToken = user.idToken?.tokenString else {
                    UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", "ID Token missing.")
                    return
                }

                let accessToken = user.accessToken.tokenString

                let credential = GoogleAuthProvider.credential(withIDToken: idToken, accessToken: accessToken)

                Auth.auth().signIn(with: credential) { authResult, error in
                    if let error = error {
                        UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", error.localizedDescription)
                        return
                    }

                    if let uid = authResult?.user.uid {
                        UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInSuccess", uid)
                    } else {
                        UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", "Firebase UID missing.")
                    }
                }
            } catch {
                UnityMessenger.send(gameObjectReceiverName, "OnGoogleSignInFailed", error.localizedDescription)
            }
        }
    }
}

