using System;
using System.Runtime.CompilerServices;

namespace Ludos.Core
{
    public class RandomDiceRoller : IDiceRoller
    {
        private readonly Random _rng;

        public RandomDiceRoller() => _rng = null;
        public RandomDiceRoller(int seed) => _rng = new Random(seed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Roll() => (byte)_rng.Next(1, 7);
    }
}