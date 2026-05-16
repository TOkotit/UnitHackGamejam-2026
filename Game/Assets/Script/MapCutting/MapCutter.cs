using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(InputManager))]
public class MapCutter : MonoBehaviour
{
    [Header("Сетка тайлов")]
    [SerializeField] private Tilemap tilemap;

    [Header("Камера")]
    [SerializeField] private float zoomOutAmount = 3f;
    [SerializeField] private float zoomDuration = 0.4f;

    [Header("Разрезаемые объекты")]
    [SerializeField] private List<GameObject> allCuttableObjects = new();

    private InputManager inputManager;
    private Camera mainCamera;
    private float defaultZoom;
    private Grid grid;

    private bool isHorizontal = false;
    private Vector2 currentMouseWorldPos;

    private void Start()
    {
        inputManager = GetComponent<InputManager>();
        mainCamera = Camera.main;
        defaultZoom = mainCamera.orthographicSize;

        if (tilemap)
            grid = tilemap.layoutGrid;

        var gameInput = inputManager.GetGameInput();
        if (gameInput != null)
        {
            gameInput.UI.Cut.performed += OnCutPerformed;
            gameInput.UI.Rotate.performed += OnRotatePerformed;
            gameInput.UI.Point.performed += OnPointPerformed;
            gameInput.UI.Point.canceled += OnPointCanceled;
            gameInput.Gameplay.CutMenu.performed += OnCutMenuPerformed;
        }

        inputManager.SetGameplay();
    }

    private void OnDestroy()
    {
        var gameInput = inputManager?.GetGameInput();
        if (gameInput != null)
        {
            gameInput.UI.Cut.performed -= OnCutPerformed;
            gameInput.UI.Rotate.performed -= OnRotatePerformed;
            gameInput.UI.Point.performed -= OnPointPerformed;
            gameInput.UI.Point.canceled -= OnPointCanceled;
            gameInput.Gameplay.CutMenu.performed -= OnCutMenuPerformed;
        }
    }

    private void OnCutMenuPerformed(InputAction.CallbackContext ctx)
    {
        if (inputManager.IsUIMode)
            inputManager.SetGameplay();
        else
            inputManager.SetUI();
    }

    private void OnCutPerformed(InputAction.CallbackContext ctx)
    {
        if (!inputManager.IsUIMode) return;
        if (currentMouseWorldPos == Vector2.zero) return;
        PerformCut(currentMouseWorldPos);
    }

    private void OnRotatePerformed(InputAction.CallbackContext ctx)
    {
        if (!inputManager.IsUIMode) return;
        isHorizontal = !isHorizontal;
    }

    private void OnPointPerformed(InputAction.CallbackContext ctx)
    {
        UpdateMousePosition(ctx.ReadValue<Vector2>());
    }

    private void OnPointCanceled(InputAction.CallbackContext ctx)
    {
        UpdateMousePosition(Vector2.zero);
    }

    private void UpdateMousePosition(Vector2 screenPos)
    {
        if (screenPos == Vector2.zero) return;
        currentMouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
    }

    private void PerformCut(Vector2 mouseWorldPos)
    {
        if (!tilemap || !grid) return;

        float cutCoordWorld;
        if (isHorizontal)
            cutCoordWorld = SnapToGrid(mouseWorldPos.y, grid.cellSize.y, grid, true);
        else
            cutCoordWorld = SnapToGrid(mouseWorldPos.x, grid.cellSize.x, grid, false);
        
        var cutCell = grid.WorldToCell(isHorizontal
            ? new Vector3(0, cutCoordWorld, 0)
            : new Vector3(cutCoordWorld, 0, 0));

        SwapTilemapParts(cutCell, isHorizontal);

        var bounds = tilemap.cellBounds;
        var minWorld = tilemap.CellToWorld(bounds.min);
        var maxWorld = tilemap.CellToWorld(bounds.max) + grid.cellSize;

        foreach (var obj in allCuttableObjects)
        {
            if (!obj) continue;
            var pos = obj.transform.position;

            if (isHorizontal)
            {
                var aSize = cutCoordWorld - minWorld.y;
                var bSize = maxWorld.y - cutCoordWorld;
                if (aSize <= 0 || bSize <= 0) continue;
                var offset = pos.y - minWorld.y;
                if (offset < aSize)
                    pos.y = pos.y + bSize;          
                else
                    pos.y = pos.y - aSize;          
            }
            else
            {
                float aSize = cutCoordWorld - minWorld.x;
                float bSize = maxWorld.x - cutCoordWorld;
                if (aSize <= 0 || bSize <= 0) continue;
                float offset = pos.x - minWorld.x;
                if (offset < aSize)
                    pos.x = pos.x + bSize;          
                else
                    pos.x = pos.x - aSize;          
            }
            obj.transform.position = pos;
        }

        StopAllCoroutines();
        StartCoroutine(ZoomEffect());
        inputManager.SetGameplay();
    }

    private void SwapTilemapParts(Vector3Int cutCell, bool horizontal)
    {
        var bounds = tilemap.cellBounds;
        if (bounds.size.x <= 0 || bounds.size.y <= 0) return;

        Dictionary<Vector3Int, TileBase> originalTiles = new();
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            var tile = tilemap.GetTile(pos);
            if (tile)
                originalTiles[pos] = tile;
        }
        tilemap.ClearAllTiles();

        if (horizontal)
        {
            var minY = bounds.yMin;
            var maxY = bounds.yMax - 1;
            var cutY = cutCell.y;
            if (cutY <= minY || cutY > maxY + 1) return;

            var aSize = cutY - minY;       
            var bSize = maxY - cutY + 1;   
            if (aSize <= 0 || bSize <= 0) return;

            foreach (var kvp in originalTiles)
            {
                var oldPos = kvp.Key;
                var newPos = oldPos;
                if (oldPos.y < cutY)
                {
                    newPos.y = oldPos.y + bSize;
                }
                else
                {
                    
                    newPos.y = oldPos.y - aSize;
                }
                if (bounds.Contains(newPos))
                    tilemap.SetTile(newPos, kvp.Value);
            }
        }
        else 
        {
            var minX = bounds.xMin;
            var maxX = bounds.xMax - 1;
            var cutX = cutCell.x;
            if (cutX <= minX || cutX > maxX + 1) return;

            var aSize = cutX - minX;       
            var bSize = maxX - cutX + 1;   
            if (aSize <= 0 || bSize <= 0) return;

            foreach (var kvp in originalTiles)
            {
                Vector3Int oldPos = kvp.Key;
                Vector3Int newPos = oldPos;
                if (oldPos.x < cutX)
                {
                    newPos.x = oldPos.x + bSize;
                }
                else
                {
                    newPos.x = oldPos.x - aSize;
                }
                if (bounds.Contains(newPos))
                    tilemap.SetTile(newPos, kvp.Value);
            }
        }
    }

    private float SnapToGrid(float worldCoord, float tileSize, Grid grid, bool isHorizontal)
    {
        Vector3 worldPoint = isHorizontal ? new Vector3(0, worldCoord, 0) : new Vector3(worldCoord, 0, 0);
        Vector3Int cellPos = grid.WorldToCell(worldPoint);
        Vector3 cellCenterWorld = grid.CellToWorld(cellPos);
        float halfTile = tileSize * 0.5f;
        if (isHorizontal)
        {
            float cellCenterY = cellCenterWorld.y;
            if (worldCoord < cellCenterY)
                return cellCenterY - halfTile;
            else
                return cellCenterY + halfTile;
        }
        else
        {
            float cellCenterX = cellCenterWorld.x;
            if (worldCoord < cellCenterX)
                return cellCenterX - halfTile;
            else
                return cellCenterX + halfTile;
        }
    }

    private IEnumerator ZoomEffect()
    {
        float elapsed = 0f;
        float startZoom = mainCamera.orthographicSize;
        float targetZoom = startZoom + zoomOutAmount;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, elapsed / zoomDuration);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            mainCamera.orthographicSize = Mathf.Lerp(targetZoom, startZoom, elapsed / zoomDuration);
            yield return null;
        }
        mainCamera.orthographicSize = startZoom;
    }
}