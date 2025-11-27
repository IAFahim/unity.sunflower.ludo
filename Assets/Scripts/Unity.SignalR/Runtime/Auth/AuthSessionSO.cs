// FILE: Assets/SignalRLib/SO/AuthSessionSO.cs
using UnityEngine;

namespace SignalRLib.SO
{
    [CreateAssetMenu(menuName = "SignalR/Auth Session", fileName = "AuthSession")]
    public class AuthSessionSO : ScriptableObject
    {
        // Volatile: Cleared when game restarts
        [System.NonSerialized] 
        private string _accessToken;

        public string AccessToken => _accessToken;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_accessToken);

        public void SetToken(string token)
        {
            _accessToken = token;
            Debug.Log($"[AuthSession] Token Updated: {token[..10]}...");
        }

        public void Clear()
        {
            _accessToken = null;
        }
    }
}