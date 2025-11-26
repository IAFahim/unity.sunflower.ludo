using System;

namespace Ludos.Core
{

    [Flags]
    public enum LudoStatus : short
    {
        None = 0,

        Success = 1 << 0,
        ExtraTurn = 1 << 1,
        CapturedOpponent = 1 << 2,
        GameWon = 1 << 3,
        TurnPassed = 1 << 4,
        ForfeitTurn = 1 << 5,

        ErrorGameEnded = 1 << 6,
        ErrorNotYourTurn = 1 << 7,
        ErrorNeedToRoll = 1 << 8,
        ErrorAlreadyRolled = 1 << 9,
        ErrorInvalidToken = 1 << 10,

        ErrorTokenInBase = 1 << 11,
        ErrorOvershotHome = 1 << 12,
        ErrorTokenCompleted = 1 << 13,
        ErrorBlocked = 1 << 14
    }
}