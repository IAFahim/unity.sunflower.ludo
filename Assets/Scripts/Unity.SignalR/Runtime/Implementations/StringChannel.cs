using SignalRLib.Architecture;
using UnityEngine;

namespace SignalRLib.Implementations
{
    [CreateAssetMenu(menuName = "SignalR/Channels/String Channel")]
    public class StringChannel : SignalRChannel<string> { }
}