using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using VirtueSky.Inspector;

public class GameAuthClient : MonoBehaviour
{
    [Header("Configuration")]
    public string baseUrl = "http://localhost:5525"; // Made public for LudoClient to see

    [Header("UI References")]
    [SerializeField] private string emailInput;
    [SerializeField] private string passwordInput;
    [SerializeField] private string amountInput;
    [SerializeField] private string logText;
    
    // Public Property for other scripts to access
    public string AccessToken { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

    // Data Classes
    [Serializable] public class AuthRequest { public string email; public string password; }
    [Serializable] public class AuthResponse { public string accessToken; }
    [Serializable] public class ProfileResponse { public int coins; }
    [Serializable] public class TransactionRequest { public int amount; }
    [Serializable] public class TransactionResponse { public int newBalance; }

    [Button]
    public void OnClick_HealthCheck()
    {
        StartCoroutine(SendRequest("/health", "GET", null, null, (response) => Log("Health Check: OK")));
    }

    [Button]
    public void OnClick_Register()
    {
        AuthRequest data = new AuthRequest { email = emailInput, password = passwordInput };
        StartCoroutine(SendRequest("/auth/register", "POST", JsonUtility.ToJson(data), null, (r) => Log("Registered! Login now.")));
    }

    [Button]
    public void OnClick_Login()
    {
        AuthRequest data = new AuthRequest { email = emailInput, password = passwordInput };
        StartCoroutine(SendRequest("/auth/login", "POST", JsonUtility.ToJson(data), null, (response) =>
        {
            var authResponse = JsonUtility.FromJson<AuthResponse>(response);
            AccessToken = authResponse.accessToken;
            Log($"🔐 Login Successful! Token stored.");
            
            // Optional: Auto-fetch profile
            OnClick_GetProfile();
        }));
    }

    [Button]
    public void OnClick_GetProfile()
    {
        if (!IsLoggedIn) { Log("Error: Not logged in."); return; }
        StartCoroutine(SendRequest("/game/me", "GET", null, AccessToken, (r) =>
        {
            var profile = JsonUtility.FromJson<ProfileResponse>(r);
            Log($"👤 Coins: {profile.coins}");
        }));
    }

    [Button]
    public void OnClick_EarnCoins()
    {
        if (!IsLoggedIn) { Log("Error: Not logged in."); return; }
        int amount = string.IsNullOrEmpty(amountInput) ? 50 : int.Parse(amountInput);
        var data = new TransactionRequest { amount = amount };
        
        StartCoroutine(SendRequest("/game/coins/transaction", "POST", JsonUtility.ToJson(data), AccessToken, (r) =>
        {
            var trans = JsonUtility.FromJson<TransactionResponse>(r);
            Log($"💰 New Balance: {trans.newBalance}");
        }));
    }

    // Helper to handle full URL or relative path
    private IEnumerator SendRequest(string endpoint, string method, string jsonData, string token, Action<string> onSuccess)
    {
        string url = endpoint.StartsWith("http") ? endpoint : baseUrl + endpoint;
        
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            if (!string.IsNullOrEmpty(token)) request.SetRequestHeader("Authorization", "Bearer " + token);

            Log($"Sending {method}...");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) onSuccess?.Invoke(request.downloadHandler.text);
            else Log($"<color=red>Error: {request.error}</color>\n{request.downloadHandler.text}");
        }
    }

    private void Log(string message)
    {
        Debug.Log($"[Auth] {message}");
        statusText = message + "\n" + statusText;
        if (statusText.Length > 1000) statusText = statusText.Substring(0, 1000);
    }
    
    // UI binding for log
    private string statusText;
    private void OnGUI() { if(!string.IsNullOrEmpty(statusText)) GUILayout.Label(statusText); } 
}