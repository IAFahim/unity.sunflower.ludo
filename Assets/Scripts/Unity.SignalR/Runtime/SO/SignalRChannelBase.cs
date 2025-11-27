// FILE: Assets/SignalRLib/SO/SignalRChannelBase.cs
using System;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

namespace SignalRLib.SO
{
    public abstract class SignalRChannelBase : ScriptableObject
    {
        [Tooltip("The exact method name defined in the C# Server Hub")]
        public string methodName;

        /// <summary>
        /// Registers the channel and returns the subscription handle.
        /// Call .Dispose() on this handle to unsubscribe.
        /// </summary>
        public abstract IDisposable Register(HubConnection connection);
    }
}