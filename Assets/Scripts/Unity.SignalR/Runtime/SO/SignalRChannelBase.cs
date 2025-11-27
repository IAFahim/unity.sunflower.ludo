// FILE: Assets/SignalRLib/Architecture/SignalRChannelBase.cs
using System;
using Microsoft.AspNetCore.SignalR.Client;
using UnityEngine;

namespace SignalRLib.Architecture
{
    public abstract class SignalRChannelBase : ScriptableObject
    {
        [Tooltip("Method name on the Server Hub")]
        public string methodName;

        /// <summary>
        /// Registers the listener and returns a disposable handle
        /// </summary>
        public abstract IDisposable Register(HubConnection connection);
    }
}