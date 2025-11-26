using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VirtueSky.Events;
using VirtueSky.Variables;

namespace Pushers
{
    public class GridControl : MonoBehaviour
    {
        public Grid grid;
        public GridScript gridScript;
        public GameObjectEvent selectedGameObject;
        
        public Camera cam;
        public GameObject prefab;
        public GameObject xPusherPrefab;
        public GameObject yPusherPrefab;
        public FloatVariable z;

        public InputActionReference addInput;
        public InputActionReference removeInput;
        public InputActionReference pusherPlace;
        public InputActionReference mousePosition;
        
        private void OnValidate() => grid = GetComponent<Grid>();

        private void OnEnable()
        {
            gridScript.Map = new();
            addInput.action.Enable();
            removeInput.action.Enable();
            pusherPlace.action.Enable();
            mousePosition.action.Enable();

            addInput.action.performed += OnAdd;
            removeInput.action.performed += OnRemove;
            pusherPlace.action.performed += PusherPlaced;
            selectedGameObject.AddListener(OnPrefabSelected);
        }

        private void PusherPlaced(InputAction.CallbackContext obj)
        {
            (Vector2Int cellCoords, Vector3 worldPos) = GetCellInfo();
            if (gridScript.Map.ContainsKey(cellCoords)) return;

            int topY = cellCoords.y;
            for (int y = cellCoords.y + 1; y < cellCoords.y + 100; y++)
            {
                if (gridScript.Map.ContainsKey(new Vector2Int(cellCoords.x, y)))
                {
                    topY = y;
                    break;
                }
            }

            int bottomY = cellCoords.y;
            for (int y = cellCoords.y - 1; y > cellCoords.y - 100; y--)
            {
                if (gridScript.Map.ContainsKey(new Vector2Int(cellCoords.x, y)))
                {
                    bottomY = y;
                    break;
                }
            }

            int rightX = cellCoords.x;
            for (int x = cellCoords.x + 1; x < cellCoords.x + 100; x++)
            {
                if (gridScript.Map.ContainsKey(new Vector2Int(x, cellCoords.y)))
                {
                    rightX = x;
                    break;
                }
            }

            int leftX = cellCoords.x;
            for (int x = cellCoords.x - 1; x > cellCoords.x - 100; x--)
            {
                if (gridScript.Map.ContainsKey(new Vector2Int(x, cellCoords.y)))
                {
                    leftX = x;
                    break;
                }
            }

            int distanceY = Mathf.Abs(topY - bottomY);
            int distanceX = Mathf.Abs(rightX - leftX);

            GameObject pusherToPlace = null;
            Vector3 centerPos = worldPos;
            Vector3 scale = Vector3.one;

            if (distanceY > distanceX && distanceY > 1)
            {
                pusherToPlace = yPusherPrefab;

                Vector3 topWorldPos = grid.GetCellCenterWorld(new Vector3Int(cellCoords.x, topY, 0));
                Vector3 bottomWorldPos = grid.GetCellCenterWorld(new Vector3Int(cellCoords.x, bottomY, 0));
                centerPos = (topWorldPos + bottomWorldPos) / 2f;
                centerPos.z = z.Value;

                float worldDistance = Vector3.Distance(topWorldPos, bottomWorldPos);
                scale = new Vector3(1f, worldDistance, 1f);
            }
            else if (distanceX > 1)
            {
                pusherToPlace = xPusherPrefab;

                Vector3 rightWorldPos = grid.GetCellCenterWorld(new Vector3Int(rightX, cellCoords.y, 0));
                Vector3 leftWorldPos = grid.GetCellCenterWorld(new Vector3Int(leftX, cellCoords.y, 0));
                centerPos = (rightWorldPos + leftWorldPos) / 2f;
                centerPos.z = z.Value;

                float worldDistance = Vector3.Distance(rightWorldPos, leftWorldPos);
                scale = new Vector3(worldDistance, 1f, 1f);
            }
            else
            {
                Debug.Log("No space to place pusher");
                return;
            }

            var o = Instantiate(pusherToPlace, centerPos, Quaternion.identity, transform);
            o.transform.localScale = scale;

            gridScript.Map.TryAdd(cellCoords, o);
        }

        private void OnPrefabSelected(GameObject fab) => prefab = fab;

        private void OnAdd(InputAction.CallbackContext _)
        {
            var (cellCoords, worldPos) = GetCellInfo();
            if (gridScript.Map.ContainsKey(cellCoords)) return;
            Place(prefab, worldPos, cellCoords);
        }

        private void Place(GameObject fab, Vector3 worldPos, Vector2Int cellCoords)
        {
            var o = Instantiate(fab, worldPos, Quaternion.identity, transform);
            gridScript.Map.TryAdd(cellCoords, o);
        }

        private void OnRemove(InputAction.CallbackContext _)
        {
            var (cellCoords, _) = GetCellInfo();
            if (!gridScript.Map.Remove(cellCoords, out var o)) return;
            Destroy(o);
        }

        private (Vector2Int cellCoords, Vector3 worldPos) GetCellInfo()
        {
            var position = mousePosition.action.ReadValue<Vector2>();
            Vector3 screenPos = new Vector3(position.x, position.y, Mathf.Abs(cam.transform.position.z - z.Value));
            var screenToWorldPoint = cam.ScreenToWorldPoint(screenPos);
            Debug.DrawRay(screenToWorldPoint, Vector3.forward * 100, Color.red, 2f);

            var cellPosition = grid.WorldToCell(screenToWorldPoint);
            var cellCoords = new Vector2Int(cellPosition.x, cellPosition.y);

            var cellCenterLocal = grid.GetCellCenterLocal(cellPosition);
            var cellCenterWorld = grid.transform.TransformPoint(cellCenterLocal);
            cellCenterWorld.z = z.Value;

            return (cellCoords, cellCenterWorld);
        }

        private void OnDisable()
        {
            addInput.action.performed -= OnAdd;
            removeInput.action.performed -= OnRemove;
            pusherPlace.action.performed -= PusherPlaced;
            selectedGameObject.RemoveListener(OnPrefabSelected);

            addInput.action.Disable();
            removeInput.action.Disable();
            pusherPlace.action.Disable();
            mousePosition.action.Disable();
        }
    }
}