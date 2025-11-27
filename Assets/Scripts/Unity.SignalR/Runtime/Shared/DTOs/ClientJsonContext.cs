// FILE: Assets/SignalRLib/Core/ClientJsonContext.cs
using System.Text.Json.Serialization;
using UnityEngine;

// Define your DTOs here (Mirrors your Backend DTOs)
namespace Shared.DTOs
{
    [System.Serializable]
    public record PlayerUpdatedMessage
    {
        public string UserId;
        public long NewCoins;
        public string Username;
        public string Email;
    };

    [System.Serializable]
    public record TransactionResult
    {
        public bool Success;
        public long NewBalance;
        public string ErrorMessage;
    };
    
    [System.Serializable]
    public struct Vector3Data { public float x, y, z; }
}

namespace SignalRLib.Core
{
    /// <summary>
    /// AOT-Safe JSON Context. 
    /// This generates the serialization code at compile time. No Reflection.
    /// </summary>
    [JsonSerializable(typeof(Shared.DTOs.PlayerUpdatedMessage))]
    [JsonSerializable(typeof(Shared.DTOs.TransactionResult))]
    [JsonSerializable(typeof(Shared.DTOs.Vector3Data))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(int))]
    public partial class ClientJsonContext : JsonSerializerContext
    {
    }
}