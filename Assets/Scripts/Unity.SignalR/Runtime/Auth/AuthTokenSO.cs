// FILE: Assets/SignalRLib/SO/AuthTokenSO.cs
using UnityEngine;

namespace SignalRLib.SO
{
    [CreateAssetMenu(menuName = "SignalR/Auth Token", fileName = "AuthToken")]
    public class AuthTokenSO : ScriptableObject
    {
        [System.NonSerialized] 
        private string _token;

        public string Token => _token;
        public bool IsValid => !string.IsNullOrEmpty(_token);

        public void Set(string token)
        {
            _token = token;
            Debug.Log($"[Auth] Token set: {token?[..5]}...");
        }

        public void Clear() => _token = null;
    }
}