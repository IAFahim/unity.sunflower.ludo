import Foundation
import AuthenticationServices
import CryptoKit
import FirebaseAuth
import FirebaseCore
import UIKit


private func randomNonceString(length: Int = 32) -> String {
    precondition(length > 0)
    let charset: Array<Character> =
        Array("0123456789ABCDEFGHIJKLMNOPQRSTUVXYZabcdefghijklmnopqrstuvwxyz-._")
    var result = ""
    var remainingLength = length

    while remainingLength > 0 {
        let randoms: [UInt8] = (0..<16).map { _ in
            var random: UInt8 = 0
            let errorCode = SecRandomCopyBytes(kSecRandomDefault, 1, &random)
            if errorCode != errSecSuccess {
                fatalError("Unable to generate nonce. SecRandomCopyBytes failed with OSStatus \(errorCode)")
            }
            return random
        }

        randoms.forEach { random in
            if remainingLength == 0 { return }

            if random < charset.count {
                result.append(charset[Int(random)])
                remainingLength -= 1
            }
        }
    }

    return result
}

private func sha256(_ input: String) -> String {
    let inputData = Data(input.utf8)
    let hashed = SHA256.hash(data: inputData)
    return hashed.compactMap { String(format: "%02x", $0) }.joined()
}

private class AppleSignInHandler: NSObject, ASAuthorizationControllerDelegate, ASAuthorizationControllerPresentationContextProviding {
    
    private static let gameObjectReceiverName = "GameObjectReceiver"
    private var currentNonce: String?

    func startSignIn() {
        currentNonce = randomNonceString()
        let hashedNonce = sha256(currentNonce!)

        let appleIDProvider = ASAuthorizationAppleIDProvider()
        let request = appleIDProvider.createRequest()
        request.requestedScopes = [.fullName, .email]
        request.nonce = hashedNonce

        let controller = ASAuthorizationController(authorizationRequests: [request])
        controller.delegate = self
        controller.presentationContextProvider = self
        controller.performRequests()
    }

    func startSignOut() {
        do {
            try Auth.auth().signOut()
            UnityMessenger.send(Self.gameObjectReceiverName, "OnAppleSignOutSuccess", "User signed out from Apple.")
            print("[AppleSignInHandler] User signed out successfully.")
        } catch let signOutError as NSError {
            UnityMessenger.send(Self.gameObjectReceiverName, "OnAppleSignOutFailed", signOutError.localizedDescription)
            print("[AppleSignInHandler] Error signing out: \(signOutError.localizedDescription)")
        }
    }
    
    
    func presentationAnchor(for controller: ASAuthorizationController) -> ASPresentationAnchor {
        return UIApplication.shared.windows.first { $0.isKeyWindow } ?? UIWindow()
    }

    func authorizationController(controller: ASAuthorizationController, didCompleteWithAuthorization authorization: ASAuthorization) {
        guard let appleIDCredential = authorization.credential as? ASAuthorizationAppleIDCredential,
              let identityToken = appleIDCredential.identityToken,
              let tokenString = String(data: identityToken, encoding: .utf8),
              let nonce = currentNonce else {
            UnityMessenger.send(Self.gameObjectReceiverName, "OnAppleSignInFailed", "Missing token or nonce.")
            return
        }

        let credential = OAuthProvider.credential(
            providerID: AuthProviderID.apple,
            idToken: tokenString,
            rawNonce: nonce
        )

        Auth.auth().signIn(with: credential) { (authResult, error) in
            if let error = error {
                print("[AppleSignInBridge] Firebase sign in failed: \(error.localizedDescription)")
                UnityMessenger.send(Self.gameObjectReceiverName, "OnAppleSignInFailed", error.localizedDescription)
                return
            }

            print("[AppleSignInBridge] Firebase sign in success.")
            let uid = authResult?.user.uid ?? "unknown"
            UnityMessenger.send(Self.gameObjectReceiverName, "OnAppleSignInSuccess", uid)
        }
    }

    func authorizationController(controller: ASAuthorizationController, didCompleteWithError error: Error) {
        UnityMessenger.send(Self.gameObjectReceiverName, "OnAppleSignInFailed", error.localizedDescription)
    }
}

@objc public class AppleSignInBridge: NSObject {

    private let signInHandler = AppleSignInHandler()

    @objc public static let shared = AppleSignInBridge()

    @objc public func signIn() {
        print("[AppleSignInBridge] Calling Swift Apple Sign-In handler.")
        signInHandler.startSignIn()
    }

    @objc public func signOut() {
        print("[AppleSignInBridge] Apple sign-out")
        signInHandler.startSignOut()
    }
}
