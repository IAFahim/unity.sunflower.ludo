using UnityEngine;
using SignalRLib.Architecture; // For the Hub Service
using VirtueSky.Events;        // For the Event Listener

public class PingPongTester : MonoBehaviour
{
    [Header("Network Dependencies")]
    [SerializeField] private SignalRHubServiceSO gameHub; // Reference the Hub Asset
    
    [Header("Events")]
    [SerializeField] private StringEvent onPongReceived; // Reference Event_PongReceived

    private void OnEnable()
    {
        // 1. Listen for the response
        onPongReceived.AddListener(HandlePong);
    }

    private void OnDisable()
    {
        onPongReceived.RemoveListener(HandlePong);
    }

    // ========================================================================
    // 1. PING (Sending)
    // ========================================================================
    [ContextMenu("Send Ping")] // Adds right-click menu in Inspector
    public void SendPing()
    {
        if (!gameHub.IsConnected)
        {
            Debug.LogWarning("Cannot Ping: Hub not connected!");
            return;
        }

        string msg = $"Ping-{Time.frameCount}";
        Debug.Log($"[Client] Sending: {msg}");

        // "Ping" matches the C# Method Name in GameHub.cs on the server
        gameHub.Send("Ping", msg);
    }

    // ========================================================================
    // 2. PONG (Receiving)
    // ========================================================================
    private void HandlePong(string response)
    {
        // This runs on the Main Thread automatically thanks to the Channel Architecture
        Debug.Log($"<color=green>[Client] Received Pong:</color> {response}");
    }
}