using UnityEngine;

namespace Ludos.Client.Config
{
    [CreateAssetMenu(menuName = "Ludos/Server Configuration", fileName = "LudoServerConfig")]
    public class LudoServerConfig : ScriptableObject
    {
        [Header("Aspire Settings")]
        [Tooltip("The port defined in GameService.ApiService launchSettings.json")]
        [SerializeField] private string ipAddress = "localhost"; 
        [SerializeField] private int port = 5525;
        [SerializeField] private bool useHttps = false;

        [Header("Endpoints")]
        [SerializeField] private string hubPath = "/hubs/game";
        [SerializeField] private string authPath = "/auth";

        // ========================================================================
        // DYNAMIC GETTERS (Handle Android Emulator vs Editor automatically)
        // ========================================================================

        public string BaseUrl
        {
            get
            {
                string host = ipAddress;

                // SPECIAL FIX: Android Emulator cannot see 'localhost'. 
                // It maps the host PC to '10.0.2.2'.
#if UNITY_ANDROID && !UNITY_EDITOR
                if (host == "localhost" || host == "127.0.0.1")
                    host = "10.0.2.2";
#endif
                string protocol = useHttps ? "https" : "http";
                return $"{protocol}://{host}:{port}";
            }
        }

        public string HubUrl => $"{BaseUrl}{hubPath}";
        
        public string RegisterUrl => $"{BaseUrl}{authPath}/register";
        public string LoginUrl => $"{BaseUrl}{authPath}/login";

        // ========================================================================
        // RESET DEFAULTS
        // ========================================================================
        private void Reset()
        {
            // These match your Aspire AppHost defaults
            ipAddress = "localhost";
            port = 5525;
            useHttps = false;
            hubPath = "/hubs/game";
            authPath = "/auth";
        }
    }
}