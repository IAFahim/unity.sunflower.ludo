// FILE: Assets/SignalRLib/SO/ServerConfigSO.cs
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
namespace SignalRLib.SO
{
    [CreateAssetMenu(menuName = "SignalR/Server Config", fileName = "ServerConfig")]
    public class ServerConfigSO : ScriptableObject
    {
        [Header("Identity Source")]
        [Tooltip("The shared session that holds the JWT")]
        [SerializeField] private AuthSessionSO authSession;

        [Header("Connection Settings")]
        [SerializeField] private string baseUrl = "http://localhost";
        [SerializeField] private int port = 5525;
        
        [Tooltip("Specific path for this hub (e.g. /hubs/game or /hubs/chat)")]
        [SerializeField] private string hubPath = "/hubs/game";
        
        public bool autoReconnect = true;
        public int[] reconnectDelaysMs = { 0, 2000, 10000 };

        // Runtime State
        public string ActiveUrl { get; private set; }
        
        // The Manager reads this property. We proxy it to the AuthSession.
        public string AccessToken => authSession != null ? authSession.AccessToken : string.Empty;

        public async UniTask InitializeAsync()
        {
            // Simple logic: Combine URL
            // (You can add the remote fetch logic here from previous answers if needed)
            ActiveUrl = $"{baseUrl}:{port}{hubPath}";
            
            await UniTask.CompletedTask;
        }
    }
}