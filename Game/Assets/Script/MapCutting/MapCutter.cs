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
    [SerializeField] private GameObject background;
    
    [Header("Разрезаемые объекты")]
    [SerializeField] private List<GameObject> allCuttableObjects = new();

    [SerializeField] private int availableCuts;

    [Header("Подсветка линии разреза")]
    [SerializeField] private LineRenderer cutLine;
    [SerializeField] private Color lineColor = Color.red;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private InputManager inputManager;

    
    
    private Camera mainCamera;
    private float defaultZoom;
    private Grid grid;

    private bool isHorizontal = false;
    private Vector2 currentMouseWorldPos;

    private Coroutine zoomCoroutine;
    private Coroutine backgroundCoroutine;
    private void Start()
    {
        mainCamera = Camera.main;
        defaultZoom = mainCamera.orthographicSize;

        if (tilemap)
        {
            grid = tilemap.layoutGrid;
            tilemap.CompressBounds();
        }

        if (!cutLine)
        {
            var lineObj = new GameObject("CutLine");
            lineObj.transform.SetParent(transform);
            cutLine = lineObj.AddComponent<LineRenderer>();
        }
        cutLine.startWidth = lineWidth;
        cutLine.endWidth = lineWidth;
        cutLine.material = new Material(Shader.Find("Sprites/Default"));
        cutLine.startColor = lineColor;
        cutLine.endColor = lineColor;
        cutLine.positionCount = 2;
        cutLine.enabled = false;

        var gameInput = inputManager.GetGameInput();
        if (gameInput != null)
        {
            gameInput.UI.Cut.performed += OnCutPerformed;
            gameInput.UI.Rotate.performed += OnRotatePerformed;
            gameInput.UI.Point.performed += OnPointPerformed;
            gameInput.UI.Point.canceled += OnPointCanceled;
            gameInput.UI.CloseCutMenu.performed += OnCloseCutMenu;
            gameInput.Gameplay.CutMenu.performed += OnCutMenuPerformed;
        }

        inputManager.SetGameplay();
        Time.timeScale = 1f;
        mainCamera.orthographicSize = defaultZoom;
    }

    private void Update()
    {
        if (inputManager.IsUIMode && tilemap && grid)
        {
            UpdateCutLine();
        }
        else if (cutLine && cutLine.enabled)
        {
            cutLine.enabled = false;
        }
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
            gameInput.UI.CloseCutMenu.performed -= OnCloseCutMenu;

            gameInput.Gameplay.CutMenu.performed -= OnCutMenuPerformed;
        }
        Time.timeScale = 1f;
    }

    private void OnCutMenuPerformed(InputAction.CallbackContext ctx)
    {
        if (inputManager.IsUIMode || availableCuts <= 0)
        {
            inputManager.SetGameplay();
            ExitCutMode();
        }
        else
        {
            if (Mouse.current != null)
            {
                var screenPos = Mouse.current.position.ReadValue();
                currentMouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
            }
            inputManager.SetUI();
            EnterCutMode();
        }
    }

    private void OnCloseCutMenu(InputAction.CallbackContext context)
    {
        inputManager.SetGameplay();
        ExitCutMode();
    }
    private void EnterCutMode()
    {
        Time.timeScale = 0f;
        if (backgroundCoroutine != null) StopCoroutine(backgroundCoroutine);
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(AnimateCameraZoom(defaultZoom + zoomOutAmount));
        if (background == null) return;
        backgroundCoroutine = StartCoroutine(AnimateBackground(2 * background.transform.localScale));
        
    }

    private void ExitCutMode()
    {
        Time.timeScale = 1f;
        if (backgroundCoroutine != null) StopCoroutine(backgroundCoroutine);
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(AnimateCameraZoom(defaultZoom));
        if (background == null) return;
        backgroundCoroutine = StartCoroutine(AnimateBackground(background.transform.localScale / 2));
    }

    private IEnumerator AnimateCameraZoom(float targetZoom)
    {
        float startZoom = mainCamera.orthographicSize;
        float elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / zoomDuration;
            mainCamera.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);
            yield return null;
        }
        mainCamera.orthographicSize = targetZoom;
    }

    private IEnumerator AnimateBackground(Vector3 scale)
    {
        
        var startZoom = background.transform.localScale;
        var elapsed = 0f;
        while (elapsed < zoomDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / zoomDuration;
            background.transform.localScale = Vector3.Lerp(startZoom, scale, t);
            yield return null;
        }
        background.transform.localScale = scale;
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

    private void UpdateCutLine()
    {
        if (currentMouseWorldPos == Vector2.zero) return;

        if (!TryGetCutLineData(currentMouseWorldPos, isHorizontal, out var cutCoordWorld, out var cutCell))
        {
            cutLine.enabled = false;
            return;
        }

        var bounds = tilemap.cellBounds;
        if (isHorizontal)
        {
            var minXWorld = tilemap.CellToWorld(new Vector3Int(bounds.xMin, 0, 0)).x;
            var maxXWorld = tilemap.CellToWorld(new Vector3Int(bounds.xMax, 0, 0)).x;
            cutLine.SetPosition(0, new Vector3(minXWorld, cutCoordWorld, 0));
            cutLine.SetPosition(1, new Vector3(maxXWorld, cutCoordWorld, 0));
        }
        else
        {
            var minYWorld = tilemap.CellToWorld(new Vector3Int(0, bounds.yMin, 0)).y;
            var maxYWorld = tilemap.CellToWorld(new Vector3Int(0, bounds.yMax, 0)).y;
            cutLine.SetPosition(0, new Vector3(cutCoordWorld, minYWorld, 0));
            cutLine.SetPosition(1, new Vector3(cutCoordWorld, maxYWorld, 0));
        }

        if (!cutLine.enabled)
            cutLine.enabled = true;
    }

    private void PerformCut(Vector2 mouseWorldPos)
    {
        if (!tilemap || !grid) return;

        var bounds = tilemap.cellBounds;
        if (bounds.size.x <= 0 || bounds.size.y <= 0) return;

        var cellPosAtMouse = grid.WorldToCell(mouseWorldPos);
        if (!bounds.Contains(cellPosAtMouse)) return;

        if (!TryGetCutLineData(mouseWorldPos, isHorizontal, out var cutCoordWorld, out var cutCell))
            return;
        availableCuts--;

        SwapTilemapParts(cutCell, isHorizontal);

        var cellSize = grid.cellSize;
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
                pos.y += (pos.y - minWorld.y < aSize) ? bSize : -aSize;
            }
            else
            {
                var aSize = cutCoordWorld - minWorld.x;
                var bSize = maxWorld.x - cutCoordWorld;
                if (aSize <= 0 || bSize <= 0) continue;
                pos.x += (pos.x - minWorld.x < aSize) ? bSize : -aSize;
            }
            obj.transform.position = pos;
        }

        inputManager.SetGameplay();
        ExitCutMode();
    }

    private bool TryGetCutLineData(Vector3 worldPos, bool horizontal, out float cutCoordWorld, out Vector3Int cutCell)
    {
        cutCoordWorld = 0;
        cutCell = Vector3Int.zero;

        var bounds = tilemap.cellBounds;
        if (bounds.size.x <= 0 || bounds.size.y <= 0) return false;

        var cellSize = grid.cellSize;

        if (horizontal)
        {
            var raw = worldPos.y;
            cutCoordWorld = SnapToGrid(raw, cellSize.y, true);
            cutCell = grid.WorldToCell(new Vector3(0, cutCoordWorld, 0));

            var cellTop = grid.CellToWorld(cutCell).y + cellSize.y;
            if (Mathf.Approximately(cutCoordWorld, cellTop))
                cutCell.y += 1;

            cutCell.y = Mathf.Clamp(cutCell.y, bounds.yMin + 1, bounds.yMax);
            cutCoordWorld = grid.CellToWorld(new Vector3Int(0, cutCell.y, 0)).y;
        }
        else
        {
            var raw = worldPos.x;
            cutCoordWorld = SnapToGrid(raw, cellSize.x, false);
            cutCell = grid.WorldToCell(new Vector3(cutCoordWorld, 0, 0));

            var cellRight = grid.CellToWorld(cutCell).x + cellSize.x;
            if (Mathf.Approximately(cutCoordWorld, cellRight))
                cutCell.x += 1;

            cutCell.x = Mathf.Clamp(cutCell.x, bounds.xMin + 1, bounds.xMax);
            cutCoordWorld = grid.CellToWorld(new Vector3Int(cutCell.x, 0, 0)).x;
        }

        return true;
    }

    private void SwapTilemapParts(Vector3Int cutCell, bool horizontal)
    {
        var bounds = tilemap.cellBounds;
        if (bounds.size.x <= 0 || bounds.size.y <= 0) return;

        if (horizontal)
        {
            var minY = bounds.yMin;
            var maxY = bounds.yMax - 1;
            var cutY = cutCell.y;
            if (cutY <= minY || cutY > maxY + 1) return;
            var aSize = cutY - minY;
            var bSize = maxY - cutY + 1;
            if (aSize <= 0 || bSize <= 0) return;
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
        }

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
            var aSize = cutY - minY;
            var bSize = maxY - cutY + 1;

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
            var aSize = cutX - minX;
            var bSize = maxX - cutX + 1;

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

    private float SnapToGrid(float worldCoord, float tileSize, bool isHorizontal)
    {
        var worldPoint = isHorizontal ? new Vector3(0, worldCoord, 0) : new Vector3(worldCoord, 0, 0);
        var cellPos = grid.WorldToCell(worldPoint);
        var cellCenterWorld = grid.CellToWorld(cellPos);
        var halfTile = tileSize * 0.5f;
        if (isHorizontal)
        {
            var cellCenterY = cellCenterWorld.y;
            return worldCoord < cellCenterY ? cellCenterY - halfTile : cellCenterY + halfTile;
        }
        else
        {
            var cellCenterX = cellCenterWorld.x;
            return worldCoord < cellCenterX ? cellCenterX - halfTile : cellCenterX + halfTile;
        }
    }
}