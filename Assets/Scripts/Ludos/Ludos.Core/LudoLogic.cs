using System.Runtime.CompilerServices;

namespace Ludos.Core
{
    public static class LudoLogic
    {
        private const ulong SafeTilesMask =
            (1UL << 0) | (1UL << 13) |
            (1UL << 26) | (1UL << 39);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSafeTile(int absPos) 
        {
            return (SafeTilesMask & (1UL << absPos)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToAbsolute(int playerIndex, byte relativePos)
        {
            if (relativePos < LudoConstants.PosStart || relativePos > LudoConstants.PosEndMain) return -1;

            return (relativePos - 1 + (playerIndex * LudoConstants.QuadrantSize)) % LudoConstants.TrackLength;
        }

        public static LudoStatus PredictMove(byte currentPos, byte dice, out byte nextPos)
        {
            nextPos = currentPos;

            if (currentPos == LudoConstants.PosHome)
                return LudoStatus.ErrorTokenCompleted;

            if (currentPos == LudoConstants.PosBase)
            {
                if (dice == LudoConstants.EntryDice)
                {
                    nextPos = LudoConstants.PosStart;
                    return LudoStatus.Success;
                }
                return LudoStatus.ErrorTokenInBase;
            }

            int potential = currentPos + dice;

            if (currentPos <= LudoConstants.PosEndMain && potential > LudoConstants.PosEndMain)
            {
                if (potential > LudoConstants.PosHome)
                    return LudoStatus.ErrorOvershotHome;

                nextPos = (byte)potential;
            }
            else if (currentPos >= LudoConstants.PosHomeStretchStart)
            {
                if (potential > LudoConstants.PosHome)
                    return LudoStatus.ErrorOvershotHome;

                nextPos = (byte)potential;
            }
            else
            {
                nextPos = (byte)potential;
            }

            return LudoStatus.Success;
        }
    }
}