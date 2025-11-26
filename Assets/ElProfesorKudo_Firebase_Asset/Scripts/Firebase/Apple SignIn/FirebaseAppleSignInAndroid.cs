using Firebase.Auth;
using UnityEngine;

namespace ElProfesorKudo.Firebase.AppleSignIn.Android
{
    using ElProfesorKudo.Firebase.Core;
    using ElProfesorKudo.Firebase.Event;
    using ElProfesorKudo.Firebase.Common;

    public class FirebaseAppleSignInAndroid : FirebaseAbstractAppleSignIn
    {
#if UNITY_ANDROID
        private AndroidJavaObject _appleSignInPlugin;

        protected override void Awake()
        {
            base.Awake();

            if (Application.platform != RuntimePlatform.Android)
            {
                CustomLogger.LogWarning("FirebaseAppleSignInAndroid is active but the platform is not Android. Destroying the component.");
                Destroy(this);
                return;
            }
        }

        protected override void Start()
        {
            base.Start();
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.elprofesorkudo.applesigninplugin.AppleSignInPlugin");
                _appleSignInPlugin = pluginClass.CallStatic<AndroidJavaObject>("getInstance", activity);
                string msg = pluginClass.CallStatic<string>("getHello", activity);
                CustomLogger.LogInfo("Received from Java: " + msg);
            }
        }

        #region Sign In
        public override void SignIn()
        {
            base.SignIn();
            if (_appleSignInPlugin != null)
            {
                CustomLogger.LogInfo("Android plugin Calling the Java plugin's signIn method.");
                _appleSignInPlugin.Call("signIn");
            }
            else
            {
                CustomLogger.LogError("Android AppleSignInPlugin not initialized. Unable to sign in.");
            }
        }

        protected override void OnAppleSignInSuccess(string idToken)
        {
            base.OnAppleSignInSuccess(idToken);

            CustomLogger.LogInfo("Apple ID token received: " + idToken);

            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            Credential credential = OAuthProvider.GetCredential("apple.com", idToken, "");

            auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    CustomLogger.LogError("SignInWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    CustomLogger.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }

                FirebaseUser newUser = task.Result;
                CustomLogger.LogInfo("User signed in successfully with Firebase via Apple: " +
                                     newUser.DisplayName + " (" + newUser.UserId + ")");
                FirebaseCallbacks.InvokeAppleSignInAndroidSuccess(idToken);
            });
        }

        protected override void OnAppleSignInFailed(string error)
        {
            CustomLogger.LogError("Apple Sign-In failed: " + error);
            FirebaseCallbacks.InvokeAppleSignInAndroidFailed(error);
        }
        #endregion Sign In

        #region Sign Out
        public override void SignOut()
        {
            base.SignOut();
            if (_appleSignInPlugin != null)
            {
                CustomLogger.LogInfo("Android plugin Calling the Java plugin's signOut method.");
                _appleSignInPlugin.Call("signOut");
            }
            else
            {
                CustomLogger.LogError("Android AppleSignInPlugin not initialized. Unable to sign out.");
            }
        }

        protected override void OnAppleSignOutSuccess()
        {
            CustomLogger.LogInfo("Apple Sign-Out successful.");
            FirebaseCoreService.Instance.Auth.SignOut();
            CustomLogger.LogInfo("Firebase user signed out.");
            FirebaseCallbacks.InvokeAppleSignOutIOSSuccess();
        }

        protected override void OnAppleSignOutFailed(string errorMessage)
        {
            CustomLogger.LogError("Apple Sign-Out failed from Java plugin: " + errorMessage);
            FirebaseCallbacks.InvokeAppleSignOutIOSFailed(errorMessage);
        }
        #endregion Sign Out

        #region Get/Set Sensitive Value
        private void SensitiveValueSet()
        {
            // WARNING: Hardcoding sensitive values (like CLIENT_ID or API URLs) is NOT a best practice.
            // Recommended: Retrieve these values securely from a backend service at runtime.
        }
        #endregion Get/Set Sensitive Value
#endif
    }
}
