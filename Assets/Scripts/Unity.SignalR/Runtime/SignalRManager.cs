// ============================================================================
// FILE: Assets/SignalRLib/Core/SignalRManager.cs
// ============================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Cysharp.Threading.Tasks;
using SignalRLib.SO;
using VirtueSky.Events;
using VirtueSky.Inspector;

namespace SignalRLib.Core
{
    [EditorIcon("icon_signalr")] // Optional: Add icon if you have one
    public class SignalRManager : MonoBehaviour
    {
        [Header("Configuration")] [SerializeField]
        private ServerConfigSO serverConfig;

        [Header("Channels (Inputs)")]
        [Tooltip("List of all channels to subscribe to. Each handles its own Type.")]
        [SerializeField]
        private List<SignalRChannelBase> incomingChannels;

        [Header("System Events (Outputs)")] [SerializeField]
        private BaseEvent<string> onConnectionStatusChanged;

        // Runtime State
        private HubConnection _connection;
        private readonly List<IDisposable> _channelSubscriptions = new List<IDisposable>();
        private bool _isShuttingDown;

        // Public Accessors
        public string ConnectionId => _connection?.ConnectionId;
        public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

        private void OnDestroy()
        {
            _isShuttingDown = true;
            DisconnectAsync().Forget();
        }

        /// <summary>
        /// Main entry point. Initializes config, builds the hub, registers channels, and connects.
        /// </summary>
        [Button]
        public async UniTask ConnectAsync()
        {
            if (State == HubConnectionState.Connected || State == HubConnectionState.Connecting)
            {
                Debug.LogWarning("[SignalRManager] Already connected or connecting.");
                return;
            }

            if (serverConfig == null)
            {
                Debug.LogError("[SignalRManager] Config is null! Cannot connect.");
                return;
            }

            // 1. Initialize Config (fetch remote URL if needed)
            await serverConfig.InitializeAsync();

            // 2. Prepare Retry Delays (Convert Int[] ms to TimeSpan[])
            // This allows Inspector editing in milliseconds, but SignalR gets TimeSpans
            var retryPolicy = Array.ConvertAll(
                serverConfig.reconnectDelaysMs,
                ms => TimeSpan.FromMilliseconds(ms)
            );

            // 3. Build Connection
            _connection = new HubConnectionBuilder()
                .WithUrl(serverConfig.ActiveUrl, options =>
                {
                    if (!string.IsNullOrEmpty(serverConfig.AccessToken))
                    {
                        options.AccessTokenProvider = () =>
                            System.Threading.Tasks.Task.FromResult(serverConfig.AccessToken);
                    }

                    // WebSockets is the most performant for Unity
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    // Skip negotiation saves 1 RTT if we know we use WebSockets
                    options.SkipNegotiation = true;
                })
                .WithAutomaticReconnect(retryPolicy)
                .ConfigureLogging(logging =>
                {
                    // Optional: Tune logging level based on debug/release
                    logging.SetMinimumLevel(LogLevel.Warning);
                })
                .Build();

            // 4. Register Smart Channels (Subscribe)
            RegisterChannels();

            // 5. Lifecycle Bindings
            BindLifecycleEvents();

            // 6. Connect
            try
            {
                Debug.Log($"[SignalR] Connecting to {serverConfig.ActiveUrl}...");
                await _connection.StartAsync();

                Debug.Log($"<color=green>[SignalR] Connected! ID: {_connection.ConnectionId}</color>");
                NotifyStatus("Connected");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalR] Connection Failed: {ex.Message}");
                NotifyStatus("Failed");
            }
        }

        /// <summary>
        /// Cleanly closes the connection and disposes all listeners.
        /// </summary>
        [Button]
        public async UniTask DisconnectAsync()
        {
            // 1. Remove listeners first to stop incoming events
            UnsubscribeChannels();

            // 2. Stop the connection
            if (_connection != null)
            {
                try
                {
                    await _connection.StopAsync();
                }
                catch
                {
                    /* Ignore stop errors */
                }

                await _connection.DisposeAsync();
                _connection = null;
            }

            NotifyStatus("Disconnected");
        }

        // ========================================================================
        // Channel Management (The Smart Part)
        // ========================================================================

        private void RegisterChannels()
        {
            // Safety: Ensure we don't have lingering subscriptions
            UnsubscribeChannels();

            if (incomingChannels == null) return;

            foreach (var channel in incomingChannels)
            {
                if (channel == null) continue;

                // Polymorphic Call: The channel knows its own Type <T>
                // It returns a handle we can use to unsubscribe later.
                var subscription = channel.Register(_connection);

                if (subscription != null)
                {
                    _channelSubscriptions.Add(subscription);
                }
            }
        }

        private void UnsubscribeChannels()
        {
            foreach (var sub in _channelSubscriptions)
            {
                sub?.Dispose();
            }

            _channelSubscriptions.Clear();
        }

        // ========================================================================
        // Outgoing Messages
        // ========================================================================

        /// <summary>
        /// Fire and forget message to server.
        /// </summary>
        public void Send(string methodName, params object[] args) => SendAsync(methodName, args).Forget();

        /// <summary>
        /// Async send message to server.
        /// </summary>
        public async UniTask SendAsync(string methodName, params object[] args)
        {
            if (State != HubConnectionState.Connected)
            {
                Debug.LogWarning($"[SignalR] Cannot send '{methodName}', not connected.");
                return;
            }

            try
            {
                await _connection.SendCoreAsync(methodName, args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalR] Send Failed: {ex.Message}");
            }
        }

        // ========================================================================
        // Lifecycle & Utils
        // ========================================================================

        private void BindLifecycleEvents()
        {
            // SignalR events (Closed, Reconnecting) expect Func<Exception, Task>
            // So 'async' IS ALLOWED here by SignalR itself, but we need to be careful with the body.
    
            _connection.Closed += (ex) => 
            {
                // Fire-and-forget wrapper
                HandleConnectionClosed(ex).Forget();
                return System.Threading.Tasks.Task.CompletedTask;
            };

            _connection.Reconnecting += (ex) => 
            {
                HandleReconnecting(ex).Forget();
                return System.Threading.Tasks.Task.CompletedTask;
            };
    
            _connection.Reconnected += (id) => 
            {
                HandleReconnected(id).Forget();
                return System.Threading.Tasks.Task.CompletedTask;
            };
        }
        
        private async UniTaskVoid HandleConnectionClosed(Exception ex)
        {
            await UniTask.SwitchToMainThread();
            if (_isShuttingDown) return;

            NotifyStatus("Disconnected");
            if(ex != null) Debug.LogWarning($"[SignalR] Connection Closed: {ex.Message}");
        }

        private async UniTaskVoid HandleReconnecting(Exception ex)
        {
            await UniTask.SwitchToMainThread();
            if (_isShuttingDown) return;

            NotifyStatus("Reconnecting");
        }

        private async UniTaskVoid HandleReconnected(string id)
        {
            await UniTask.SwitchToMainThread();
            if (_isShuttingDown) return;

            NotifyStatus("Connected");
            Debug.Log($"[SignalR] Reconnected: {id}");
        }

        private void NotifyStatus(string status)
        {
            onConnectionStatusChanged?.Raise(status);
        }
    }
}