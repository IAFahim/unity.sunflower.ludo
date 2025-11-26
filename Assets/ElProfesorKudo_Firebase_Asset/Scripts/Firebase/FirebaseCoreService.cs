using Firebase;
using Firebase.Auth;
using System;


namespace ElProfesorKudo.Firebase.Core
{
    using ElProfesorKudo.Firebase.Event;
    using ElProfesorKudo.Firebase.Common;
    using System.Threading.Tasks;

    public class FirebaseCoreService : Singleton<FirebaseCoreService>
    {
        private FirebaseAuth _auth;
        private FirebaseUser _currentUser;
        private bool _isInitialized = false;

        public FirebaseAuth Auth { get => _auth; }
        public FirebaseUser CurrentUser { get => _currentUser; }
        public bool IsInitialized { get => _isInitialized; }

        protected override void Awake()
        {
            base.Awake();
            InitializeFirebase();
        }

        private void InitializeFirebase()
        {
            CustomLogger.LogDebug("Starting Firebase initialization...");
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    CustomLogger.LogError("Firebase init faulted: " + task.Exception);
                    return;
                }
                if (task.IsCanceled)
                {
                    CustomLogger.LogError("Firebase init canceled.");
                    return;
                }
                DependencyStatus status = task.Result;
                CustomLogger.LogInfo("Dependency check result: " + status);

                if (status == DependencyStatus.Available)
                {
                    try
                    {
                        _auth = FirebaseAuth.DefaultInstance;

                        _auth.StateChanged += OnAuthStateChanged;
                        _currentUser = _auth.CurrentUser;
                        if (_currentUser != null)
                        {
                            CustomLogger.LogDebug("User already logged in at startup: " + _currentUser.UserId);
                        }
                        _isInitialized = true;
                        CustomLogger.LogInfo("Firebase initialized.");
                        FirebaseCallbacks.InvokeFirebaseReady();
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.LogError("Exception during Firebase init: " + ex);
                    }
                }
                else
                {
                    CustomLogger.LogError("Firebase init failed: " + task.Result);
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void OnAuthStateChanged(object sender, EventArgs e)
        {
            CustomLogger.LogDebug("OnAuthStateChanged Call");
            FirebaseUser newUser = _auth.CurrentUser;
            //Case 1 : No current user and no new user
            if (_currentUser == null && newUser == null)
            {
                CustomLogger.LogInfo("No CurrentUser Log ");
                return;
            }
            //Case 2 : A user was already logged in, but the new user is the same
            if (_currentUser != null && newUser != null && _currentUser.UserId == newUser.UserId)
            {
                CustomLogger.LogInfo("CurrentUser Log but User didn't change");
                FirebaseCallbacks.InvokeCurrentUserLoaded(_currentUser);
                return;
            }
            //Case 3 : The previous user was null, but now a user is logged in
            if (_currentUser == null && newUser != null)
            {
                _currentUser = newUser;
                CustomLogger.LogInfo("New user logged in: " + _currentUser.UserId);
                FirebaseCallbacks.InvokeCurrentUserLoaded(_currentUser);
                return;
            }
            // Case 4 : New user not log , log out
            if (_currentUser != null && newUser == null)
            {
                CustomLogger.LogInfo("User logged out: " + _currentUser.UserId);
                _currentUser = null;
                return;
            }

            // Case 5 : User change
            if (_currentUser != null && newUser != null && _currentUser.UserId != newUser.UserId)
            {
                CustomLogger.LogInfo("User switched from " + _currentUser.UserId + " to " + newUser.UserId);
                _currentUser = newUser;
                FirebaseCallbacks.InvokeCurrentUserLoaded(_currentUser);
                return;
            }
            CustomLogger.LogWarning("OnAuthStateChanged reached unexpected case");

        }

        public void SetCurrentUser(FirebaseUser user)
        {
            _currentUser = user;
        }

        private void OnDestroy()
        {
            if (_auth != null)
            {
                _auth.StateChanged -= OnAuthStateChanged;
            }
        }
    }
}