// FILE: Assets/Ludos/Client/UI/LudoLobbyUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Ludos.Core;

namespace Ludos.Client.UI
{
    public class LudoLobbyUI : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private LudoGameManager gameManager;

        [Header("Panels")]
        // [SerializeField] private GameObject menuPanel;
        // [SerializeField] private GameObject gamePanel; // The board view
        // [SerializeField] private GameObject waitingPanel; // "Waiting for opponent..."

        [Header("Menu Inputs")]
        [SerializeField] private Button createGameBtn;
        [SerializeField] private Button joinGameBtn;
        [SerializeField] private TMP_InputField roomIdInput;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Waiting Room")]
        [SerializeField] private TextMeshProUGUI roomIdDisplay;
        // [SerializeField] private Button copyRoomIdBtn;

        private void Start()
        {
            // Reset UI
            // menuPanel.SetActive(true);
            // gamePanel.SetActive(false);
            // waitingPanel.SetActive(false);
            statusText.text = "";

            // Bind Buttons
            createGameBtn.onClick.AddListener(OnCreateClicked);
            joinGameBtn.onClick.AddListener(OnJoinClicked);
            // copyRoomIdBtn.onClick.AddListener(CopyRoomIdToClipboard);

            // Bind Manager Events
        }

        private void OnDestroy()
        {
            if (gameManager)
            {
            }
        }

        // ==============================================================================
        // ACTION HANDLERS
        // ==============================================================================

        private void OnCreateClicked()
        {
            SetBusy("Creating Room...");
            // We assume Host is Seat 0
        }

        private void OnJoinClicked()
        {
            string roomCode = roomIdInput.text.Trim();

            if (string.IsNullOrEmpty(roomCode))
            {
                statusText.text = "<color=red>Please enter a Room ID</color>";
                return;
            }

            SetBusy($"Joining {roomCode}...");
            
        }

        // ==============================================================================
        // EVENT CALLBACKS
        // ==============================================================================

        private void OnRoomJoined(string roomId)
        {
            // 1. Switch to Waiting Panel
            // menuPanel.SetActive(false);
            // waitingPanel.SetActive(true);
            // gamePanel.SetActive(true); // Show board in background?

            // 2. Update Display
            roomIdDisplay.text = roomId;
            statusText.text = "";
            
            Debug.Log($"[UI] Joined Room: {roomId}");
        }

        private void OnGameStateReceived(LudoState state)
        {
            // If the game actually starts (e.g. 2 players present), hide waiting panel
            // We check the ActiveSeats or a generic "GameStarted" flag
            // For now, if we have state, we assume we are "In Game"
            
            // Optional: Hide waiting text if opponent joined
            // waitingPanel.SetActive(false); 
        }

        // ==============================================================================
        // HELPERS
        // ==============================================================================

        private void SetBusy(string msg)
        {
            createGameBtn.interactable = false;
            joinGameBtn.interactable = false;
            roomIdInput.interactable = false;
            statusText.text = msg;
        }

        private void CopyRoomIdToClipboard()
        {
            string id = roomIdDisplay.text;
            GUIUtility.systemCopyBuffer = id;
            
            // Quick feedback
            // var originalText = copyRoomIdBtn.GetComponentInChildren<TextMeshProUGUI>().text;
            // copyRoomIdBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Copied!";
            Invoke(nameof(ResetCopyText), 1.5f);
            
            // If you have DoTween:
            // copyRoomIdBtn.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }
        
        private void ResetCopyText()
        {
            // if(copyRoomIdBtn) 
            //     copyRoomIdBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Copy ID";
        }
    }
}