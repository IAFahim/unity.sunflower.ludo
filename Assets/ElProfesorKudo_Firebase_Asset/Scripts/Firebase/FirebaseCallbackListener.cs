using System.Collections.Generic;
using Firebase.Auth;
using UnityEngine;

namespace ElProfesorKudo.Firebase.Event.Listener
{
    using ElProfesorKudo.Firebase.UI;
    using ElProfesorKudo.Firebase.Common;

    public class FirebaseCallbackListener : MonoBehaviour
    {
        private void OnEnable()
        {
            FirebaseCallbacks.SubscribeUserChanged(OnUserChanged);
            FirebaseCallbacks.SubscribeCurrentUserLoaded(OnCurrentUserLoaded);
            FirebaseCallbacks.SubscribeFirebaseReady(OnFirebaseReady);

            FirebaseCallbacks.SubscribeLoginSuccess(OnLoginSuccess);
            FirebaseCallbacks.SubscribeLoginFailed(OnLoginFailed);
            FirebaseCallbacks.SubscribeLogout(OnLogout);

            FirebaseCallbacks.SubscribeRegisterSuccess(OnRegisterSuccess);
            FirebaseCallbacks.SubscribeRegisterFailed(OnRegisterFailed);

            FirebaseCallbacks.SubscribeEmailVerificationSent(OnEmailVerificationSent);

            FirebaseCallbacks.SubscribePasswordResetSuccess(OnPasswordResetSuccess);
            FirebaseCallbacks.SubscribePasswordResetFailed(OnPasswordResetFailed);

            FirebaseCallbacks.SubscribeDeleteCurrentUserSuccess(OnDeleteCurrentUserSuccess);
            FirebaseCallbacks.SubscribeDeleteCurrentUserFailed(OnDeleteCurrentUserFailed);
            FirebaseCallbacks.SubscribeDeleteUserDataSuccess(OnDeleteUserDataSuccess);
            FirebaseCallbacks.SubscribeDeleteUserDataFailed(OnDeleteUserDataFailed);

            FirebaseCallbacks.SubscribeGoogleSignInAndroidSuccess(OnGoogleSignInAndroidSuccess);
            FirebaseCallbacks.SubscribeGoogleSignInAndroidFailed(OnGoogleSignInAndroidFailed);
            FirebaseCallbacks.SubscribeGoogleSignOutAndroidSuccess(OnGoogleSignOutAndroidSuccess);
            FirebaseCallbacks.SubscribeGoogleSignOutAndroidFailed(OnGoogleSignOutAndroidFailed);

            FirebaseCallbacks.SubscribeGoogleSignInIOSSuccess(OnGoogleSignInIOSSuccess);
            FirebaseCallbacks.SubscribeGoogleSignInIOSFailed(OnGoogleSignInIOSFailed);
            FirebaseCallbacks.SubscribeGoogleSignOutIOSSuccess(OnGoogleSignOutIOSSuccess);
            FirebaseCallbacks.SubscribeGoogleSignOutIOSFailed(OnGoogleSignOutIOSFailed);

            FirebaseCallbacks.SubscribeAppleSignInIOSSuccess(OnAppleSignInIOSSuccess);
            FirebaseCallbacks.SubscribeAppleSignInIOSFailed(OnAppleSignInIOSFailed);
            FirebaseCallbacks.SubscribeAppleSignOutIOSSuccess(OnAppleSignOutIOSSuccess);
            FirebaseCallbacks.SubscribeAppleSignOutIOSFailed(OnAppleSignOutIOSFailed);

            FirebaseCallbacks.SubscribeAppleSignInAndroidSuccess(OnAppleSignInAndroidSuccess);
            FirebaseCallbacks.SubscribeAppleSignInAndroidFailed(OnAppleSignInAndroidFailed);
            FirebaseCallbacks.SubscribeAppleSignOutAndroidSuccess(OnAppleSignOutAndroidSuccess);
            FirebaseCallbacks.SubscribeAppleSignOutAndroidFailed(OnAppleSignOutAndroidFailed);
        }

        private void OnDisable()
        {
            FirebaseCallbacks.SubscribeUserChanged(OnUserChanged, false);
            FirebaseCallbacks.SubscribeCurrentUserLoaded(OnCurrentUserLoaded, false);
            FirebaseCallbacks.SubscribeFirebaseReady(OnFirebaseReady, false);

            FirebaseCallbacks.SubscribeLoginSuccess(OnLoginSuccess, false);
            FirebaseCallbacks.SubscribeLoginFailed(OnLoginFailed, false);
            FirebaseCallbacks.SubscribeLogout(OnLogout, false);

            FirebaseCallbacks.SubscribeRegisterSuccess(OnRegisterSuccess, false);
            FirebaseCallbacks.SubscribeRegisterFailed(OnRegisterFailed, false);

            FirebaseCallbacks.SubscribeEmailVerificationSent(OnEmailVerificationSent, false);

            FirebaseCallbacks.SubscribePasswordResetSuccess(OnPasswordResetSuccess, false);
            FirebaseCallbacks.SubscribePasswordResetFailed(OnPasswordResetFailed, false);

            FirebaseCallbacks.SubscribeDeleteCurrentUserSuccess(OnDeleteCurrentUserSuccess, false);
            FirebaseCallbacks.SubscribeDeleteCurrentUserFailed(OnDeleteCurrentUserFailed, false);
            FirebaseCallbacks.SubscribeDeleteUserDataSuccess(OnDeleteUserDataSuccess, false);
            FirebaseCallbacks.SubscribeDeleteUserDataFailed(OnDeleteUserDataFailed, false);

            FirebaseCallbacks.SubscribeGoogleSignInAndroidSuccess(OnGoogleSignInAndroidSuccess, false);
            FirebaseCallbacks.SubscribeGoogleSignInAndroidFailed(OnGoogleSignInAndroidFailed, false);
            FirebaseCallbacks.SubscribeGoogleSignOutAndroidSuccess(OnGoogleSignOutAndroidSuccess, false);
            FirebaseCallbacks.SubscribeGoogleSignOutAndroidFailed(OnGoogleSignOutAndroidFailed, false);

            FirebaseCallbacks.SubscribeGoogleSignInIOSSuccess(OnGoogleSignInIOSSuccess, false);
            FirebaseCallbacks.SubscribeGoogleSignInIOSFailed(OnGoogleSignInIOSFailed, false);
            FirebaseCallbacks.SubscribeGoogleSignOutIOSSuccess(OnGoogleSignOutIOSSuccess, false);
            FirebaseCallbacks.SubscribeGoogleSignOutIOSFailed(OnGoogleSignOutIOSFailed, false);

            FirebaseCallbacks.SubscribeAppleSignInIOSSuccess(OnAppleSignInIOSSuccess, false);
            FirebaseCallbacks.SubscribeAppleSignInIOSFailed(OnAppleSignInIOSFailed, false);
            FirebaseCallbacks.SubscribeAppleSignOutIOSSuccess(OnAppleSignOutIOSSuccess, false);
            FirebaseCallbacks.SubscribeAppleSignOutIOSFailed(OnAppleSignOutIOSFailed, false);

            FirebaseCallbacks.SubscribeAppleSignInAndroidSuccess(OnAppleSignInAndroidSuccess, false);
            FirebaseCallbacks.SubscribeAppleSignInAndroidFailed(OnAppleSignInAndroidFailed, false);
            FirebaseCallbacks.SubscribeAppleSignOutAndroidSuccess(OnAppleSignOutAndroidSuccess, false);
            FirebaseCallbacks.SubscribeAppleSignOutAndroidFailed(OnAppleSignOutAndroidFailed, false);
        }

        // === Callbacks ===
        #region SERVICE
        private void OnUserChanged(FirebaseUser user)
        {
            CustomLogger.LogInfo("OnUserChanged callback called");
        }

        private void OnCurrentUserLoaded(FirebaseUser user)
        {
            if (user == null)
            {
                CustomLogger.LogWarning("OnCurrentUserLoaded callback called: user is null");
            }
            else
            {
                CustomLogger.LogInfo("OnCurrentUserLoaded callback called: UID = " + user.UserId + ", Email = " + user.Email);
                CustomLogger.LogDebug("FirebaseAuth.DefaultInstance.CurrentUser = " + FirebaseAuth.DefaultInstance.CurrentUser);
            }
        }

        private void OnFirebaseReady()
        {
            CustomLogger.LogInfo("OnFirebaseReady callback called");
        }
        #endregion SERVICE

        #region LOGIN
        private void OnLoginSuccess()
        {
            CustomLogger.LogInfo("OnLoginSuccess callback called");
        }

        private void OnLoginFailed(string errorMsg)
        {
            CustomLogger.LogError("OnLoginFailed callback called: " + errorMsg);
            FirebaseAuthUIController.Instance.SetTexNotification(errorMsg);
        }

        private void OnLogout()
        {
            CustomLogger.LogInfo("OnLogout callback called");
            FirebaseAuthUIController.Instance.HideAllPanel();
        }
        #endregion LOGIN

        #region REGISTER
        private void OnRegisterSuccess()
        {
            CustomLogger.LogInfo("OnRegisterSuccess callback called");
        }

        private void OnRegisterFailed(string errorMsg)
        {
            CustomLogger.LogError("OnRegisterFailed callback called: " + errorMsg);
            FirebaseAuthUIController.Instance.SetTexNotification(errorMsg);
        }
        #endregion REGISTER

        #region EMAIL VERIFICATION
        private void OnEmailVerificationSent(string emailVerificationMsg)
        {
            CustomLogger.LogInfo("OnEmailVerificationSent callback called: " + emailVerificationMsg);
            FirebaseAuthUIController.Instance.SetTexNotification(emailVerificationMsg);
        }
        #endregion EMAIL VERIFICATION

        #region PASSWORD RESET
        private void OnPasswordResetSuccess(string msg)
        {
            CustomLogger.LogInfo("OnPasswordResetSuccess callback called: " + msg);
        }

        private void OnPasswordResetFailed(string msg)
        {
            CustomLogger.LogError("OnPasswordResetFailed callback called: " + msg);
        }
        #endregion PASSWORD RESET

        #region DELETE CURRENT USER
        private void OnDeleteCurrentUserSuccess()
        {
            CustomLogger.LogInfo("OnDeleteCurrentUserSuccess callback called: User account deleted successfully.");
            FirebaseAuthUIController.Instance.SetTexNotification("User account deleted successfully.");
        }

        private void OnDeleteCurrentUserFailed(string error)
        {
            CustomLogger.LogError("OnDeleteCurrentUserFailed callback called: " + error);
            FirebaseAuthUIController.Instance.SetTexNotification(error);
        }
        #endregion DELETE CURRENT USER

        #region DELETE USER DATA
        private void OnDeleteUserDataSuccess()
        {
            CustomLogger.LogInfo("OnDeleteUserDataSuccess callback called: User data deleted successfully.");
            FirebaseAuthUIController.Instance.SetTexNotification("User data deleted successfully.");
        }

        private void OnDeleteUserDataFailed(string error)
        {
            CustomLogger.LogError("OnDeleteUserDataFailed callback called: " + error);
            FirebaseAuthUIController.Instance.SetTexNotification(error);
        }
        #endregion DELETE USER DATA

        #region GOOGLE SIGN-IN ANDROID
        private void OnGoogleSignInAndroidSuccess(string idToken)
        {
            CustomLogger.LogInfo("OnGoogleSignInAndroidSuccess callback called: ID Token = " + idToken);
        }

        private void OnGoogleSignInAndroidFailed(string error)
        {
            CustomLogger.LogError("OnGoogleSignInAndroidFailed callback called: " + error);
        }

        private void OnGoogleSignOutAndroidSuccess()
        {
            CustomLogger.LogInfo("OnGoogleSignOutAndroidSuccess callback called");
            FirebaseAuthUIController.Instance.HideAllPanel();
        }

        private void OnGoogleSignOutAndroidFailed(string error)
        {
            CustomLogger.LogError("OnGoogleSignOutAndroidFailed callback called: " + error);
        }
        #endregion GOOGLE SIGN-IN ANDROID

        #region GOOGLE SIGN-IN IOS
        private void OnGoogleSignInIOSSuccess(string idToken)
        {
            CustomLogger.LogInfo("OnGoogleSignInIOSSuccess callback called: ID Token = " + idToken);
        }

        private void OnGoogleSignInIOSFailed(string error)
        {
            CustomLogger.LogError("OnGoogleSignInIOSFailed callback called: " + error);
        }

        private void OnGoogleSignOutIOSSuccess()
        {
            CustomLogger.LogInfo("OnGoogleSignOutIOSSuccess callback called");
            FirebaseAuthUIController.Instance.HideAllPanel();
        }

        private void OnGoogleSignOutIOSFailed(string error)
        {
            CustomLogger.LogError("OnGoogleSignOutIOSFailed callback called: " + error);
        }
        #endregion GOOGLE SIGN-IN IOS

        #region APPLE SIGN-IN IOS
        private void OnAppleSignInIOSSuccess(string idToken)
        {
            CustomLogger.LogInfo("OnAppleSignInIOSSuccess callback called: ID Token = " + idToken);
        }

        private void OnAppleSignInIOSFailed(string error)
        {
            CustomLogger.LogError("OnAppleSignInIOSFailed callback called: " + error);
            FirebaseAuthUIController.Instance.SetTexNotification(error);
        }

        private void OnAppleSignOutIOSSuccess()
        {
            CustomLogger.LogInfo("OnAppleSignOutIOSSuccess callback called");
            FirebaseAuthUIController.Instance.HideAllPanel();
        }

        private void OnAppleSignOutIOSFailed(string error)
        {
            CustomLogger.LogError("OnAppleSignOutIOSFailed callback called: " + error);
            FirebaseAuthUIController.Instance.SetTexNotification(error);
        }
        #endregion APPLE SIGN-IN IOS

        #region APPLE SIGN-IN ANDROID
        private void OnAppleSignInAndroidSuccess(string idToken)
        {
            CustomLogger.LogInfo("OnAppleSignInAndroidSuccess callback called: ID Token = " + idToken);
        }

        private void OnAppleSignInAndroidFailed(string error)
        {
            CustomLogger.LogError("OnAppleSignInAndroidFailed callback called: " + error);
            FirebaseAuthUIController.Instance.SetTexNotification(error);
        }

        private void OnAppleSignOutAndroidSuccess()
        {
            CustomLogger.LogInfo("OnAppleSignOutAndroidSuccess callback called");
            FirebaseAuthUIController.Instance.HideAllPanel();
        }

        private void OnAppleSignOutAndroidFailed(string error)
        {
            CustomLogger.LogError("OnAppleSignOutAndroidFailed callback called: " + error);
            FirebaseAuthUIController.Instance.SetTexNotification(error);
        }
        #endregion APPLE SIGN-IN ANDROID
    }
}
