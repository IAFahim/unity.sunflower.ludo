using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pushers
{
    [CreateAssetMenu(fileName = "GridScript", menuName = "GridScript/New")]
    public class GridScript : ScriptableObject
    {
        public Dictionary<Vector2Int, GameObject> Map = new();
    }
}