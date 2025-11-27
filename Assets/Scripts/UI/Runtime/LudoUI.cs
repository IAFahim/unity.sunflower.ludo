// FILE: Assets/Ludos/Client/UI/LudoUI.cs

using Ludos.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Ludos.Client.UI
{
    public class LudoUI : MonoBehaviour
    {
        [SerializeField] private LudoGameManager manager;
        
        [Header("Room Setup")]
        [SerializeField] private TMP_InputField roomIdInput;
        [SerializeField] private Button joinBtn;
        [SerializeField] private Button createBtn;

        [Header("Game HUD")]
        [SerializeField] private Button rollDiceBtn;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI diceValueText;

        private string _currentRoomId;

        private void Start()
        {
            manager.StateUpdated += UpdateHUD;
            manager.DiceRolled += val => diceValueText.text = val.ToString();

            createBtn.onClick.AddListener(() => 
            {
                manager.SendCreateGame();
                // For demo, assume we auto join "room_1"
                _currentRoomId = "room_1"; 
                manager.SetMySeat(0); // Host is Red
                manager.SendJoinGame(_currentRoomId);
            });

            joinBtn.onClick.AddListener(() => 
            {
                _currentRoomId = roomIdInput.text;
                manager.SetMySeat(2); // Guest is Yellow
                manager.SendJoinGame(_currentRoomId);
            });

            rollDiceBtn.onClick.AddListener(() => 
            {
                manager.SendRollDice(_currentRoomId);
            });
        }

        private void UpdateHUD(LudoState state)
        {
            // Enable Roll button only if:
            // 1. It is my turn
            // 2. I haven't rolled yet (LastDiceRoll == 0)
            bool canRoll = manager.IsMyTurn && state.LastDiceRoll == 0;
            
            rollDiceBtn.interactable = canRoll;
            
            string pName = state.CurrentPlayer switch { 0=>"Red", 2=>"Yellow", _=>"Other" };
            statusText.text = manager.IsMyTurn ? "YOUR TURN" : $"{pName}'s Turn";
            
            if (state.LastDiceRoll > 0)
                diceValueText.text = state.LastDiceRoll.ToString();
        }
    }
}