using System;
using Ludos.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SignalRLib.SO;

public class LudoGameManager : MonoBehaviour
{
    [Header("Network")]
    [SerializeField] private LudoHubClient client; // Drag the script here
    
    // Runtime State
    public LudoState CurrentState;
    public bool IsMyTurn { get; private set; }
    public int MySeatIndex { get; private set; } = -1;
    private string _currentRoomId;

    public AuthSessionSO authSessionSo;
    
    private void OnEnable()
    {
        client.OnRoomCreated += HandleRoomCreated;
        client.OnGameStateReceived += HandleStateUpdate;
        client.OnDiceRolled += OnClientOnOnDiceRolled;
    }

    private void Start()
    {
        if (authSessionSo.IsLoggedIn)
        {
            client.ConnectAsync().Forget();
        }
    }

    private void OnClientOnOnDiceRolled(int dice)
    {
        Debug.Log($"Dice: {dice}");
    }

    private void OnDisable()
    {
        client.OnRoomCreated -= HandleRoomCreated;
        client.OnGameStateReceived -= HandleStateUpdate;
        client.OnDiceRolled -= OnClientOnOnDiceRolled;

    }

    // --- UI Commands ---

    public void UI_CreateGame()
    {
        MySeatIndex = 0; // Host
        client.CreateGame();
    }

    public void UI_JoinGame(string roomId)
    {
        MySeatIndex = 2; // Joiner (Simple Logic)
        client.JoinGame(roomId);
    }

    public void UI_RollDice()
    {
        if (!IsMyTurn) return;
        client.RollDice(_currentRoomId);
    }

    public void UI_MoveToken(int tokenIndex)
    {
        if (!IsMyTurn) return;
        client.MoveToken(_currentRoomId, tokenIndex);
    }

    // --- Handlers ---

    private void HandleRoomCreated(string roomId)
    {
        _currentRoomId = roomId;
        Debug.Log($"Room Created: {roomId}");
    }

    private unsafe void HandleStateUpdate(byte[] data)
    {
        if (data.Length != 28) return;

        fixed (byte* ptr = data)
        {
            CurrentState = *(LudoState*)ptr;
        }

        IsMyTurn = (CurrentState.CurrentPlayer == MySeatIndex);
        
        // Notify Visuals...
        Debug.Log($"State Sync. Turn: {CurrentState.CurrentPlayer}");
    }
}