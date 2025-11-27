// FILE: Assets/SignalRLib/SO/Channels/StandardChannels.cs
using UnityEngine;

namespace SignalRLib.SO.Channels
{
    // ========================================================================
    // 1. STRING CHANNEL (JSON or Text)
    // ========================================================================
    [CreateAssetMenu(menuName = "SignalR/Channels/String Channel", fileName = "CH_String_New")]
    public class StringSignalRChannel : SignalRChannel<string> { }

    // ========================================================================
    // 2. INT CHANNEL
    // ========================================================================
    [CreateAssetMenu(menuName = "SignalR/Channels/Int Channel", fileName = "CH_Int_New")]
    public class IntSignalRChannel : SignalRChannel<int> { }

    // ========================================================================
    // 3. EXAMPLE CUSTOM OBJECT CHANNEL
    // ========================================================================
    [System.Serializable]
    public struct SimplePlayerData
    {
        public string id;
        public int health;
    }

    [CreateAssetMenu(menuName = "SignalR/Channels/Player Data Channel", fileName = "CH_PlayerData")]
    public class PlayerDataSignalRChannel : SignalRChannel<SimplePlayerData> { }
}