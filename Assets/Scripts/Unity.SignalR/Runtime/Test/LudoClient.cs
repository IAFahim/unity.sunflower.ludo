using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;
using VirtueSky.Inspector;

public class LudoClient : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameAuthClient authClient;

    [Header("Game Info")]
    [SerializeField] private string _roomId;
    [SerializeField] private int _mySeat = -1;
    [SerializeField] private int _currentTurnSeat = -1; // -1 means "Unknown/Not Started"
    
    [Header("Controls")]
    [SerializeField] private int _tokenIndex = 0;

    private HubConnection _hub;

    #region 1. Connection

    [Button]
    public async void Connect()
    {
        if (authClient == null || !authClient.IsLoggedIn) { Log("❌ Login in AuthClient first!"); return; }
        if (_hub != null) await _hub.DisposeAsync();

        string url = $"{authClient.baseUrl}/hubs/ludo";
        
        _hub = new HubConnectionBuilder()
            .WithUrl(url, o => o.AccessTokenProvider = () => Task.FromResult(authClient.AccessToken))
            .WithAutomaticReconnect()
            .Build();

        RegisterEvents();

        try {
            await _hub.StartAsync();
            Log("✅ Connected to Hub!");
        } catch (Exception ex) { Log("Conn Error: " + ex.Message); }
    }

    [Button]
    public async void DisconnectAndClear()
    {
        if (_hub != null) await _hub.DisposeAsync();
        _roomId = "";
        _mySeat = -1;
        _currentTurnSeat = -1;
        Log("🔌 Disconnected.");
    }

    #endregion

    #region 2. Events

    private void RegisterEvents()
    {
        _hub.On<string>("PlayerJoined", (uid) => RunOnMain(() => Log($"👋 Player Joined: {uid}")));

        // The critical update: Getting the board state
        _hub.On<byte[]>("GameState", (data) => RunOnMain(() => {
            if (data.Length < 28) return;
            
            _currentTurnSeat = data[16]; // Byte 16 is CurrentPlayer
            int dice = data[17];         // Byte 17 is LastDice
            
            string turnMsg = (_currentTurnSeat == _mySeat) ? "🟢 YOUR TURN!" : $"🔴 Turn: Seat {_currentTurnSeat}";
            Log($"🔄 State: {turnMsg} | Dice: {dice}");
        }));

        _hub.On<byte>("RollResult", (d) => RunOnMain(() => Log($"🎲 Rolled: {d}")));
        _hub.On<string>("Error", (m) => RunOnMain(() => Log($"🛑 Error: {m}")));
    }

    #endregion

    #region 3. Actions

    [Button]
    public async void CreateRoom()
    {
        if (!IsConnected()) return;
        try {
            // This waits for the string return from Server
            string id = await _hub.InvokeAsync<string>("CreateGame");
            
            RunOnMain(() => {
                _roomId = id;
                _mySeat = 0; // Creator is always Seat 0
                _currentTurnSeat = 0; // Game always starts at Seat 0
                Log($"✅ Room Created: {id}. You are Seat 0.");
                Log("👉 Click 'Roll Dice' to start.");
            });
        } 
        catch (Exception ex) { Log("❌ Create Failed: " + ex.Message); }
    }

    [Button]
    public async void JoinRoom()
    {
        if (!IsConnected() || string.IsNullOrEmpty(_roomId)) { Log("❌ Enter Room ID first"); return; }
        
        try {
            bool success = await _hub.InvokeAsync<bool>("JoinGame", _roomId);
            if (success) {
                // We assume Seat 1 for testing simplicity. 
                // Ideally, server should tell us our seat in the Join response.
                if (_mySeat == -1) _mySeat = 1; 
                Log($"✅ Joined {_roomId}. Assuming Seat {_mySeat}.");
            } else {
                Log("❌ Join Failed (Room Full or Not Found).");
            }
        }
        catch (Exception ex) { Log("❌ Join Error: " + ex.Message); }
    }

    [Button]
    public async void RollDice()
    {
        if (!IsConnected()) return;
        // Logic check: only warn, don't block (in case client state is desynced)
        if (_currentTurnSeat != -1 && _currentTurnSeat != _mySeat) 
            Log($"⚠️ Warning: Client thinks it is Seat {_currentTurnSeat}'s turn."); 
        
        await _hub.SendAsync("RollDice", _roomId);
    }

    [Button]
    public async void MoveToken()
    {
        if (!IsConnected()) return;
        await _hub.SendAsync("MoveToken", _roomId, _tokenIndex);
    }

    #endregion

    private bool IsConnected() => _hub?.State == HubConnectionState.Connected;
    private void Log(string m) => Debug.Log($"[Ludo] {m}");
    private void RunOnMain(Action a) => UnityMainThreadDispatcher.Instance().Enqueue(a);
    private void OnDestroy() => _hub?.DisposeAsync();
}