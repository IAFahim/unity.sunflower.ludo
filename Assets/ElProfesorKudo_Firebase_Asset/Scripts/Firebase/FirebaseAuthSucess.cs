using UnityEngine;
using Firebase.Auth;
using TMPro;

namespace ElProfesorKudo.Firebase.Auth
{
    using ElProfesorKudo.Firebase.Core;
    using ElProfesorKudo.Firebase.Event;
    using ElProfesorKudo.Firebase.Common;
    using ElProfesorKudo.Firebase.UI;

    /// <summary>
    /// Handles checking if a user is already logged in on startup
    /// and shows appropriate UI based on authentication state
    /// </summary>
    public class FirebaseAuthStateHandler : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text notificationText;

        private bool _hasCheckedInitialState = false;

        private void OnEnable()
        {
            // Subscribe to Firebase ready event
            FirebaseCallbacks.SubscribeFirebaseReady(OnFirebaseReady);
            
            // Subscribe to user loaded event
            FirebaseCallbacks.SubscribeCurrentUserLoaded(OnCurrentUserLoaded);
            
            // Subscribe to login/logout events
            FirebaseCallbacks.SubscribeLoginSuccess(OnLoginSuccess);
            FirebaseCallbacks.SubscribeLogout(OnLogout);
            
            // Subscribe to Google Sign-In events
            FirebaseCallbacks.SubscribeGoogleSignInAndroidSuccess(OnGoogleSignInSuccess);
            FirebaseCallbacks.SubscribeGoogleSignInIOSSuccess(OnGoogleSignInSuccess);
            
            // Subscribe to Apple Sign-In events
            FirebaseCallbacks.SubscribeAppleSignInAndroidSuccess(OnAppleSignInSuccess);
            FirebaseCallbacks.SubscribeAppleSignInIOSSuccess(OnAppleSignInSuccess);
        }

        private void OnDisable()
        {
            // Unsubscribe from all events
            FirebaseCallbacks.SubscribeFirebaseReady(OnFirebaseReady, false);
            FirebaseCallbacks.SubscribeCurrentUserLoaded(OnCurrentUserLoaded, false);
            FirebaseCallbacks.SubscribeLoginSuccess(OnLoginSuccess, false);
            FirebaseCallbacks.SubscribeLogout(OnLogout, false);
            
            FirebaseCallbacks.SubscribeGoogleSignInAndroidSuccess(OnGoogleSignInSuccess, false);
            FirebaseCallbacks.SubscribeGoogleSignInIOSSuccess(OnGoogleSignInSuccess, false);
            
            FirebaseCallbacks.SubscribeAppleSignInAndroidSuccess(OnAppleSignInSuccess, false);
            FirebaseCallbacks.SubscribeAppleSignInIOSSuccess(OnAppleSignInSuccess, false);
        }

        private void Start()
        {
            // Check if Firebase is already initialized
            if (FirebaseCoreService.Instance.IsInitialized)
            {
                CheckAuthenticationState();
            }
        }

        private void OnFirebaseReady()
        {
            CustomLogger.LogInfo("Firebase is ready, checking authentication state...");
            CheckAuthenticationState();
        }

        private void CheckAuthenticationState()
        {
            if (_hasCheckedInitialState)
                return;

            _hasCheckedInitialState = true;

            FirebaseUser currentUser = FirebaseCoreService.Instance.CurrentUser;
            
            if (currentUser != null)
            {
                CustomLogger.LogInfo($"User already logged in: {currentUser.Email} (UID: {currentUser.UserId})");
                ShowLoggedInState(currentUser);
            }
            else
            {
                CustomLogger.LogInfo("No user currently logged in");
                LogToUI("No user currently logged in");
            }
        }

        private void OnCurrentUserLoaded(FirebaseUser user)
        {
            if (user != null)
            {
                CustomLogger.LogInfo($"Current user loaded: {user.Email}");
                ShowLoggedInState(user);
            }
            else
            {
                LogToUI("User session ended");
            }
        }

        private void OnLoginSuccess()
        {
            FirebaseUser user = FirebaseCoreService.Instance.CurrentUser;
            if (user != null)
            {
                ShowLoggedInState(user);
            }
        }

        private void OnGoogleSignInSuccess(string idToken)
        {
            FirebaseUser user = FirebaseCoreService.Instance.CurrentUser;
            if (user != null)
            {
                ShowLoggedInState(user);
            }
        }

        private void OnAppleSignInSuccess(string idToken)
        {
            FirebaseUser user = FirebaseCoreService.Instance.CurrentUser;
            if (user != null)
            {
                ShowLoggedInState(user);
            }
        }

        private void OnLogout()
        {
            LogToUI("User logged out");
        }

        private void ShowLoggedInState(FirebaseUser user)
        {
            CustomLogger.LogInfo("Showing logged in state");
            DisplayUserInfo(user);
        }

        private void DisplayUserInfo(FirebaseUser user)
        {
            if (user == null)
                return;

            string userInfo = $"Email: {user.Email}\n" +
                            $"UID: {user.UserId}\n" +
                            $"Display Name: {user.DisplayName ?? "N/A"}\n" +
                            $"Email Verified: {user.IsEmailVerified}\n" +
                            $"Provider: {(user.ProviderId ?? "N/A")}";
            
            CustomLogger.LogInfo(userInfo);
            LogToUI(userInfo);
        }

        private void LogToUI(string message)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
            }
        }

        /// <summary>
        /// Manual method to refresh authentication state
        /// Can be called from UI buttons if needed
        /// </summary>
        public void RefreshAuthState()
        {
            _hasCheckedInitialState = false;
            CheckAuthenticationState();
        }

        /// <summary>
        /// Check if user is currently logged in
        /// </summary>
        public bool IsUserLoggedIn()
        {
            return FirebaseCoreService.Instance.CurrentUser != null;
        }

        /// <summary>
        /// Get current logged in user
        /// </summary>
        public FirebaseUser GetCurrentUser()
        {
            return FirebaseCoreService.Instance.CurrentUser;
        }
    }
}