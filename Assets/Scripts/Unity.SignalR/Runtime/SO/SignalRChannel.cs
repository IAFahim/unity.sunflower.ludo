// FILE: Assets/SignalRLib/Architecture/SignalRChannel.cs
using System;
using Microsoft.AspNetCore.SignalR.Client;
using VirtueSky.Events;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace SignalRLib.Architecture
{
    /// <summary>
    /// Generic channel. T is the DTO type (e.g., PlayerUpdatedMessage)
    /// </summary>
    public abstract class SignalRChannel<T> : SignalRChannelBase
    {
        [Header("Output")]
        [SerializeField] protected BaseEvent<T> onMessageReceived;

        public override IDisposable Register(HubConnection connection)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            // Register using SignalR's native type system.
            // Because we configure the connection with System.Text.Json options later,
            // this is AOT safe.
            return connection.On<T>(methodName, (payload) =>
            {
                // Bridge background thread -> Unity Main Thread
                DispatchSafely(payload).Forget();
            });
        }

        private async UniTaskVoid DispatchSafely(T payload)
        {
            try
            {
                await UniTask.SwitchToMainThread();
                if (onMessageReceived != null)
                {
                    onMessageReceived.Raise(payload);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalR] Error in channel '{methodName}': {ex}");
            }
        }
    }
}