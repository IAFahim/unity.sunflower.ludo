using UnityEngine;

namespace ElProfesorKudo.Firebase.AppleSignIn
{
    using ElProfesorKudo.Firebase.Common;
    public abstract class FirebaseAbstractAppleSignIn : MonoBehaviour
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
        protected virtual void OnAppleSignInSuccess(string idToken)
        {
            CustomLogger.LogInfo("Apple Sign-In success");
        }

        /// <summary>
        /// Call From swift or java code with unity send message
        /// </summary>
        /// <param name="error"></param>
        protected virtual void OnAppleSignInFailed(string error)
        {
            CustomLogger.LogError("Apple Sign-In failed: " + error);
        }

        public virtual void SignOut()
        {
            CustomLogger.LogInfo("Attempting to disconnect...");
        }

        /// <summary>
        /// Call From swift or java code with unity send message
        /// </summary>
        protected virtual void OnAppleSignOutSuccess()
        {
            CustomLogger.LogInfo("Apple Sign-Out successful.");
        }

        /// <summary>
        /// Call From swift or java code with unity send message
        /// </summary>
        /// <param name="errorMessage"></param>
        protected virtual void OnAppleSignOutFailed(string errorMessage)
        {
            CustomLogger.LogError("Apple Sign-Out failed from Java plugin: " + errorMessage);
        }
    }
}
