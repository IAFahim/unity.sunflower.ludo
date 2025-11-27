// FILE: Assets/Ludos/Client/LudoGameManager.cs
using System;
using Ludos.Core;
using UnityEngine;
using SignalRLib.Architecture;
using SignalRLib.SO;
using Unity.SignalR.Runtime;
using VirtueSky.Events;

namespace Ludos.Client
{
    public class LudoGameManager : MonoBehaviour
    {
        [Header("Network Services")]
        [SerializeField] private SignalRHubServiceSO ludoHub;
        [SerializeField] private AuthSessionSO authSession;

        [Header("Listening Channels")]
        [SerializeField] private ByteArrayEvent onGameStateReceived; // Drag LudoGameStateChannel event here
        [SerializeField] private IntegerEvent onRollResultReceived;   // Drag LudoIntChannel event here
        [SerializeField] private StringEvent onPlayerJoined;

        [Header("Runtime State")]
        // We expose the raw struct. Renderers read this.
        public LudoState CurrentState; 
        public bool IsMyTurn { get; private set; }
        public int MySeatIndex { get; private set; } = -1; // 0=Red, 1=Green...

        // Events for UI/Visuals to subscribe to
        public event Action<LudoState> StateUpdated;
        public event Action<int> DiceRolled;

        private void OnEnable()
        {
            onGameStateReceived.AddListener(HandleStateUpdate);
            onRollResultReceived.AddListener(HandleRollResult);
        }

        private void OnDisable()
        {
            onGameStateReceived.RemoveListener(HandleStateUpdate);
            onRollResultReceived.RemoveListener(HandleRollResult);
        }

        // =================================================================================
        // 1. INCOMING DATA HANDLERS
        // =================================================================================

        private unsafe void HandleStateUpdate(byte[] rawData)
        {
            if (rawData == null || rawData.Length != 28)
            {
                Debug.LogError($"[Ludo] Invalid state packet size: {rawData?.Length}");
                return;
            }

            // Efficient Byte[] -> Struct Deserialization
            fixed (byte* ptr = rawData)
            {
                CurrentState = *(LudoState*)ptr;
            }

            // Determine if it's my turn
            // Note: We need to know which Seat (0-3) belongs to our UserId.
            // In a real app, the "JoinGame" response would tell us our Seat Index.
            // For now, let's assume the Server told us via a separate RPC or we infer it.
            // Let's assume we stored MySeatIndex when we joined.
            
            IsMyTurn = (CurrentState.CurrentPlayer == MySeatIndex);

            Debug.Log($"[Ludo] State Updated. Turn: P{CurrentState.CurrentPlayer}. MyTurn: {IsMyTurn}");
            StateUpdated?.Invoke(CurrentState);
        }

        private void HandleRollResult(int value)
        {
            Debug.Log($"[Ludo] Dice Rolled: {value}");
            DiceRolled?.Invoke(value);
        }

        // =================================================================================
        // 2. OUTGOING COMMANDS (Called by UI)
        // =================================================================================

        public void SendCreateGame()
        {
            ludoHub.Send("CreateGame", null); // Server returns RoomID via return value, tough with void Send. 
            // Better: Hub sends "RoomCreated" event back.
        }

        public void SendJoinGame(string roomId)
        {
            // Simple hash for testing to determine seat. 
            // Real logic: Server response tells you your seat.
            // For this demo, if I created it, I'm 0. If I joined, I'm 2.
            MySeatIndex = 0; // Force P0 for testing host
            ludoHub.Send("JoinGame", roomId);
        }

        public void SendRollDice(string roomId)
        {
            if (!IsMyTurn) return;
            ludoHub.Send("RollDice", roomId);
        }

        public void SendMoveToken(string roomId, int tokenIndex)
        {
            if (!IsMyTurn) return;
            ludoHub.Send("MoveToken", roomId, tokenIndex);
        }

        // Helper to set seat (Call this from a UI input field or Join response)
        public void SetMySeat(int seat) => MySeatIndex = seat;
    }
}