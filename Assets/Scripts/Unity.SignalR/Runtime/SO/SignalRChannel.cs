// FILE: Assets/SignalRLib/SO/SignalRChannel.cs
using System;
using Microsoft.AspNetCore.SignalR.Client;
using VirtueSky.Events;
using UnityEngine;
using Cysharp.Threading.Tasks; // Required

namespace SignalRLib.SO
{
    public abstract class SignalRChannel<T> : SignalRChannelBase
    {
        [SerializeField] protected BaseEvent<T> responseEvent;

        public override IDisposable Register(HubConnection connection)
        {
            if (string.IsNullOrEmpty(methodName)) return null;

            // 1. We keep this lambda Synchronous (no 'async' keyword here)
            return connection.On<T>(methodName, (data) =>
            {
                // 2. We call a separate async method and explicitly "Forget" it.
                // This tells the compiler: "I know this is async, run it separately, and if it fails, log it via UniTask."
                HandleMessageOnMainThread(data).Forget();
            });
        }

        private async UniTaskVoid HandleMessageOnMainThread(T data)
        {
            try 
            {
                await UniTask.SwitchToMainThread();
                
                if (responseEvent != null) 
                {
                    responseEvent.Raise(data);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SignalR] Error processing channel {methodName}: {ex}");
            }
        }
    }
}