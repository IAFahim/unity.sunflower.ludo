using System.Text.Json.Serialization;
using Shared.DTOs;

namespace SignalRLib.Core
{
    [JsonSerializable(typeof(PlayerUpdatedMessage))]
    [JsonSerializable(typeof(TransactionResult))]
    // CRITICAL: Arrays and Primitives used in SignalR methods must be defined
    [JsonSerializable(typeof(byte[]))] 
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(string[]))]
    public partial class ClientJsonContext : JsonSerializerContext
    {
    }
}