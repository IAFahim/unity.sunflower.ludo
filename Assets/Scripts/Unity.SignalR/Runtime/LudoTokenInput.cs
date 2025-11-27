// FILE: Assets/Ludos/Client/Visuals/LudoTokenInput.cs
using UnityEngine;

namespace Ludos.Client.Visuals
{
    public class LudoTokenInput : MonoBehaviour
    {
        private LudoGameManager _manager;
        private int _tokenIndex;

        public void Setup(LudoGameManager manager, int tokenIndex)
        {
            _manager = manager;
            _tokenIndex = tokenIndex;
        }

        private void OnMouseDown()
        {
            // In a real game, you should check if this token belongs to the local player first
            // But the server validates it anyway, so sending it is safe.
            
            // HARDCODED ROOM ID FOR DEMO
            // In production, Manager holds the current RoomID
            _manager.SendMoveToken("room_1", _tokenIndex);
        }
    }
}