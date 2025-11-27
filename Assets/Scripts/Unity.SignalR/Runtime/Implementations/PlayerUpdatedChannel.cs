using Shared.DTOs;
using SignalRLib.Architecture;
using UnityEngine;

namespace SignalRLib.Implementations
{
    [CreateAssetMenu(menuName = "SignalR/Channels/PlayerUpdated Channel")]
    public class PlayerUpdatedChannel : SignalRChannel<PlayerUpdatedMessage> { }
}