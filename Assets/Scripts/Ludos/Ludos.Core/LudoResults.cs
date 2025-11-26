namespace Ludos.Core
{
    public readonly struct RollResult
    {
        public readonly LudoStatus Status;
        public readonly byte DiceValue;

        public RollResult(LudoStatus status, byte dice)
        {
            Status = status;
            DiceValue = dice;
        }

        public bool IsValid => (Status & LudoStatus.Success) != 0 || (Status & LudoStatus.TurnPassed) != 0 ||
                               (Status & LudoStatus.ForfeitTurn) != 0;
    }

    public readonly struct MoveResult
    {
        public readonly LudoStatus Status;
        public readonly byte NewPos;
        public readonly int CapturedPid;
        public readonly int CapturedTid;

        public MoveResult(LudoStatus status, byte pos, int capPid = -1, int capTid = -1)
        {
            Status = status;
            NewPos = pos;
            CapturedPid = capPid;
            CapturedTid = capTid;
        }

        public bool IsSuccess => (Status & LudoStatus.Success) != 0;
    }
}