using UnityEngine;

namespace Ludos.Client.Visuals
{
    public class LudoBoardLayout : MonoBehaviour
    {
        [Header("Shared Track")]
        [Tooltip("The 52 tiles forming the main loop. Index 0 must be Red's starting tile.")]
        public Transform[] mainTrack; // Size should be 52

        [Header("Player Specifics")]
        public PlayerPath[] playerPaths; // Size must be 4

        [System.Serializable]
        public class PlayerPath
        {
            public string name;
            public Color debugColor = Color.white;
            public Transform baseAnchor;       // Logic 0
            public Transform[] homeStretch;    // Logic 52, 53, 54, 55, 56
            public Transform homeGoal;         // Logic 57
        }

        // ========================================================================
        // THE MAPPING LOGIC
        // ========================================================================

        /// <summary>
        /// Converts Abstract Logic Position (0-57) to World Vector3
        /// </summary>
        public Vector3 GetWorldPosition(int playerIndex, byte logicPos, int tokenIndex)
        {
            if (playerIndex < 0 || playerIndex >= playerPaths.Length) return Vector3.zero;
            var pathData = playerPaths[playerIndex];

            // 1. BASE (Logic 0)
            if (logicPos == 0)
            {
                return GetBasePositionOffset(pathData.baseAnchor, tokenIndex);
            }

            // 2. MAIN TRACK (Logic 1 - 51)
            if (logicPos <= 51)
            {
                // Calculate Offset based on Player Color
                // Red (0) starts at 0
                // Green (1) starts at 13
                // Yellow (2) starts at 26
                // Blue (3) starts at 39
                int startOffset = playerIndex * 13;
                
                // Logic 1 is Index 0, so we subtract 1
                int actualIndex = (startOffset + (logicPos - 1)) % 52;
                
                if (mainTrack != null && actualIndex < mainTrack.Length)
                    return mainTrack[actualIndex].position;
            }

            // 3. HOME STRETCH (Logic 52 - 56)
            if (logicPos >= 52 && logicPos <= 56)
            {
                int stretchIndex = logicPos - 52;
                if (pathData.homeStretch != null && stretchIndex < pathData.homeStretch.Length)
                    return pathData.homeStretch[stretchIndex].position;
            }

            // 4. GOAL (Logic 57)
            if (logicPos == 57)
            {
                // We can add a small offset so 4 tokens don't stack perfectly on top
                return GetBasePositionOffset(pathData.homeGoal, tokenIndex, spacing: 0.2f);
            }

            return Vector3.zero;
        }

        private Vector3 GetBasePositionOffset(Transform anchor, int tokenIndex, float spacing = 0.5f)
        {
            if (anchor == null) return Vector3.zero;
            
            // Arrange 4 tokens in a generic 2x2 grid relative to the anchor
            float x = (tokenIndex % 2 == 0) ? -spacing : spacing;
            float z = (tokenIndex < 2) ? -spacing : spacing;
            
            // Respect rotation of the anchor
            return anchor.TransformPoint(new Vector3(x, 0, z));
        }

        // ========================================================================
        // EDITOR VISUALIZATION (GIZMOS)
        // ========================================================================
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (mainTrack == null || mainTrack.Length == 0) return;

            // 1. Draw Main Track Loop
            Gizmos.color = Color.gray;
            for (int i = 0; i < mainTrack.Length; i++)
            {
                if (mainTrack[i] == null) continue;
                Transform next = mainTrack[(i + 1) % mainTrack.Length];
                if (next != null)
                {
                    Gizmos.DrawLine(mainTrack[i].position, next.position);
                    // Draw small tick for direction
                    Vector3 dir = (next.position - mainTrack[i].position).normalized;
                    Gizmos.DrawRay(mainTrack[i].position, (dir - Vector3.right * 0.2f) * 0.5f); 
                }
            }

            // 2. Draw Player Paths
            if (playerPaths == null) return;
            for (int p = 0; p < playerPaths.Length; p++)
            {
                var pp = playerPaths[p];
                Gizmos.color = pp.debugColor;

                // Draw Base -> Start Point
                int startIndex = (p * 13) % mainTrack.Length;
                if (pp.baseAnchor && mainTrack[startIndex])
                {
                    Gizmos.DrawLine(pp.baseAnchor.position, mainTrack[startIndex].position);
                    DrawLabel(pp.baseAnchor.position, $"P{p} Base");
                }

                // Draw Home Stretch Entry (Track -> Stretch 0)
                // The entry point is the tile BEFORE the next player's start.
                // Red (0) enters home after tile 50 (Logic 51).
                int entryIndex = ((p * 13) + 50) % 52;
                
                if (mainTrack[entryIndex] && pp.homeStretch != null && pp.homeStretch.Length > 0 && pp.homeStretch[0])
                {
                    Gizmos.DrawLine(mainTrack[entryIndex].position, pp.homeStretch[0].position);
                }

                // Draw Internal Home Stretch
                if (pp.homeStretch != null)
                {
                    for (int i = 0; i < pp.homeStretch.Length - 1; i++)
                    {
                        if(pp.homeStretch[i] && pp.homeStretch[i+1])
                            Gizmos.DrawLine(pp.homeStretch[i].position, pp.homeStretch[i+1].position);
                    }
                    
                    // Stretch Last -> Goal
                    if (pp.homeStretch.Length > 0 && pp.homeGoal)
                    {
                         Gizmos.DrawLine(pp.homeStretch[^1].position, pp.homeGoal.position);
                    }
                }
            }
        }

        private void DrawLabel(Vector3 pos, string text)
        {
            UnityEditor.Handles.Label(pos + Vector3.up * 0.5f, text);
        }
#endif
    }
}