using UnityEngine;
using VirtueSky.Events;

namespace Unity.SignalR.Runtime
{
    [CreateAssetMenu(fileName = "BaseEvent", menuName = "Sunflower/Scriptable/Event/Byte Array Event")]
    public class ByteArrayEvent : BaseEvent<byte[]>
    {
    }
}