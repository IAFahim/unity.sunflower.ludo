using UnityEngine;
using Firebase.Auth;

namespace ElProfesorKudo.Firebase.GoogleSignIn.Android
{
    using ElProfesorKudo.Firebase.Event;
    using ElProfesorKudo.Firebase.Common;

    public class FirebaseGoogleSignInAndroid : FirebaseAbstractGoogleSignIn
    {
#if UNITY_ANDROID
        private AndroidJavaObject _googleSignInPlugin;

        protected override void Awake()
        {
            base.Awake();

            if (Application.platform != RuntimePlatform.Android)
            {
                CustomLogger.LogWarning("FirebaseGoogleSignInAndroid is active but the platform is not Android. Destroying the component.");
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
                AndroidJavaClass pluginClass = new AndroidJavaClass("com.elprofesorkudo.googlesigninplugin.GoogleSignInPlugin");
                _googleSignInPlugin = pluginClass.CallStatic<AndroidJavaObject>("getInstance", activity);
                string msg = pluginClass.CallStatic<string>("getHello", activity);
                CustomLogger.LogInfo("Received from Java: " + msg);
            }
        }

        #region Sign In
        public override void SignIn()
        {
            base.SignIn();
            if (_googleSignInPlugin != null)
            {
                CustomLogger.LogInfo("Android plugin Calling the Java plugin's signIn method.");
                _googleSignInPlugin.Call("signIn");
            }
            else
            {
                CustomLogger.LogError("Android GoogleSignInPlugin not initialized. Unable to sign in.");
            }
        }

        protected override void OnGoogleSignInSuccess(string idToken)
        {
            base.OnGoogleSignInSuccess(idToken);

            FirebaseAuth auth = FirebaseAuth.DefaultInstance;
            Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

            auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    CustomLogger.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    CustomLogger.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }

                FirebaseUser newUser = task.Result.User;
                CustomLogger.LogInfo("User signed in successfully with Firebase: " + newUser.DisplayName + " (" + newUser.UserId + ")");

                FirebaseCallbacks.InvokeGoogleSignInAndroidSuccess(idToken);
            });
        }

        protected override void OnGoogleSignInFailed(string errorMessage)
        {
            base.OnGoogleSignInFailed(errorMessage);
            CustomLogger.LogError("Google Sign-In failed: " + errorMessage);
            FirebaseCallbacks.InvokeGoogleSignInAndroidFailed(errorMessage);
        }
        #endregion Sign In

        #region Sign Out
        public override void SignOut()
        {
            base.SignOut();
            FirebaseAuth.DefaultInstance.SignOut();
            CustomLogger.LogInfo("User log out of Firebase Authentication");

            if (_googleSignInPlugin != null)
            {
                CustomLogger.LogInfo("Android plugin Calling the Java plugin's signOut method");
                _googleSignInPlugin.Call("signOut");
            }
            else
            {
                CustomLogger.LogWarning("Android GoogleSignInPlugin not initialized. Unable to sign out of Google.");
            }
        }

        protected override void OnGoogleSignOutSuccess()
        {
            base.OnGoogleSignOutSuccess();
            CustomLogger.LogInfo("Google Sign-Out successful.");
            FirebaseCallbacks.InvokeGoogleSignOutAndroidSuccess();
        }

        protected override void OnGoogleSignOutFailed(string errorMessage)
        {
            base.OnGoogleSignOutFailed(errorMessage);
            CustomLogger.LogError("Google Sign-Out failed: " + errorMessage);
            FirebaseCallbacks.InvokeGoogleSignOutAndroidFailed(errorMessage);
        }
        #endregion Sign Out

#endif
    }
}
