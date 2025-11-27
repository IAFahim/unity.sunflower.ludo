// FILE: Assets/Ludos/Client/Channels/LudoIntChannel.cs
using SignalRLib.Architecture;
using UnityEngine;

namespace Ludos.Client.Channels
{
    // Listens for "RollResult" (int)
    [CreateAssetMenu(menuName = "Ludo/Channels/Int Channel")]
    public class LudoIntChannel : SignalRChannel<int> { }
}