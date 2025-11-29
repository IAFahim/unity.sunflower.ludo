// FILE: Assets/Ludos/Client/Visuals/LudoBoardRenderer.cs
using System.Collections;
using System.Collections.Generic;
using Ludos.Core;
using UnityEngine;

namespace Ludos.Client.Visuals
{
    public class LudoBoardRenderer : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private LudoGameManager manager;
        [SerializeField] private LudoBoardLayout layout; // Link the script above

        [Header("Prefabs")]
        [SerializeField] private GameObject tokenPrefab;
        [SerializeField] private Color[] playerColors = { Color.red, Color.green, Color.yellow, Color.blue };

        [Header("Animation")]
        [SerializeField] private float moveDuration = 0.3f;
        [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // State Tracking
        // Key: "P{playerIndex}_T{tokenIndex}"
        private Dictionary<string, Transform> _spawnedTokens = new Dictionary<string, Transform>();
        private Dictionary<string, Coroutine> _activeAnimations = new Dictionary<string, Coroutine>();

        private void Start()
        {
            // Validate Layout
            if (layout == null)
            {
                Debug.LogError("[LudoRenderer] Layout not assigned!");
                enabled = false;
                return;
            }

            InitializeBoard();
        }
        
        private void InitializeBoard()
        {
            _spawnedTokens.Clear();
            
            // Clean up existing children if any (for development iteration)
            foreach(Transform child in layout.transform) {
                 // Don't destroy the layout waypoints, only tokens if you parent them here
                 // Better to parent tokens to a specific container
            }

            // Spawn 16 tokens (4 players * 4 tokens)
            for (int p = 0; p < 4; p++)
            {
                for (int t = 0; t < 4; t++)
                {
                    // Logic 0 = Base
                    Vector3 startPos = layout.GetWorldPosition(p, 0, t);
                    
                    var go = Instantiate(tokenPrefab, startPos, Quaternion.identity);
                    go.name = $"P{p}_T{t}";
                    
                    // Color
                    var rend = go.GetComponentInChildren<Renderer>();
                    if (rend && p < playerColors.Length) rend.material.color = playerColors[p];

                    // Input Handler (Make sure you have this script created from previous step)

                    _spawnedTokens[go.name] = go.transform;
                }
            }
        }

        private void RenderState(LudoState state)
        {
            for (int p = 0; p < 4; p++)
            {
                for (int t = 0; t < 4; t++)
                {
                    byte logicPos = state.GetTokenPos(p, t);
                    string tokenKey = $"P{p}_T{t}";

                    if (!_spawnedTokens.TryGetValue(tokenKey, out Transform token)) continue;

                    // Calculate Target
                    Vector3 targetPos = layout.GetWorldPosition(p, logicPos, t);
                    
                    // Check if actually moving to save performance
                    if (Vector3.SqrMagnitude(token.position - targetPos) < 0.001f) continue;

                    // Stop old animation
                    if (_activeAnimations.TryGetValue(tokenKey, out Coroutine activeRoutine) && activeRoutine != null)
                    {
                        StopCoroutine(activeRoutine);
                    }
                    
                    // Start new animation
                    _activeAnimations[tokenKey] = StartCoroutine(MoveToken(token, targetPos));
                }
            }
        }

        private IEnumerator MoveToken(Transform token, Vector3 targetPosition)
        {
            Vector3 startPosition = token.position;
            float elapsed = 0f;

            // Optional: Add a "Jump" arc effect
            bool isLongJump = Vector3.Distance(startPosition, targetPosition) > 2.0f;
            float jumpHeight = isLongJump ? 1.0f : 0.0f;

            while (elapsed < moveDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / moveDuration);
                float curveValue = moveCurve.Evaluate(t);
                
                Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, curveValue);
                
                // Add jump arc
                if (jumpHeight > 0)
                    currentPos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

                token.position = currentPos;
                
                yield return null;
            }

            token.position = targetPosition;
            _activeAnimations.Remove(token.name);
        }
    }
}