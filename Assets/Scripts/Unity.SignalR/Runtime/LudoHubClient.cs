using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SignalRLib.Core;
using SignalRLib.SO;
using Cysharp.Threading.Tasks;

public class LudoHubClient : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string serverUrl = "http://localhost:5525/hubs/game";
    [SerializeField] private AuthSessionSO authSession;

    private HubConnection _connection;
    public bool IsConnected => _connection?.State == HubConnectionState.Connected;

    // --- Strongly Typed Events (No more ScriptableObjects needed) ---
    public event Action<string> OnRoomCreated;
    public event Action<string> OnPlayerJoined;
    public event Action<byte[]> OnGameStateReceived; // Raw State
    public event Action<int> OnDiceRolled;
    public event Action<string> OnError;

    // --- Lifecycle ---

    public async UniTask ConnectAsync()
    {
        if (IsConnected) return;

        // 1. Handle URL for Android/Device (Localhost fix)
        string url = serverUrl;
#if !UNITY_EDITOR && UNITY_ANDROID
        url = url.Replace("localhost", "10.0.2.2"); 
#endif

        // 2. Build Connection with AOT JSON
        _connection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(authSession?.AccessToken);
                // WebSockets is best for Unity to avoid HTTP Long Polling issues
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets; 
                options.SkipNegotiation = true;
            })
            .AddJsonProtocol(options =>
            {
                // CRITICAL: Bind the Source Generator Context
                options.PayloadSerializerOptions.TypeInfoResolver = ClientJsonContext.Default;
            })
            .WithAutomaticReconnect()
            .Build();

        // 3. Register Listeners (Bind BEFORE starting)
        BindEvents();

        try 
        {
            Debug.Log($"[LudoHub] Connecting to {url}...", this);
            await _connection.StartAsync();
            Debug.Log("<color=green>[LudoHub] Connected!</color>");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LudoHub] Connection Failed: {ex.Message}", this);
            OnError?.Invoke(ex.Message);
        }
    }

    public async UniTask DisconnectAsync()
    {
        if (_connection != null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }

    private void BindEvents()
    {
        // "RoomCreated" -> Matches Server SendAsync("RoomCreated", roomId)
        _connection.On<string>("RoomCreated", (roomId) => 
        {
            // UniTask handles Main Thread dispatch automatically if awaited, 
            // but for callbacks we use Post to be safe.
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnRoomCreated?.Invoke(roomId));
        });

        _connection.On<string>("PlayerJoined", (userId) => 
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnPlayerJoined?.Invoke(userId)));

        _connection.On<byte[]>("GameState", (data) => 
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnGameStateReceived?.Invoke(data)));

        _connection.On<int>("RollResult", (dice) => 
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnDiceRolled?.Invoke(dice)));
        
        _connection.On<string>("Error", (msg) => 
            UnityMainThreadDispatcher.Instance().Enqueue(() => OnError?.Invoke(msg)));
    }

    // --- Outgoing Commands ---

    [ContextMenu("CreateGame")]
    public void CreateGame() => Send("CreateGame");
    public void JoinGame(string roomId) => Send("JoinGame", roomId);
    public void RollDice(string roomId) => Send("RollDice", roomId);
    public void MoveToken(string roomId, int tokenIndex) => Send("MoveToken", roomId, tokenIndex);

    private void Send(string method, params object[] args)
    {
        if (!IsConnected)
        {
            Debug.LogWarning($"[LudoHub] Cannot send {method}: Not connected.");
            return;
        }

        _connection.SendCoreAsync(method, args);
    }

    private void OnDestroy() => DisconnectAsync().Forget();
}