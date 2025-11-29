using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using VirtueSky.Inspector;

public class LudoClient : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameAuthClient authClient;

    [Header("Game Inputs")]
    [SerializeField] private string joinRoomIdInput; // Type the Room ID here to join
    [SerializeField] private int tokenIndexToMove;   // 0-3 (Which piece to move)

    [Header("UI Output")]
    [TextArea(5, 10)] 
    [SerializeField] private string statusText;

    // State
    private HubConnection _hub;
    private string _currentRoomId;

    #region Connection Setup

    [Button]
    public async void OnClick_ConnectToGameServer()
    {
        if (authClient == null) { Log("❌ Error: GameAuthClient reference missing!"); return; }
        if (!authClient.IsLoggedIn) { Log("❌ Error: You must Login in GameAuthClient first!"); return; }

        await ConnectToHub();
    }

    private async Task ConnectToHub()
    {
        if (_hub != null) await _hub.DisposeAsync();

        // 1. Construct URL (Note: /hubs/ludo based on your LudoModule configuration)
        string hubUrl = $"{authClient.baseUrl}/hubs/ludo";
        string token = authClient.AccessToken;

        _hub = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        // 2. Register Event Handlers
        RegisterHandlers();

        // 3. Start Connection
        try
        {
            await _hub.StartAsync();
            RunOnMainThread(() => Log("✅ Connected to Ludo Hub!"));
        }
        catch (Exception ex)
        {
            RunOnMainThread(() => Log($"❌ Connection Error: {ex.Message}"));
        }
    }

    private void RegisterHandlers()
    {
        // Called when you create a room
        _hub.On<string>("RoomCreated", (roomId) =>
        {
            _currentRoomId = roomId;
            RunOnMainThread(() => 
            {
                joinRoomIdInput = roomId; // Auto-fill for convenience
                Log($"🏠 Room Created: {roomId} (Waiting for players...)");
            });
        });

        // Called when anyone joins (including you)
        _hub.On<string>("PlayerJoined", (userId) =>
        {
            RunOnMainThread(() => Log($"👋 Player Joined: {userId}"));
        });

        // Called when you join successfully or a move/roll happens
        _hub.On<byte[]>("GameState", (stateBytes) =>
        {
            // The server sends a raw struct byte array. 
            // For now, we just acknowledge receipt. To parse this, you'd need the Struct layout.
            RunOnMainThread(() => Log($"📦 Game State Received ({stateBytes.Length} bytes)"));
        });

        _hub.On<byte>("RollResult", (dice) =>
        {
            RunOnMainThread(() => Log($"🎲 Dice Rolled: {dice}"));
        });
        
        _hub.On<string>("GameWon", (winnerId) =>
        {
            RunOnMainThread(() => Log($"🏆 GAME OVER! Winner: {winnerId}"));
        });

        _hub.On<string>("Error", (msg) =>
        {
            RunOnMainThread(() => Log($"<color=red>Server Error: {msg}</color>"));
        });
    }

    #endregion

    #region Game Actions

    [Button]
    public async void OnClick_CreateRoom()
    {
        if (!CheckConnection()) return;
        Log("Creating Room...");
        await _hub.SendAsync("CreateGame");
    }

    [Button]
    public async void OnClick_JoinRoom()
    {
        if (!CheckConnection()) return;
        if (string.IsNullOrEmpty(joinRoomIdInput)) { Log("❌ Enter a Room ID first!"); return; }

        Log($"Joining Room {joinRoomIdInput}...");

        // We use InvokeAsync<bool> to get the return value immediately
        try 
        {
            bool success = await _hub.InvokeAsync<bool>("JoinGame", joinRoomIdInput);
            
            if (success)
            {
                _currentRoomId = joinRoomIdInput;
                Log($"✅ Successfully joined room: {_currentRoomId}");
            }
            else
            {
                Log("❌ Failed to join room (Full or doesn't exist).");
            }
        }
        catch(Exception ex)
        {
            Log($"❌ Join Error: {ex.Message}");
        }
    }

    [Button]
    public async void OnClick_RollDice()
    {
        if (!CheckConnection()) return;
        if (string.IsNullOrEmpty(_currentRoomId)) { Log("❌ No active room."); return; }
        
        await _hub.SendAsync("RollDice", _currentRoomId);
    }

    [Button]
    public async void OnClick_MoveToken()
    {
        if (!CheckConnection()) return;
        if (string.IsNullOrEmpty(_currentRoomId)) { Log("❌ No active room."); return; }

        Log($"Moving Token {tokenIndexToMove}...");
        await _hub.SendAsync("MoveToken", _currentRoomId, tokenIndexToMove);
    }

    #endregion

    #region Helpers

    private bool CheckConnection()
    {
        if (_hub?.State != HubConnectionState.Connected)
        {
            Log("⚠️ Not connected. Click 'Connect To Game Server' first.");
            return false;
        }
        return true;
    }

    private void Log(string msg)
    {
        Debug.Log($"[Ludo] {msg}");
        statusText = msg + "\n" + statusText;
        if (statusText.Length > 2000) statusText = statusText.Substring(0, 2000);
    }

    private void RunOnMainThread(Action action)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(action);
    }
    
    private void OnDestroy() { _hub?.DisposeAsync(); }

    #endregion
}