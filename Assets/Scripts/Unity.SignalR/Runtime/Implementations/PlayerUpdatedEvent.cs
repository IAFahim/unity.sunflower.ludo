using Shared.DTOs;
using UnityEngine;
using VirtueSky.Events;

namespace SignalRLib.Implementations
{
    [CreateAssetMenu(menuName = "SignalR/Game/PlayerUpdated")]
    public class PlayerUpdatedEvent : BaseEvent<PlayerUpdatedMessage> { }
}