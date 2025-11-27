// FILE: Assets/Ludos/Client/Channels/LudoGameStateChannel.cs
using SignalRLib.Architecture;
using UnityEngine;

namespace Ludos.Client.Channels
{
    // Listens for "GameState" (byte[]) from server
    [CreateAssetMenu(menuName = "Ludo/Channels/Game State Channel")]
    public class LudoGameStateChannel : SignalRChannel<byte[]> { }
}