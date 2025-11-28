using System;
using System.Text;
using Cysharp.Threading.Tasks;
using Ludos.Client.Auth;
using SignalRLib.Architecture;
using SignalRLib.SO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Ludos.Client.Auth
{
    public class GuestLoginManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private string baseUrl = "http://localhost:5525";
        
        [Header("Dependencies")]
        [SerializeField] private AuthSessionSO authSession;
        [SerializeField] private SignalRHubServiceSO hubService;

        [Header("UI")]
        [SerializeField] private Button quickPlayBtn;
        [SerializeField] private TMP_Text statusText;

        private const string PREF_EMAIL = "Guest_Email";
        private const string PREF_PASS = "Guest_Pass";

        private void Start()
        {
            quickPlayBtn.onClick.AddListener(() => LoginProcess().Forget());
        }

        private async UniTaskVoid LoginProcess()
        {
            quickPlayBtn.interactable = false;
            SetStatus("Checking Identity...");

            string email = PlayerPrefs.GetString(PREF_EMAIL, "");
            string password = PlayerPrefs.GetString(PREF_PASS, "");
            bool isExistingAccount = !string.IsNullOrEmpty(email);

            // If no account exists, generate one now
            if (!isExistingAccount)
            {
                (email, password) = GenerateGuestCredentials();
            }

            try
            {
                // 1. Register (Only if we just generated new credentials)
                if (!isExistingAccount)
                {
                    SetStatus("Creating Account...");
                    bool registered = await PostRequest("/auth/register", email, password);
                    if (!registered)
                    {
                        SetStatus("Registration Failed.");
                        quickPlayBtn.interactable = true;
                        return;
                    }
                    
                    // Save ONLY after successful registration
                    SaveCredentials(email, password);
                }

                // 2. Login
                SetStatus("Logging in...");
                var token = await PostLogin("/auth/login", email, password);

                if (!string.IsNullOrEmpty(token))
                {
                    SetStatus("Success! Connecting...");
                    authSession.SetToken(token);
                    await hubService.ConnectAsync();
                    gameObject.SetActive(false); 
                }
            }
            catch (UnityWebRequestException ex) when (ex.ResponseCode == 401)
            {
                // HANDLE STALE ACCOUNTS (Server DB wiped but Client has Prefs)
                Debug.LogWarning("[Auth] Stale Credentials detected (401). Clearing and Retrying...");
                
                ClearCredentials();
                
                if (isExistingAccount)
                {
                    // Recursively try again as a NEW user
                    LoginProcess().Forget();
                }
                else
                {
                    SetStatus("Login Failed (401)");
                    quickPlayBtn.interactable = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Auth] Error: {ex.Message}");
                SetStatus("Network Error");
                quickPlayBtn.interactable = true;
            }
        }

        private async UniTask<bool> PostRequest(string endpoint, string email, string password)
        {
            var body = new AuthRequest { email = email, password = password };
            var json = JsonUtility.ToJson(body);
            
            using var req = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            await req.SendWebRequest();
            return req.result == UnityWebRequest.Result.Success;
        }

        private async UniTask<string> PostLogin(string endpoint, string email, string password)
        {
            var body = new AuthRequest { email = email, password = password };
            var json = JsonUtility.ToJson(body);

            using var req = new UnityWebRequest($"{baseUrl}{endpoint}", "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            // This throws UnityWebRequestException on 401/500, caught in LoginProcess
            await req.SendWebRequest(); 

            if (req.result == UnityWebRequest.Result.Success)
            {
                var res = JsonUtility.FromJson<TokenResponse>(req.downloadHandler.text);
                return res.accessToken;
            }
            return null;
        }

        // --- Helpers ---

        private (string, string) GenerateGuestCredentials()
        {
            string guid = Guid.NewGuid().ToString("N")[..8];
            string e = $"guest_{guid}@ludo.game";
            string p = $"Guest_{guid}!123";
            return (e, p);
        }

        private void SaveCredentials(string e, string p)
        {
            PlayerPrefs.SetString(PREF_EMAIL, e);
            PlayerPrefs.SetString(PREF_PASS, p);
            PlayerPrefs.Save();
        }

        private void ClearCredentials()
        {
            PlayerPrefs.DeleteKey(PREF_EMAIL);
            PlayerPrefs.DeleteKey(PREF_PASS);
            PlayerPrefs.Save();
        }

        private void SetStatus(string msg)
        {
            if (statusText) statusText.text = msg;
            Debug.Log($"[Auth] {msg}");
        }
    }
}