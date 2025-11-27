// FILE: Assets/SignalRLib/Architecture/SignalRHubServiceSO.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection; // For Json options
using UnityEngine;
using Cysharp.Threading.Tasks;
using SignalRLib.Core;
using SignalRLib.SO; // For ClientJsonContext
using VirtueSky.Events;
using VirtueSky.Inspector;

namespace SignalRLib.Architecture
{
    [CreateAssetMenu(menuName = "SignalR/Architecture/Hub Service", fileName = "NewHubService")]
    public class SignalRHubServiceSO : ScriptableObject
    {
        [Header("Config")]
        [SerializeField] private string serverUrl = "http://localhost:5525";
        [SerializeField] private string hubPath = "/hubs/game";
        
        [Header("Dependencies")]
        [SerializeField] private AuthSessionSO authSession;
        
        [Header("Listening Channels")]
        [SerializeField] private List<SignalRChannelBase> incomingChannels;

        [Header("Events")]
        [SerializeField] private StringEvent onStatusChanged;

        // Runtime State
        private HubConnection _connection;
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private bool _isActive;

        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        [Button]
        public async UniTask ConnectAsync()
        {
            if (_isActive || IsConnected) return;
            _isActive = true;

            await DisposeInternal(); // Cleanup old state

            var fullUrl = $"{serverUrl}{hubPath}";

            _connection = new HubConnectionBuilder()
                .WithUrl(fullUrl, options =>
                {
                    // Dynamic Token Injection
                    options.AccessTokenProvider = () => 
                        System.Threading.Tasks.Task.FromResult(authSession?.AccessToken);
                    
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    options.SkipNegotiation = true;
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                // CRITICAL: Inject our AOT-Friendly JSON Context
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.TypeInfoResolver = ClientJsonContext.Default;
                })
                .Build();

            BindLifecycle();
            RegisterChannels();

            try
            {
                Debug.Log($"[SignalR] Connecting to {hubPath}...");
                await _connection.StartAsync();
                NotifyStatus("Connected");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalR] Connection Error: {ex.Message}");
                NotifyStatus("Failed");
                _isActive = false;
            }
        }

        [Button]
        public async UniTask DisconnectAsync()
        {
            _isActive = false;
            await DisposeInternal();
            NotifyStatus("Disconnected");
        }

        // ========================================================================
        // Sending (Typesafe & AOT Safe)
        // ========================================================================

        public void Send(string method, object arg1) => SendCore(method, new[] { arg1 }).Forget();
        public void Send(string method, object arg1, object arg2) => SendCore(method, new[] { arg1, arg2 }).Forget();

        private async UniTaskVoid SendCore(string method, object[] args)
        {
            if (!IsConnected)
            {
                Debug.LogWarning($"[SignalR] '{name}' not connected. Dropping message '{method}'");
                return;
            }

            try
            {
                await _connection.SendCoreAsync(method, args);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalR] Send Failed: {ex.Message}");
            }
        }

        // ========================================================================
        // Internals
        // ========================================================================

        private void RegisterChannels()
        {
            foreach (var ch in incomingChannels)
            {
                if (ch == null) continue;
                var sub = ch.Register(_connection);
                if (sub != null) _subscriptions.Add(sub);
            }
        }

        private void BindLifecycle()
        {
            _connection.Closed += (ex) => 
            { 
                LifecycleEvent("Disconnected", ex).Forget(); 
                return System.Threading.Tasks.Task.CompletedTask; 
            };
            
            _connection.Reconnecting += (ex) => 
            { 
                LifecycleEvent("Reconnecting", ex).Forget(); 
                return System.Threading.Tasks.Task.CompletedTask; 
            };
            
            _connection.Reconnected += (id) => 
            { 
                LifecycleEvent("Connected", null).Forget(); 
                return System.Threading.Tasks.Task.CompletedTask; 
            };
        }

        private async UniTaskVoid LifecycleEvent(string status, Exception ex)
        {
            await UniTask.SwitchToMainThread();
            NotifyStatus(status);
            if (ex != null) Debug.LogWarning($"[SignalR] {name}: {ex.Message}");
        }

        private void NotifyStatus(string status) => onStatusChanged?.Raise(status);

        private async UniTask DisposeInternal()
        {
            foreach (var sub in _subscriptions) sub.Dispose();
            _subscriptions.Clear();

            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        
        // Safety Net: Cleanup if Unity exits
        private void OnDisable() => DisconnectAsync().Forget();
    }
}