using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using VirtueSky.Inspector;

public class GameAuthClient : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string baseUrl = "http://localhost:5525";

    [Header("UI References")]
    [SerializeField] private string emailInput;
    [SerializeField] private string passwordInput;
    [SerializeField] private string amountInput; // For coins
    [SerializeField] private string logText;
    
    // Internal State
    private string _authToken;
    public AuthResponse authResponse;

    // Data Classes for JSON Serialization
    [Serializable]
    public class AuthRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class AuthResponse
    {
        public string accessToken;
    }

    [Serializable]
    public class ProfileResponse
    {
        public int coins;
        // Add other fields from your /game/me endpoint here
    }

    [Serializable]
    public class TransactionRequest
    {
        public int amount;
    }

    [Serializable]
    public class TransactionResponse
    {
        public int newBalance;
    }

    #region Public Interaction Methods (Link these to Buttons)
    [Button]

    public void OnClick_HealthCheck()
    {
        StartCoroutine(SendRequest(baseUrl + "/health", "GET", null, null, (response) =>
        {
            Log("Health Check: OK");
        }));
    }

    [Button]
    public void OnClick_Register()
    {
        AuthRequest data = new AuthRequest { email = emailInput, password = passwordInput };
        string json = JsonUtility.ToJson(data);

        StartCoroutine(SendRequest(baseUrl + "/auth/register", "POST", json, null, (response) =>
        {
            Log("Registration Successful! Please Login.");
        }));
    }

    [Button]
    public void OnClick_Login()
    {
        AuthRequest data = new AuthRequest { email = emailInput, password = passwordInput };
        string json = JsonUtility.ToJson(data);

        StartCoroutine(SendRequest(baseUrl + "/auth/login", "POST", json, null, (response) =>
        {
            authResponse = JsonUtility.FromJson<AuthResponse>(response);
            _authToken = authResponse.accessToken;
            Log($"🔐 Login Successful! Token stored.");
            
            // Auto-fetch profile after login
            OnClick_GetProfile();
        }));
    }

    [Button]
    public void OnClick_GetProfile()
    {
        if (string.IsNullOrEmpty(_authToken)) { Log("Error: Not logged in."); return; }

        StartCoroutine(SendRequest(baseUrl + "/game/me", "GET", null, _authToken, (response) =>
        {
            ProfileResponse profile = JsonUtility.FromJson<ProfileResponse>(response);
            Log($"👤 Profile Loaded. Coins: {profile.coins}");
        }));
    }

    [Button]
    public void OnClick_EarnCoins()
    {
        if (string.IsNullOrEmpty(_authToken)) { Log("Error: Not logged in."); return; }

        // Default to 50 if empty, otherwise parse input
        int amount = string.IsNullOrEmpty(amountInput) ? 50 : int.Parse(amountInput);

        TransactionRequest data = new TransactionRequest { amount = amount };
        string json = JsonUtility.ToJson(data);

        StartCoroutine(SendRequest(baseUrl + "/game/coins/transaction", "POST", json, _authToken, (response) =>
        {
            TransactionResponse trans = JsonUtility.FromJson<TransactionResponse>(response);
            Log($"💰 Transaction Complete. New Balance: {trans.newBalance}");
        }));
    }

    #endregion

    #region Network Logic

    private IEnumerator SendRequest(string url, string method, string jsonData, string token, Action<string> onSuccess)
    {
        using (UnityWebRequest request = new UnityWebRequest(url, method))
        {
            // 1. Attach Body (if POST/PUT)
            if (!string.IsNullOrEmpty(jsonData))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            // 2. Set Headers
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(token))
            {
                request.SetRequestHeader("Authorization", "Bearer " + token);
            }

            // 3. Send
            Log($"Sending {method} to {url}...");
            yield return request.SendWebRequest();

            // 4. Handle Result
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Log($"<color=red>Error: {request.error}</color> \nResponse: {request.downloadHandler.text}");
            }
            else
            {
                // Success
                onSuccess?.Invoke(request.downloadHandler.text);
            }
        }
    }

    private void Log(string message)
    {
        Debug.Log(message);
        if (logText != null)
        {
            logText = message + "\n" + logText;
            // Keep log short in UI
            if (logText.Length > 1000) logText = logText.Substring(0, 1000); 
        }
    }

    #endregion
}