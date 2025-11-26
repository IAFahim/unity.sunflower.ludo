using System;
using System.Runtime.CompilerServices;

namespace Ludos.Core
{
    public class LudoGame
    {
        public LudoState State;
        
        private readonly IDiceRoller _roller;

        public LudoGame(int playerCount, IDiceRoller roller)
        {
            if (playerCount < 2 || playerCount > 4) 
                throw new ArgumentOutOfRangeException(nameof(playerCount));
                
            _roller = roller;

            byte mask = 0;
            if (playerCount == 2)
            {
                mask = 0b00000101;
            }
            else
            {
                for (int i = 0; i < playerCount; i++) mask |= (byte)(1 << i);
            }

            State.Init(mask);
        }

        public LudoGame(int seed, int playerCount) : this(playerCount, new RandomDiceRoller(seed)) { }

        public bool IsGameWon => State.Winner != 255;

        public bool TryRollDice(out RollResult result)
        {
            if (State.Winner != 255)
            {
                result = new RollResult(LudoStatus.ErrorGameEnded, 0);
                return false;
            }

            if (State.LastDiceRoll != 0)
            {
                result = new RollResult(LudoStatus.ErrorAlreadyRolled, State.LastDiceRoll);
                return false;
            }

            byte dice = _roller.Roll();
            State.LastDiceRoll = dice;

            if (dice == 6)
            {
                State.ConsecutiveSixes++;
                if (State.ConsecutiveSixes >= LudoConstants.MaxConsecutiveSixes)
                {
                    EndTurn(advancePlayer: true);
                    result = new RollResult(LudoStatus.ForfeitTurn, dice);
                    return true;
                }
            }
            else
            {
                State.ConsecutiveSixes = 0;
            }

            bool anyMovePossible = false;
            int pIdx = State.CurrentPlayer;
            for (int i = 0; i < 4; i++)
            {
                byte pos = State.GetTokenPos(pIdx, i);
                if (LudoLogic.PredictMove(pos, dice, out _) == LudoStatus.Success)
                {
                    anyMovePossible = true;
                    break;
                }
            }

            if (!anyMovePossible)
            {
                bool isBonusRoll = (dice == 6);
                EndTurn(advancePlayer: !isBonusRoll);
                result = new RollResult(isBonusRoll ? LudoStatus.Success : LudoStatus.TurnPassed, dice);
                return true;
            }

            result = new RollResult(LudoStatus.Success, dice);
            return true;
        }

        public unsafe bool TryMoveToken(int tokenIndex, out MoveResult result)
        {
            if (State.Winner != 255)
            {
                result = new MoveResult(LudoStatus.ErrorGameEnded, 0);
                return false;
            }

            if (State.LastDiceRoll == 0)
            {
                result = new MoveResult(LudoStatus.ErrorNeedToRoll, 0);
                return false;
            }

            if ((uint)tokenIndex >= 4)
            {
                result = new MoveResult(LudoStatus.ErrorInvalidToken, 0);
                return false;
            }

            int pIdx = State.CurrentPlayer;
            byte currentPos = State.GetTokenPos(pIdx, tokenIndex);

            LudoStatus status = LudoLogic.PredictMove(currentPos, State.LastDiceRoll, out byte nextPos);

            if (status != LudoStatus.Success)
            {
                result = new MoveResult(status, currentPos);
                return true;
            }

            State.SetTokenPos(pIdx, tokenIndex, nextPos);
            LudoStatus outcome = LudoStatus.Success;

            if (nextPos == LudoConstants.PosHome)
            {
                int homeCount = 0;
                if (State.GetTokenPos(pIdx, 0) == LudoConstants.PosHome) homeCount++;
                if (State.GetTokenPos(pIdx, 1) == LudoConstants.PosHome) homeCount++;
                if (State.GetTokenPos(pIdx, 2) == LudoConstants.PosHome) homeCount++;
                if (State.GetTokenPos(pIdx, 3) == LudoConstants.PosHome) homeCount++;

                if (homeCount == 4)
                {
                    State.Winner = (byte)pIdx;
                    outcome |= LudoStatus.GameWon;
                }
            }

            int capPid = -1, capTid = -1;
            int absDest = LudoLogic.ToAbsolute(pIdx, nextPos);

            if (absDest != -1 && !LudoLogic.IsSafeTile(absDest))
            {
                for (int i = 0; i < 16; i++)
                {
                    int owner = i >> 2;
                    if (owner == pIdx) continue;

                    byte otherPos = State.Tokens[i];
                    if (otherPos == 0) continue;

                    if (LudoLogic.ToAbsolute(owner, otherPos) == absDest)
                    {
                        State.Tokens[i] = LudoConstants.PosBase;
                        capPid = owner;
                        capTid = i & 3;
                        outcome |= LudoStatus.CapturedOpponent;
                        break;
                    }
                }
            }

            bool rolledSix = (State.LastDiceRoll == 6);
            bool captured = (outcome & LudoStatus.CapturedOpponent) != 0;

            if (rolledSix || captured)
            {
                outcome |= LudoStatus.ExtraTurn;
            }

            result = new MoveResult(outcome, nextPos, capPid, capTid);

            if ((outcome & LudoStatus.GameWon) == 0)
            {
                EndTurn(advancePlayer: !result.Status.HasFlagFast(LudoStatus.ExtraTurn));
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EndTurn(bool advancePlayer)
        {
            State.LastDiceRoll = 0;

            if (advancePlayer)
            {
                State.AdvanceTurnPointer();
                State.ConsecutiveSixes = 0;
                State.TurnId++;
            }
        }
    }
}