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

    [SerializeField] private int availableCuts;

    [SerializeField] private InputManager inputManager;
    private Camera mainCamera;
    private float defaultZoom;
    private Grid grid;

    private bool isHorizontal = false;
    private Vector2 currentMouseWorldPos;

    private void Start()
    {
        mainCamera = Camera.main;
        defaultZoom = mainCamera.orthographicSize;

        if (tilemap)
        {
            grid = tilemap.layoutGrid;
            tilemap.CompressBounds(); 
        }

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
        Debug.Log("<color=green> ASSSSSSS");
        if (inputManager.IsUIMode || availableCuts <= 0)
            inputManager.SetGameplay();
        else
        {
            Debug.Log("<color=green> WWWWWESSDSD");
            availableCuts--; 
            inputManager.SetUI();
        }
            
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

        var bounds = tilemap.cellBounds;
        if (bounds.size.x <= 0 || bounds.size.y <= 0) return;

        var cellSize = grid.cellSize;
        float cutCoordWorld;
        Vector3Int cutCell;

        if (isHorizontal)
        {
            cutCoordWorld = SnapToGrid(mouseWorldPos.y, cellSize.y, grid, true);
            cutCell = grid.WorldToCell(new Vector3(0, cutCoordWorld, 0));
            
            var cellTop = grid.CellToWorld(cutCell).y + cellSize.y;
            if (Mathf.Approximately(cutCoordWorld, cellTop))
                cutCell.y += 1;
            
            cutCell.y = Mathf.Clamp(cutCell.y, bounds.yMin + 1, bounds.yMax);
            cutCoordWorld = grid.CellToWorld(new Vector3Int(0, cutCell.y, 0)).y;
        }
        else
        {
            cutCoordWorld = SnapToGrid(mouseWorldPos.x, cellSize.x, grid, false);
            cutCell = grid.WorldToCell(new Vector3(cutCoordWorld, 0, 0));
            
            var cellRight = grid.CellToWorld(cutCell).x + cellSize.x;
            if (Mathf.Approximately(cutCoordWorld, cellRight))
                cutCell.x += 1;
            
            cutCell.x = Mathf.Clamp(cutCell.x, bounds.xMin + 1, bounds.xMax);
            cutCoordWorld = grid.CellToWorld(new Vector3Int(cutCell.x, 0, 0)).x;
        }

        SwapTilemapParts(cutCell, isHorizontal);

        var minWorld = tilemap.CellToWorld(bounds.min);
        var maxWorld = tilemap.CellToWorld(bounds.max) + cellSize;

        foreach (var obj in allCuttableObjects)
        {
            if (!obj) continue;
            var pos = obj.transform.position;

            if (isHorizontal)
            {
                var aSize = cutCoordWorld - minWorld.y;
                var bSize = maxWorld.y - cutCoordWorld;
                if (aSize <= 0 || bSize <= 0) continue;
                if (pos.y - minWorld.y < aSize)
                    pos.y += bSize;
                else
                    pos.y -= aSize;
            }
            else
            {
                var aSize = cutCoordWorld - minWorld.x;
                var bSize = maxWorld.x - cutCoordWorld;
                if (aSize <= 0 || bSize <= 0) continue;
                if (pos.x - minWorld.x < aSize)
                    pos.x += bSize;
                else
                    pos.x -= aSize;
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

        var originalTiles = new Dictionary<Vector3Int, TileBase>();
        foreach (var pos in bounds.allPositionsWithin)
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
                    newPos.y = oldPos.y + bSize;
                else
                    newPos.y = oldPos.y - aSize;
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
                var oldPos = kvp.Key;
                var newPos = oldPos;
                if (oldPos.x < cutX)
                    newPos.x = oldPos.x + bSize;
                else
                    newPos.x = oldPos.x - aSize;
                if (bounds.Contains(newPos))
                    tilemap.SetTile(newPos, kvp.Value);
            }
        }
    }

    private float SnapToGrid(float worldCoord, float tileSize, Grid grid, bool isHorizontal)
    {
        var worldPoint = isHorizontal ? new Vector3(0, worldCoord, 0) : new Vector3(worldCoord, 0, 0);
        var cellPos = grid.WorldToCell(worldPoint);
        var cellCenterWorld = grid.CellToWorld(cellPos);
        var halfTile = tileSize * 0.5f;
        if (isHorizontal)
        {
            var cellCenterY = cellCenterWorld.y;
            if (worldCoord < cellCenterY)
                return cellCenterY - halfTile;
            else
                return cellCenterY + halfTile;
        }
        else
        {
            var cellCenterX = cellCenterWorld.x;
            if (worldCoord < cellCenterX)
                return cellCenterX - halfTile;
            else
                return cellCenterX + halfTile;
        }
    }

    private IEnumerator ZoomEffect()
    {
        var elapsed = 0f;
        var startZoom = mainCamera.orthographicSize;
        var targetZoom = startZoom + zoomOutAmount;
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