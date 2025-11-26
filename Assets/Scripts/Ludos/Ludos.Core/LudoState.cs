using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ludos.Core
{
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public unsafe struct LudoState
    {
        [FieldOffset(0)] public fixed byte Tokens[16];

        [FieldOffset(16)] public byte CurrentPlayer;
        [FieldOffset(17)] public byte LastDiceRoll;
        [FieldOffset(18)] public byte ConsecutiveSixes;
        [FieldOffset(19)] public byte Winner;

        [FieldOffset(20)] public int TurnId;

        [FieldOffset(24)] public byte ActiveSeats;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetTokenPos(int player, int tokenIndex)
        {
            return Tokens[(player << 2) + tokenIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTokenPos(int player, int tokenIndex, byte pos)
        {
            Tokens[(player << 2) + tokenIndex] = pos;
        }

        public void Init(byte activeSeatsMask)
        {
            fixed (byte* ptr = Tokens)
            {
                *(long*)ptr = 0;
                *(long*)(ptr + 8) = 0;
            }

            CurrentPlayer = 0;
            LastDiceRoll = 0;
            ConsecutiveSixes = 0;
            Winner = 255;
            TurnId = 1;
            ActiveSeats = activeSeatsMask;

            if ((ActiveSeats & 1) == 0)
            {
                AdvanceTurnPointer();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AdvanceTurnPointer()
        {
            do
            {
                CurrentPlayer = (byte)((CurrentPlayer + 1) & 3);
            } 
            while ((ActiveSeats & (1 << CurrentPlayer)) == 0);
        }

        public override string ToString()
        {
            char diceChar = LastDiceRoll == 0 ? '◌' : (char)(0x267F + LastDiceRoll);
            string pIcon = CurrentPlayer switch { 0 => "🔴", 1 => "🟢", 2 => "🟡", 3 => "🔵", _ => "?" };
            
            System.Text.StringBuilder sb = new();
            sb.Append($"T{TurnId:D3} {pIcon} [{diceChar}] ");

            for (int i = 0; i < 4; i++)
            {
                if ((ActiveSeats & (1 << i)) == 0) continue;

                sb.Append(i == 0 ? "{" : " | ");
                for (int t = 0; t < 4; t++)
                {
                    byte pos = GetTokenPos(i, t);
                    string s = pos == 0 ? "_" : (pos == 57 ? "★" : pos.ToString());
                    sb.Append(t == 0 ? s : $",{s}");
                }
            }
            sb.Append("}");
            return sb.ToString();
        }
    }
}