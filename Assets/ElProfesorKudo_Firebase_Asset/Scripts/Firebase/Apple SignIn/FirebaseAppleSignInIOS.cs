using System.Runtime.InteropServices;
using UnityEngine;
using Firebase.Auth;

namespace ElProfesorKudo.Firebase.AppleSignIn.iOS
{
    using ElProfesorKudo.Firebase.Common;
    using ElProfesorKudo.Firebase.Event;

    public class FirebaseAppleSignInIOS : FirebaseAbstractAppleSignIn
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _AppleSignIniOS_SignIn();

        [DllImport("__Internal")]
        private static extern void _AppleSignIniOS_SignOut();

        protected override void Awake()
        {
            base.Awake();

            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                CustomLogger.LogWarning("FirebaseAppleSignInIOS is active but the platform is not iOS. Destroying the component.");
                Destroy(this);
                return;
            }
        }

        protected override void Start()
        {
        }

        public override void SignIn()
        {
            base.SignIn();
            _AppleSignIniOS_SignIn();
        }

        protected override void OnAppleSignInSuccess(string idToken)
        {
            CustomLogger.LogInfo("Apple Sign-In success");

            FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser != null && currentUser.UserId == idToken)
            {
                CustomLogger.LogInfo("Apple Sign-In, Firebase user is: " + currentUser.DisplayName + " (" + currentUser.UserId + ")");
                FirebaseCallbacks.InvokeAppleSignInIOSSuccess(idToken);
            }
            else
            {
                CustomLogger.LogError("Apple Sign-In UID mismatch or Firebase not initialized. ID token: " + idToken);
                FirebaseCallbacks.InvokeAppleSignInIOSFailed("UID mismatch or Firebase not initialized.");
            }
        }

        protected override void OnAppleSignInFailed(string error)
        {
            CustomLogger.LogError("Apple Sign-In failed: " + error);
            FirebaseCallbacks.InvokeAppleSignInIOSFailed(error);
        }

        public override void SignOut()
        {
            base.SignOut();
            _AppleSignIniOS_SignOut();
        }

        protected override void OnAppleSignOutSuccess()
        {
            CustomLogger.LogInfo("Apple Sign-Out successful.");
            FirebaseCallbacks.InvokeAppleSignOutIOSSuccess();
        }

        protected override void OnAppleSignOutFailed(string errorMessage)
        {
            CustomLogger.LogError("Apple Sign-Out failed: " + errorMessage);
            FirebaseCallbacks.InvokeAppleSignOutIOSFailed(errorMessage);
        }
#endif
    }
}
