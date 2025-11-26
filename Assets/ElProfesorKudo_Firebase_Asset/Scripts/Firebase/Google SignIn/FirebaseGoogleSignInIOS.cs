using System.Runtime.InteropServices;
using UnityEngine;
using Firebase.Auth;

namespace ElProfesorKudo.Firebase.GoogleSignIn.iOS
{
    using ElProfesorKudo.Firebase.Common;
    using ElProfesorKudo.Firebase.Event;

    public class FirebaseGoogleSignInIOS : FirebaseAbstractGoogleSignIn
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _GoogleSignIniOS_SignIn();
        [DllImport("__Internal")]
        private static extern void _GoogleSignIniOS_SignOut();
        [DllImport("__Internal")]
        private static extern void _GoogleSignIniOS_RestoreSignIn();

        protected override void Awake()
        {
            base.Awake();

            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                CustomLogger.LogWarning("GoogleSignInManagerIOS is active but the platform is not iOS. Destroying the component.");
                Destroy(this);
                return;
            }
        }

        protected override void Start()
        {
            base.Start();
            CustomLogger.LogInfo("iOS plugin Silent Google login restoration attempt...");
            _GoogleSignIniOS_RestoreSignIn();
        }

        #region Sign In
        public override void SignIn()
        {
            base.SignIn();
            CustomLogger.LogInfo("iOS plugin Calling the native _GoogleSignIniOS_SignIn method");
            _GoogleSignIniOS_SignIn();
        }

        protected override void OnGoogleSignInSuccess(string idToken)
        {
            base.OnGoogleSignInSuccess(idToken);
            string firebaseUid = idToken; // Pour iOS, c'est l'UID Firebase directement
            CustomLogger.LogInfo("[iOS] Google Sign-In success, UID Firebase: " + firebaseUid);

            FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser != null && currentUser.UserId == firebaseUid)
            {
                CustomLogger.LogInfo("iOS plugin Firebase user already logged in via iOS: " + currentUser.DisplayName + " (" + currentUser.UserId + ")");
                FirebaseCallbacks.InvokeGoogleSignInIOSSuccess(firebaseUid);
            }
            else
            {
                CustomLogger.LogError("iOS plugin UID mismatch or Firebase user not logged in after Swift callback. Expected UID: " + firebaseUid + ", current UID: " + (currentUser?.UserId ?? "N/A"));
            }
        }

        protected override void OnGoogleSignInFailed(string errorMessage)
        {
            base.OnGoogleSignInFailed(errorMessage);
            CustomLogger.LogError("Google Sign-In failed: " + errorMessage);
            FirebaseCallbacks.InvokeGoogleSignInIOSFailed(errorMessage);
        }
        #endregion Sign In

        #region Sign Out
        public override void SignOut()
        {
            base.SignOut();
            FirebaseAuth.DefaultInstance.SignOut();
            CustomLogger.LogInfo("User log out of Firebase Authentication");
            CustomLogger.LogInfo("iOS plugin call native function _GoogleSignIniOS_SignOut.");
            _GoogleSignIniOS_SignOut();
        }

        protected override void OnGoogleSignOutSuccess()
        {
            base.OnGoogleSignOutSuccess();
            CustomLogger.LogInfo("Google Sign-Out successful.");
            FirebaseCallbacks.InvokeGoogleSignOutIOSSuccess();
        }

        protected override void OnGoogleSignOutFailed(string errorMessage)
        {
            base.OnGoogleSignOutFailed(errorMessage);
            CustomLogger.LogError("Google Sign-Out failed: " + errorMessage);
            FirebaseCallbacks.InvokeGoogleSignOutIOSFailed(errorMessage);
        }
        #endregion Sign Out

#endif
    }
}
