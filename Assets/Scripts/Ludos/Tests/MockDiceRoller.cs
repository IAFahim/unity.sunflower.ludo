using System.Collections.Generic;
using Ludos.Core;

public class MockDiceRoller : IDiceRoller
{
    private readonly Queue<byte> _scriptedRolls = new Queue<byte>();

    public void Enqueue(params byte[] rolls)
    {
        foreach (var r in rolls) _scriptedRolls.Enqueue(r);
    }

    public void Clear() => _scriptedRolls.Clear();

    public byte Roll()
    {
        if (_scriptedRolls.Count > 0)
            return _scriptedRolls.Dequeue();
        
        // Default fallback
        return 1; 
    }
}