using Firebase.Auth;

namespace ElProfesorKudo.Firebase.Event
{
    using ElProfesorKudo.Firebase.Common;
    public static class FirebaseCallbacks
    {
        #region SERVICE

        private static Callback<FirebaseUser> _onUserChanged;
        public static void SubscribeUserChanged(Callback<FirebaseUser> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onUserChanged += callback;
            else
            {
                _onUserChanged -= callback;
                if (forceNull) _onUserChanged = null;
            }
        }
        public static void InvokeUserChanged(FirebaseUser user) => _onUserChanged?.Invoke(user);

        private static Callback<FirebaseUser> _onCurrentUserLoaded;
        public static void SubscribeCurrentUserLoaded(Callback<FirebaseUser> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onCurrentUserLoaded += callback;
            else
            {
                _onCurrentUserLoaded -= callback;
                if (forceNull) _onCurrentUserLoaded = null;
            }
        }
        public static void InvokeCurrentUserLoaded(FirebaseUser user) => _onCurrentUserLoaded?.Invoke(user);

        private static Callback _onFirebaseReady;
        public static void SubscribeFirebaseReady(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onFirebaseReady += callback;
            else
            {
                _onFirebaseReady -= callback;
                if (forceNull) _onFirebaseReady = null;
            }
        }
        public static void InvokeFirebaseReady() => _onFirebaseReady?.Invoke();

        #endregion SERVICE

        #region LOGIN

        private static Callback _onLoginSuccess;
        public static void SubscribeLoginSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onLoginSuccess += callback;
            else
            {
                _onLoginSuccess -= callback;
                if (forceNull) _onLoginSuccess = null;
            }
        }
        public static void InvokeLoginSuccess() => _onLoginSuccess?.Invoke();

        private static Callback<string> _onLoginFailed;
        public static void SubscribeLoginFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onLoginFailed += callback;
            else
            {
                _onLoginFailed -= callback;
                if (forceNull) _onLoginFailed = null;
            }
        }
        public static void InvokeLoginFailed(string errorMsg) => _onLoginFailed?.Invoke(errorMsg);


        // LOGOUT
        private static Callback _onLogout;
        public static void SubscribeLogout(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onLogout += callback;
            else
            {
                _onLogout -= callback;
                if (forceNull) _onLogout = null;
            }
        }
        public static void InvokeLogout() => _onLogout?.Invoke();

        #endregion LOGIN

        #region REGISTER
        private static Callback _onRegisterSuccess;
        public static void SubscribeRegisterSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onRegisterSuccess += callback;
            else
            {
                _onRegisterSuccess -= callback;
                if (forceNull) _onRegisterSuccess = null;
            }
        }
        public static void InvokeRegisterSuccess() => _onRegisterSuccess?.Invoke();

        private static Callback<string> _onRegisterFailed;
        public static void SubscribeRegisterFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onRegisterFailed += callback;
            else
            {
                _onRegisterFailed -= callback;
                if (forceNull) _onRegisterFailed = null;
            }
        }
        public static void InvokeRegisterFailed(string errorLog) => _onRegisterFailed?.Invoke(errorLog);

        #endregion REGISTER

        #region EMAIL VERIFICATION
        private static Callback<string> _onEmailVerificationSent;
        public static void SubscribeEmailVerificationSent(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onEmailVerificationSent += callback;
            else
            {
                _onEmailVerificationSent -= callback;
                if (forceNull) _onEmailVerificationSent = null;
            }
        }
        public static void InvokeEmailVerificationSent(string msg) => _onEmailVerificationSent?.Invoke(msg);

        #endregion EMAIL VERIFICATION

        #region PASSWORD RESET

        private static Callback<string> _onPasswordResetSuccess;
        public static void SubscribePasswordResetSuccess(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onPasswordResetSuccess += callback;
            else
            {
                _onPasswordResetSuccess -= callback;
                if (forceNull) _onPasswordResetSuccess = null;
            }
        }
        public static void InvokePasswordResetSuccess(string msg) => _onPasswordResetSuccess?.Invoke(msg);

        private static Callback<string> _onPasswordResetFailed;
        public static void SubscribePasswordResetFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onPasswordResetFailed += callback;
            else
            {
                _onPasswordResetFailed -= callback;
                if (forceNull) _onPasswordResetFailed = null;
            }
        }
        public static void InvokePasswordResetFailed(string errorMsg) => _onPasswordResetFailed?.Invoke(errorMsg);

        #endregion PASSWORD RESET
        

        #region DELETE CURRENT USER

        private static Callback _onDeleteCurrentUserSuccess;
        public static void SubscribeDeleteCurrentUserSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onDeleteCurrentUserSuccess += callback;
            else
            {
                _onDeleteCurrentUserSuccess -= callback;
                if (forceNull) _onDeleteCurrentUserSuccess = null;
            }
        }
        public static void InvokeDeleteCurrentUserSuccess() => _onDeleteCurrentUserSuccess?.Invoke();

        private static Callback<string> _onDeleteCurrentUserFailed;
        public static void SubscribeDeleteCurrentUserFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onDeleteCurrentUserFailed += callback;
            else
            {
                _onDeleteCurrentUserFailed -= callback;
                if (forceNull) _onDeleteCurrentUserFailed = null;
            }
        }
        public static void InvokeDeleteCurrentUserFailed(string error) => _onDeleteCurrentUserFailed?.Invoke(error);

        #endregion DELETE CURRENT USER

        #region DELETE USER DATA

        private static Callback _onDeleteUserDataSuccess;
        public static void SubscribeDeleteUserDataSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onDeleteUserDataSuccess += callback;
            else
            {
                _onDeleteUserDataSuccess -= callback;
                if (forceNull) _onDeleteUserDataSuccess = null;
            }
        }
        public static void InvokeDeleteUserDataSuccess() => _onDeleteUserDataSuccess?.Invoke();

        private static Callback<string> _onDeleteUserDataFailed;
        public static void SubscribeDeleteUserDataFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onDeleteUserDataFailed += callback;
            else
            {
                _onDeleteUserDataFailed -= callback;
                if (forceNull) _onDeleteUserDataFailed = null;
            }
        }
        public static void InvokeDeleteUserDataFailed(string error) => _onDeleteUserDataFailed?.Invoke(error);

        #endregion DELETE USER DATA

        #region GOOGLE SIGN-IN ANDROID

        private static Callback<string> _onGoogleSignInAndroidSuccess;
        public static void SubscribeGoogleSignInAndroidSuccess(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignInAndroidSuccess += callback;
            else
            {
                _onGoogleSignInAndroidSuccess -= callback;
                if (forceNull) _onGoogleSignInAndroidSuccess = null;
            }
        }
        public static void InvokeGoogleSignInAndroidSuccess(string idToken) => _onGoogleSignInAndroidSuccess?.Invoke(idToken);


        private static Callback<string> _onGoogleSignInAndroidFailed;
        public static void SubscribeGoogleSignInAndroidFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignInAndroidFailed += callback;
            else
            {
                _onGoogleSignInAndroidFailed -= callback;
                if (forceNull) _onGoogleSignInAndroidFailed = null;
            }
        }
        public static void InvokeGoogleSignInAndroidFailed(string error) => _onGoogleSignInAndroidFailed?.Invoke(error);


        private static Callback _onGoogleSignOutAndroidSuccess;
        public static void SubscribeGoogleSignOutAndroidSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignOutAndroidSuccess += callback;
            else
            {
                _onGoogleSignOutAndroidSuccess -= callback;
                if (forceNull) _onGoogleSignOutAndroidSuccess = null;
            }
        }
        public static void InvokeGoogleSignOutAndroidSuccess() => _onGoogleSignOutAndroidSuccess?.Invoke();


        private static Callback<string> _onGoogleSignOutAndroidFailed;
        public static void SubscribeGoogleSignOutAndroidFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignOutAndroidFailed += callback;
            else
            {
                _onGoogleSignOutAndroidFailed -= callback;
                if (forceNull) _onGoogleSignOutAndroidFailed = null;
            }
        }
        public static void InvokeGoogleSignOutAndroidFailed(string error) => _onGoogleSignOutAndroidFailed?.Invoke(error);

        #endregion GOOGLE SIGN-IN ANDROID

        #region GOOGLE SIGN-IN IOS

        private static Callback<string> _onGoogleSignInIOSSuccess;
        public static void SubscribeGoogleSignInIOSSuccess(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignInIOSSuccess += callback;
            else
            {
                _onGoogleSignInIOSSuccess -= callback;
                if (forceNull) _onGoogleSignInIOSSuccess = null;
            }
        }
        public static void InvokeGoogleSignInIOSSuccess(string idToken) => _onGoogleSignInIOSSuccess?.Invoke(idToken);

        private static Callback<string> _onGoogleSignInIOSFailed;
        public static void SubscribeGoogleSignInIOSFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignInIOSFailed += callback;
            else
            {
                _onGoogleSignInIOSFailed -= callback;
                if (forceNull) _onGoogleSignInIOSFailed = null;
            }
        }
        public static void InvokeGoogleSignInIOSFailed(string error) => _onGoogleSignInIOSFailed?.Invoke(error);

        private static Callback _onGoogleSignOutIOSSuccess;
        public static void SubscribeGoogleSignOutIOSSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignOutIOSSuccess += callback;
            else
            {
                _onGoogleSignOutIOSSuccess -= callback;
                if (forceNull) _onGoogleSignOutIOSSuccess = null;
            }
        }
        public static void InvokeGoogleSignOutIOSSuccess() => _onGoogleSignOutIOSSuccess?.Invoke();

        private static Callback<string> _onGoogleSignOutIOSFailed;
        public static void SubscribeGoogleSignOutIOSFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onGoogleSignOutIOSFailed += callback;
            else
            {
                _onGoogleSignOutIOSFailed -= callback;
                if (forceNull) _onGoogleSignOutIOSFailed = null;
            }
        }
        public static void InvokeGoogleSignOutIOSFailed(string error) => _onGoogleSignOutIOSFailed?.Invoke(error);

        #endregion GOOGLE SIGN-IN IOS

        #region APPLE SIGN-IN IOS

        private static Callback<string> _onAppleSignInIOSSuccess;
        public static void SubscribeAppleSignInIOSSuccess(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignInIOSSuccess += callback;
            else
            {
                _onAppleSignInIOSSuccess -= callback;
                if (forceNull) _onAppleSignInIOSSuccess = null;
            }
        }
        public static void InvokeAppleSignInIOSSuccess(string idToken) => _onAppleSignInIOSSuccess?.Invoke(idToken);

        private static Callback<string> _onAppleSignInIOSFailed;
        public static void SubscribeAppleSignInIOSFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignInIOSFailed += callback;
            else
            {
                _onAppleSignInIOSFailed -= callback;
                if (forceNull) _onAppleSignInIOSFailed = null;
            }
        }
        public static void InvokeAppleSignInIOSFailed(string error) => _onAppleSignInIOSFailed?.Invoke(error);

        private static Callback _onAppleSignOutIOSSuccess;
        public static void SubscribeAppleSignOutIOSSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignOutIOSSuccess += callback;
            else
            {
                _onAppleSignOutIOSSuccess -= callback;
                if (forceNull) _onAppleSignOutIOSSuccess = null;
            }
        }
        public static void InvokeAppleSignOutIOSSuccess() => _onAppleSignOutIOSSuccess?.Invoke();

        private static Callback<string> _onAppleSignOutIOSFailed;
        public static void SubscribeAppleSignOutIOSFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignOutIOSFailed += callback;
            else
            {
                _onAppleSignOutIOSFailed -= callback;
                if (forceNull) _onAppleSignOutIOSFailed = null;
            }
        }
        public static void InvokeAppleSignOutIOSFailed(string error) => _onAppleSignOutIOSFailed?.Invoke(error);

        #endregion APPLE SIGN-IN IOS

        #region APPLE SIGN-IN ANDROID

        private static Callback<string> _onAppleSignInAndroidSuccess;
        public static void SubscribeAppleSignInAndroidSuccess(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignInAndroidSuccess += callback;
            else
            {
                _onAppleSignInAndroidSuccess -= callback;
                if (forceNull) _onAppleSignInAndroidSuccess = null;
            }
        }
        public static void InvokeAppleSignInAndroidSuccess(string idToken) => _onAppleSignInAndroidSuccess?.Invoke(idToken);

        private static Callback<string> _onAppleSignInAndroidFailed;
        public static void SubscribeAppleSignInAndroidFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignInAndroidFailed += callback;
            else
            {
                _onAppleSignInAndroidFailed -= callback;
                if (forceNull) _onAppleSignInAndroidFailed = null;
            }
        }
        public static void InvokeAppleSignInAndroidFailed(string error) => _onAppleSignInAndroidFailed?.Invoke(error);

        private static Callback _onAppleSignOutAndroidSuccess;
        public static void SubscribeAppleSignOutAndroidSuccess(Callback callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignOutAndroidSuccess += callback;
            else
            {
                _onAppleSignOutAndroidSuccess -= callback;
                if (forceNull) _onAppleSignOutAndroidSuccess = null;
            }
        }
        public static void InvokeAppleSignOutAndroidSuccess() => _onAppleSignOutAndroidSuccess?.Invoke();

        private static Callback<string> _onAppleSignOutAndroidFailed;
        public static void SubscribeAppleSignOutAndroidFailed(Callback<string> callback, bool subscribe = true, bool forceNull = true)
        {
            if (subscribe) _onAppleSignOutAndroidFailed += callback;
            else
            {
                _onAppleSignOutAndroidFailed -= callback;
                if (forceNull) _onAppleSignOutAndroidFailed = null;
            }
        }
        public static void InvokeAppleSignOutAndroidFailed(string error) => _onAppleSignOutAndroidFailed?.Invoke(error);

        #endregion APPLE SIGN-IN ANDROID

    }
}
