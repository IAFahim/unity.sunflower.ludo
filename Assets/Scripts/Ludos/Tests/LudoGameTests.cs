using Ludos.Core;
using NUnit.Framework;

[TestFixture]
public class LudoGameTests
{
    private LudoGame _game;
    private MockDiceRoller _mockRoller;

    [SetUp]
    public void Setup()
    {
        _mockRoller = new MockDiceRoller();
        // Default 4 player game
        _game = new LudoGame(playerCount: 4, _mockRoller);
    }

    // =======================================================================
    // 1. ARCHITECTURE & MEMORY LAYOUT
    // =======================================================================

    [Test]
    public void Init_StateIsClean()
    {
        Assert.AreEqual(0, _game.State.CurrentPlayer);
        Assert.AreEqual(0, _game.State.LastDiceRoll);
        Assert.AreEqual(255, _game.State.Winner);
        Assert.AreEqual(0, _game.State.ConsecutiveSixes);
        
        // Verify ActiveSeats mask for 4 players (Binary 1111 = 15)
        Assert.AreEqual(15, _game.State.ActiveSeats);

        // Verify all tokens are at PosBase (0)
        for(int p=0; p<4; p++)
            for(int t=0; t<4; t++)
                Assert.AreEqual(LudoConstants.PosBase, _game.State.GetTokenPos(p, t));
    }

    [Test]
    public void Init_TwoPlayer_DiagonalSeats()
    {
        // 2 Players = P0 & P2 (ActiveSeats 0101 = 5)
        _game = new LudoGame(2, _mockRoller);
        
        Assert.AreEqual(5, _game.State.ActiveSeats, "Bitmask should be 0101 (5)");

        // P0 moves, passes to P2 (skipping P1)
        _mockRoller.Enqueue(1); // P0 rolls 1 (Turn passed)
        _game.TryRollDice(out _);
        
        Assert.AreEqual(2, _game.State.CurrentPlayer, "Should skip Player 1 in 2-player mode");
        
        _mockRoller.Enqueue(1); // P2 rolls 1 (Turn passed)
        _game.TryRollDice(out _);
        
        Assert.AreEqual(0, _game.State.CurrentPlayer, "Should skip Player 3 in 2-player mode");
    }

    // =======================================================================
    // 2. ROLLING LOGIC & THE "TRIPLE 6" RULE
    // =======================================================================

    [Test]
    public void Roll_SimpleWait_NeedToMove()
    {
        _mockRoller.Enqueue(5);
        bool success = _game.TryRollDice(out var result);
        
        Assert.IsTrue(success);
        Assert.AreEqual(5, result.DiceValue);
        Assert.AreEqual(LudoStatus.TurnPassed, result.Status, "5 from base should pass turn immediately");
        Assert.AreEqual(1, _game.State.CurrentPlayer, "Turn should advance");
    }

    [Test]
    public void Roll_TripleSix_Forfeit()
    {
        _mockRoller.Enqueue(6, 6, 6);

        // 1st Six
        _game.TryRollDice(out _);
        _game.State.SetTokenPos(0, 0, 1); // Cheat: Move token out so logic allows move
        _game.TryMoveToken(0, out _); 

        // 2nd Six
        _game.TryRollDice(out _);
        _game.TryMoveToken(0, out _);

        // 3rd Six -> Forfeit
        _game.TryRollDice(out var r3);
        
        Assert.AreEqual(LudoStatus.ForfeitTurn, r3.Status);
        Assert.AreEqual(1, _game.State.CurrentPlayer, "Player changed after 3rd six");
        Assert.AreEqual(0, _game.State.ConsecutiveSixes, "Counter reset");
    }

    // =======================================================================
    // 3. MOVEMENT LOGIC
    // =======================================================================

    [Test]
    public void Move_FromBase_RequiresSix()
    {
        // Setup: P0 has Token 1 on field (Pos 10), Token 0 in Base.
        _game.State.SetTokenPos(0, 1, 10); 
        
        _mockRoller.Enqueue(5);
        _game.TryRollDice(out _); // Rolls 5
        
        // Try moving Token 0 (at Base)
        _game.TryMoveToken(0, out var moveRes);
        
        Assert.IsFalse(moveRes.IsSuccess);
        Assert.AreEqual(LudoStatus.ErrorTokenInBase, moveRes.Status);
    }

    [Test]
    public void Move_FromBase_WithSix_Success()
    {
        _mockRoller.Enqueue(6);
        _game.TryRollDice(out _);
        
        bool moved = _game.TryMoveToken(0, out var res);
        
        Assert.IsTrue(moved);
        Assert.AreEqual(LudoConstants.PosStart, res.NewPos); // 1
        Assert.IsTrue(res.Status.HasFlagFast(LudoStatus.ExtraTurn));
    }

    [TestCase(1, 1, 2, LudoStatus.Success)]       // Start -> 2
    [TestCase(50, 1, 51, LudoStatus.Success)]     // End Main -> End Main
    [TestCase(51, 1, 52, LudoStatus.Success)]     // End Main -> Home Stretch
    [TestCase(56, 1, 57, LudoStatus.Success)]     // Home Stretch -> Home (Win logic is handled in game check)
    public void Move_Scenarios(int startPos, int dice, int expectedPos, LudoStatus expectedStatus)
    {
        _game.State.SetTokenPos(0, 0, (byte)startPos);
        _mockRoller.Enqueue((byte)dice);
        
        _game.TryRollDice(out _);
        _game.TryMoveToken(0, out var res);

        if (expectedStatus == LudoStatus.ErrorOvershotHome)
        {
            Assert.AreEqual(expectedStatus, res.Status);
            Assert.AreEqual(startPos, res.NewPos);
        }
        else
        {
            Assert.AreEqual((byte)expectedPos, res.NewPos);
        }
    }

    // =======================================================================
    // 4. CAPTURE LOGIC
    // =======================================================================

    [Test]
    public void Capture_Opponent_SentToBase()
    {
        // P0 (Red) moves to Absolute 15.
        // P0 Start=1. Abs 15 = Rel 15.
        // P1 (Green) is ON Absolute 15.
        // P1 Start=14. Abs 15 = Rel 2.
        
        _game.State.SetTokenPos(1, 0, 2); // P1 is sitting on field
        _game.State.SetTokenPos(0, 0, 10); // P0 is approaching
        
        _mockRoller.Enqueue(5); // 10 + 5 = 15
        _game.TryRollDice(out _);
        
        _game.TryMoveToken(0, out var res);
        
        Assert.IsTrue(res.Status.HasFlagFast(LudoStatus.CapturedOpponent));
        Assert.IsTrue(res.Status.HasFlagFast(LudoStatus.ExtraTurn));
        
        // Check P1 was sent home
        Assert.AreEqual(LudoConstants.PosBase, _game.State.GetTokenPos(1, 0));
        Assert.AreEqual(1, res.CapturedPid);
        Assert.AreEqual(0, res.CapturedTid);
    }
    

    // =======================================================================
    // 5. WIN CONDITION
    // =======================================================================

    [Test]
    public void Game_WinCondition()
    {
        // Set P0 tokens 0,1,2 to Home
        _game.State.SetTokenPos(0, 0, LudoConstants.PosHome);
        _game.State.SetTokenPos(0, 1, LudoConstants.PosHome);
        _game.State.SetTokenPos(0, 2, LudoConstants.PosHome);
        
        // Set P0 token 3 to 56 (needs 1)
        _game.State.SetTokenPos(0, 3, 56);
        
        _mockRoller.Enqueue(1);
        _game.TryRollDice(out _);
        _game.TryMoveToken(3, out var res);
        
        Assert.IsTrue(res.Status.HasFlagFast(LudoStatus.GameWon));
        Assert.AreEqual(0, _game.State.Winner);
        Assert.IsTrue(_game.IsGameWon);
        
        // Ensure game is locked
        _mockRoller.Enqueue(6);
        bool rollPossible = _game.TryRollDice(out var finalRes);
        Assert.IsFalse(rollPossible);
        Assert.AreEqual(LudoStatus.ErrorGameEnded, finalRes.Status);
    }

    // =======================================================================
    // 6. BOT & FUZZ TEST
    // =======================================================================

    [Test]
    public void Bot_PlayTurn_AdvancesGame()
    {
        // Setup: P0 can move out of base
        _mockRoller.Enqueue(6);
        
        // Act: Bot Plays
        LudoBot.PlayTurn(_game);
        
        // Assert: 
        // 1. Dice rolled (6)
        // 2. Token moved (0 -> 1)
        // 3. Current Player still 0 (Extra turn)
        Assert.AreEqual(LudoConstants.PosStart, _game.State.GetTokenPos(0, 0));
        Assert.AreEqual(0, _game.State.CurrentPlayer);
        
        // Act: Bot Plays Again (Roll 1)
        _mockRoller.Enqueue(1);
        LudoBot.PlayTurn(_game);
        
        // Assert: 
        // 1. Token moved (1 -> 2)
        // 2. Turn advanced to P1
        Assert.AreEqual(2, _game.State.GetTokenPos(0, 0));
        Assert.AreEqual(1, _game.State.CurrentPlayer);
    }
    
    [Test]
    public void Fuzz_RandomGame_DoesNotCrash()
    {
        var rngGame = new LudoGame(42, 4); // Use internal random roller
        
        for (int i = 0; i < 2000; i++)
        {
            if (rngGame.IsGameWon) break;
            LudoBot.PlayTurn(rngGame);
        }
        Assert.Pass("Survived 2000 turns of random play");
    }
}