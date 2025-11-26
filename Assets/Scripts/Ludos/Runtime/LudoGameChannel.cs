using Ludos.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLudoChannel", menuName = "Ludos/Game Channel")]
public class LudoGameChannel : ScriptableObject
{
    [Header("Settings")] 
    public int seed = 12345;
    
    [Range(2, 4)] 
    public int playerCount = 4;

    // The pure C# instance (Runtime only, not saved to disk)
    [System.NonSerialized] 
    public LudoGame Game;

    private void OnEnable()
    {
        // Ensure game exists on load/play
        if (Game == null) CreateGame();
    }

    /// <summary>
    /// Replaces the current game instance externally.
    /// </summary>
    public void Refresh(LudoGame game)
    {
        Game = game;
    }

    /// <summary>
    /// Initializes a new game instance with current settings.
    /// </summary>
    public void CreateGame()
    {
        Game = new LudoGame(seed, playerCount);
    }
}