using System.Runtime.CompilerServices;

namespace Ludos.Core
{
    /// <summary>
    /// Pure logic controller for AI behavior.
    /// Stateless and allocation-free.
    /// </summary>
    public static class LudoBot
    {
        /// <summary>
        /// Executes a full turn: Rolls dice and moves the first valid token found.
        /// </summary>
        /// <param name="game">The game instance to operate on.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayTurn(LudoGame game)
        {
            // 1. Roll Logic
            if (!game.TryRollDice(out var rollResult))
            {
                // Could not roll (Game ended or error)
                return;
            }

            // 2. Move Logic
            // Only proceed if the roll logic allows a move (Success)
            // If status is TurnPassed (rolled 1-5 with all tokens in base), we stop.
            if ((rollResult.Status & LudoStatus.Success) != 0)
            {
                PerformMove(game);
            }
            
            // Note: If the bot rolled a 6, TryRollDice logic keeps the turn on the same player.
            // A more advanced bot would loop here until turn ends, but for a single "Step",
            // we return to let the outer loop/engine handle the frame.
        }
        
        private static void PerformMove(LudoGame game)
        {
            // Simple Greedy Strategy: Move the first valid token (0 to 3)
            for (int i = 0; i < LudoConstants.TokensPerPlayer; i++)
            {
                if (game.TryMoveToken(i, out var moveRes))
                {
                    if (moveRes.IsSuccess)
                    {
                        return; // Move executed successfully
                    }
                }
            }
        }
    }
}