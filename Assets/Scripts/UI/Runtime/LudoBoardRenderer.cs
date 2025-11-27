// // FILE: Assets/Ludos/Client/Visuals/LudoBoardRenderer.cs
// using System.Collections.Generic;
// using UnityEngine;
// using DG.Tweening; // Highly recommended for movement (DOTween)
//
// namespace Ludos.Client.Visuals
// {
//     public class LudoBoardRenderer : MonoBehaviour
//     {
//         [Header("Dependencies")]
//         [SerializeField] private LudoGameManager manager;
//
//         [Header("Board Config")]
//         // Assign 58 transforms (0=Start... 57=Home)
//         // Note: Logic defines 0 as Base, 1 as Start. 
//         // You need a mapping strategy.
//         [SerializeField] private Transform[] pathWaypoints; 
//         [SerializeField] private Transform[] baseAnchors; // 4 Parents (Red, Green, Yellow, Blue)
//
//         [Header("Prefabs")]
//         [SerializeField] private GameObject tokenPrefab;
//         [SerializeField] private Color[] playerColors = { Color.red, Color.green, Color.yellow, Color.blue };
//
//         // State Tracking
//         private Dictionary<string, Transform> _spawnedTokens = new Dictionary<string, Transform>();
//
//         private void Start()
//         {
//             manager.StateUpdated += RenderState;
//             InitializeBoard();
//         }
//
//         private void OnDestroy()
//         {
//             if(manager) manager.StateUpdated -= RenderState;
//         }
//
//         private void InitializeBoard()
//         {
//             // Spawn 16 tokens (4 players * 4 tokens)
//             for (int p = 0; p < 4; p++)
//             {
//                 for (int t = 0; t < 4; t++)
//                 {
//                     var go = Instantiate(tokenPrefab, baseAnchors[p]);
//                     go.name = $"P{p}_T{t}";
//                     
//                     // Color
//                     var rend = go.GetComponentInChildren<Renderer>();
//                     if(rend) rend.material.color = playerColors[p];
//
//                     // Input Handler
//                     var input = go.AddComponent<LudoTokenInput>();
//                     input.Setup(manager, t);
//
//                     _spawnedTokens[go.name] = go.transform;
//                     
//                     // Initial Pos: Base
//                     go.transform.localPosition = GetBasePosition(t);
//                 }
//             }
//         }
//
//         private void RenderState(LudoState state)
//         {
//             for (int p = 0; p < 4; p++)
//             {
//                 for (int t = 0; t < 4; t++)
//                 {
//                     byte logicPos = state.GetTokenPos(p, t);
//                     Transform token = _spawnedTokens[$"P{p}_T{t}"];
//                     
//                     Vector3 targetPos = CalculateWorldPosition(p, logicPos, t);
//                     
//                     // Smooth Move
//                     token.DOMove(targetPos, 0.3f);
//                 }
//             }
//         }
//
//         private Vector3 CalculateWorldPosition(int playerIndex, byte logicPos, int tokenIndex)
//         {
//             // 1. In Base
//             if (logicPos == LudoConstants.PosBase)
//             {
//                 return baseAnchors[playerIndex].TransformPoint(GetBasePosition(tokenIndex));
//             }
//
//             // 2. On Track
//             // We need to convert Relative Logic Pos to Absolute Board Index
//             // This math depends heavily on your specific board Waypoint setup.
//             // Assuming Waypoints[] is a loop of 52 tiles.
//             
//             // Offset: Red starts at index 0. Green at 13. Yellow at 26. Blue at 39.
//             int startOffset = playerIndex * 13; 
//             
//             if (logicPos <= LudoConstants.TrackLength) // 1-52
//             {
//                 // -1 because logic 1 is board index 0
//                 int absIndex = (startOffset + (logicPos - 1)) % 52; 
//                 return pathWaypoints[absIndex].position;
//             }
//             
//             // 3. Home Stretch (53-57)
//             // You likely have special waypoints for home runs, or math it out.
//             // For simplicity, returning Vector3.zero + Up * logicPos
//             return Vector3.zero; 
//         }
//
//         private Vector3 GetBasePosition(int tokenIndex)
//         {
//             // Arranges 4 tokens in a 2x2 grid inside the base anchor
//             float spacing = 0.5f;
//             float x = (tokenIndex % 2 == 0) ? -spacing : spacing;
//             float z = (tokenIndex < 2) ? -spacing : spacing;
//             return new Vector3(x, 0, z);
//         }
//     }
// }