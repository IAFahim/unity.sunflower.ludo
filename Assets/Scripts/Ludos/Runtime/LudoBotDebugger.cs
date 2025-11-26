using System.Runtime.CompilerServices;
using Ludos.Core;
using UnityEngine;

public class LudoBotDebugger : MonoBehaviour
{
    public LudoGameChannel gameChannel;
    public bool autoPlay = false;
    public float autoStepInterval = 0.1f;

    private float _timer;

    private void Update()
    {
        if (autoPlay)
        {
            _timer += Time.deltaTime;
            if (_timer >= autoStepInterval)
            {
                _timer = 0;
                Step();
            }
        }
    }

    [ContextMenu("Step Bot Turn")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Step()
    {
        LudoBot.PlayTurn(gameChannel.Game);
    }
    
    [ContextMenu("Reset Game")]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetGame()
    {
        gameChannel.CreateGame();
    }
}