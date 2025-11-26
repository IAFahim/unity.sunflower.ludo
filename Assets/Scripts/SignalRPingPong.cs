using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

public class SignalRPingPong : MonoBehaviour
{
    private HubConnection _connection;
    
    // ADJUST URL IF NEEDED
    private const string ServerUrl = "http://localhost:5525/hubs/game";

    [Header("Auth")]
    [TextArea] 
    public string accessToken = "PASTE_YOUR_TOKEN_HERE";

    private async void Start()
    {
        // -------------------------------------------------------------
        // FIX: Initialize Dispatcher on Main Thread immediately!
        // -------------------------------------------------------------
        var dispatcher = UnityMainThreadDispatcher.Instance();
        
        Debug.Log($"Dispatcher ID: {dispatcher.GetInstanceID()} (Initialized on Main Thread)");

        // 1. Setup Connection
        _connection = new HubConnectionBuilder()
            .WithUrl(ServerUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(accessToken);
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                options.SkipNegotiation = true;
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddProvider(new UnityLoggerProvider());
            })
            .WithAutomaticReconnect()
            .Build();

        // 2. Register Handler
        _connection.On<string>("Pong", (message) =>
        {
            // Now this is safe because Instance() was already created on Main Thread
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"<color=green>PONG RECEIVED!</color> {message}");
            });
        });

        // 3. Connect
        try
        {
            Debug.Log("Connecting...");
            await _connection.StartAsync();
            Debug.Log("Connected to GameHub!");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection Failed: {ex.Message}");
        }
    }

    public async void SendPing()
    {
        if (_connection != null && _connection.State == HubConnectionState.Connected)
        {
            string msg = $"Ping-{Time.frameCount}";
            Debug.Log($"Sending: {msg}");
            try 
            {
                await _connection.InvokeAsync("Ping", msg);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Send Failed: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Cannot Ping: Not Connected");
        }
    }

    private async void OnDestroy()
    {
        if (_connection != null) await _connection.DisposeAsync();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "SEND PING"))
        {
            SendPing();
        }
    }

    // LOGGER
    private class UnityLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new UnityLogger();
        public void Dispose() { }
    }

    private class UnityLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string msg = formatter(state, exception);
            // Only log errors or specific messages to avoid spam
            if (logLevel >= LogLevel.Error || msg.Contains("Pong") || msg.Contains("Invoking"))
            {
                // Dispatch logging to main thread to be safe (though Debug.Log is usually thread-safe)
                UnityMainThreadDispatcher.Instance().Enqueue(() => 
                {
                    Debug.Log($"[SignalR-Internal] {msg}"); 
                });
            }
        }
    }
}

// DISPATCHER
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (!_instance)
        {
            // This check prevents creating GameObjects on background threads
            if (System.Threading.Thread.CurrentThread.ManagedThreadId != 1 && Application.isPlaying)
            {
                // If this throws, it means we forgot to call Instance() in Start()
                throw new Exception("Attempted to create Dispatcher on background thread! Call UnityMainThreadDispatcher.Instance() in Start().");
            }

            var go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}