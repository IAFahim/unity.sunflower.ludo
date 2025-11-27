using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace SignalRLib.SO
{
    [CreateAssetMenu(menuName = "SignalR/Server Config", fileName = "ServerConfig")]
    public class ServerConfigSO : ScriptableObject
    {
        [Header("Local / Fallback Config")]
        [SerializeField] private string baseUrl = "http://localhost";
        [SerializeField] private int port = 5525;
        [SerializeField] private string hubPath = "/hubs/game";
        
        [Header("Remote Configuration")]
        [SerializeField] private bool useRemoteConfig = false;
        [SerializeField] private string remoteConfigUrl = "https://api.mygame.com/v1/server-info";

        [Header("Settings")] 
        public bool autoReconnect = true;
        public int[] reconnectDelaysMs = { 0, 2000, 10000, 30000 };

        // Runtime State
        public string ActiveUrl { get; private set; }
        public string AccessToken { get; private set; }

        public void SetAccessToken(string token) => AccessToken = token;

        /// <summary>
        /// Prepares the URL. If Remote Config is enabled, fetches it first.
        /// </summary>
        public async UniTask InitializeAsync()
        {
            if (useRemoteConfig)
            {
                await FetchRemoteConfig();
            }
            else
            {
                ActiveUrl = $"{baseUrl}:{port}{hubPath}";
            }
            
            Debug.Log($"[SignalR Config] Initialized targeting: {ActiveUrl}");
        }

        private async UniTask FetchRemoteConfig()
        {
            // Smart solution: Fetch config from an API endpoint using UnityWebRequest + UniTask
            using var uwr = UnityWebRequest.Get(remoteConfigUrl);
            await uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SignalR Config] Failed to fetch remote config: {uwr.error}. Using fallback.");
                ActiveUrl = $"{baseUrl}:{port}{hubPath}";
                return;
            }

            // Example JSON: { "url": "http://192.168.1.50", "port": 8080 }
            // Using minimal JSON parsing to stay AOT friendly
            var json = uwr.downloadHandler.text;
            var data = JsonUtility.FromJson<RemoteServerData>(json);
            
            ActiveUrl = $"{data.url}:{data.port}{hubPath}";
        }

        [Serializable]
        private class RemoteServerData
        {
            public string url;
            public int port;
        }
    }
}