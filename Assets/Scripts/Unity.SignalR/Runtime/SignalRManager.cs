// FILE: Assets/Scripts/Gameplay/GameManager.cs

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SignalRLib.Architecture;
using Shared.DTOs;
using SignalRLib.Implementations;
using SignalRLib.SO;
using VirtueSky.Inspector;

public class SingalRManager : MonoBehaviour
{
    [Header("Network Services")]
    [SerializeField] private SignalRHubServiceSO gameHub;
    [SerializeField] private AuthSessionSO authSession;

    [Header("Incoming Events")]
    [SerializeField] private PlayerUpdatedEvent onPlayerUpdated;
    
    public string token;

    private void OnEnable()
    {
        // 1. Listen to events (Decoupled!)
        onPlayerUpdated.AddListener(HandlePlayerUpdate);

        // 2. Connect (Usually done in a Login screen, but works here too)
        // Note: In real app, you'd set the token first
    }
    
    private void Start()
    {
        authSession.SetToken(token); 
        gameHub.ConnectAsync().Forget();
    }

    private void OnDisable()
    {
        onPlayerUpdated.RemoveListener(HandlePlayerUpdate);
    }

    private void HandlePlayerUpdate(PlayerUpdatedMessage msg)
    {
        Debug.Log($"Player {msg.Username} has {msg.NewCoins} coins!");
        // Update UI...
    }

    [Button]
    public void DoTransaction(int amount)
    {
        // Sending is just one line
        // Server expects: ProcessTransaction(string userId, long amount)
        // But your token usually handles the UserId on the server side context.
        // Assuming server method: "UpdateCoins", arg: amount
        gameHub.Send("UpdateCoins", amount);
    }

    private void OnDestroy()
    {
        onPlayerUpdated.RemoveListener(HandlePlayerUpdate);
    }
}