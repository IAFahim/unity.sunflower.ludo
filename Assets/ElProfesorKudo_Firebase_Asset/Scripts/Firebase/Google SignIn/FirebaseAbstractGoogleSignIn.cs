using UnityEngine;

namespace ElProfesorKudo.Firebase.GoogleSignIn
{
    using ElProfesorKudo.Firebase.Common;
    public abstract class FirebaseAbstractGoogleSignIn : MonoBehaviour
    {
        protected virtual void Awake()
        {
        }

        protected virtual void Start()
        {
        }

        public virtual void SignIn()
        {
            CustomLogger.LogInfo("SignIn() Function call");
        }

        /// <summary>
        /// Call From swift or java code with unity send message
        /// </summary>
        /// <param name="idToken"></param>
        protected virtual void OnGoogleSignInSuccess(string idToken)
        {
            CustomLogger.LogInfo("Google Sign-In success");
        }

        /// <summary>
        /// Call From swift or java code with unity send message
        /// </summary>
        /// <param name="error"></param>
        protected virtual void OnGoogleSignInFailed(string error)
        {
            CustomLogger.LogError("Google Sign-In failed: " + error);
        }

        public virtual void SignOut()
        {
            CustomLogger.LogInfo("Attempting to disconnect...");
        }

        /// <summary>
        /// Call From swift or java code with unity send message
        /// </summary>
        protected virtual void OnGoogleSignOutSuccess()
        {
            CustomLogger.LogInfo("Google Sign-Out successful.");
        }

        /// <summary>
        /// Call From swift or java code with unity send message
        /// </summary>
        /// <param name="errorMessage"></param>
        protected virtual void OnGoogleSignOutFailed(string errorMessage)
        {
            CustomLogger.LogError("Google Sign-Out failed from Java plugin: " + errorMessage);
        }
    }
}
