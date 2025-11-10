using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using System;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
    public float tileSize = 2f;
    public float tilePadding = 0.1f;
    public Vector3 origin = Vector3.zero;
    [SerializeField] private int visualTilePoolSize = 500;
    [SerializeField] private int highlightPoolSize = 100;
    private Dictionary<Vector2Int, GridTile> tileGrid = new();

    [Space(10)]
    [Header("References")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject highlightPrefab;
    [SerializeField] private GameObject tileParent;
    [SerializeField] private GameObject highlightParent;
    [SerializeField] private GameObject entitySpawnParent;
    [SerializeField] private LevelTileData levelTileData;
    [SerializeField] private GridTileVisual[] visualTilePool;
    [SerializeField] private GridTileHighlight[] highlightPool;

    // Vector2Int Cardial directions
    private static readonly Vector2Int[] directions = new Vector2Int[]
        {Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
    private int previousActiveTileCount;
    private int previousActiveHighlightCount;

    // Colors for visual tiles
    public readonly Color openColor = new Color(1, 1, 1, 0.5f);
    public readonly Color occupiedColor = new Color(1, 0, 0, 0.7f);
    public readonly Color hoverColor = new Color(1, 1, 0, 0.7f);
    public readonly Color higlightColor = new Color(0.9245283f, 0.5480992f, 0.1f, 0.7f);

    // References to delegates for visualTile events
    private Action<GridTileVisual> hoverH, exitH, clickH;

    // Editor only variables
#if UNITY_EDITOR
    [Header("Editor Tiles")]
    private Dictionary<Vector2Int, GridTileVisualEditor> editorTiles = new Dictionary<Vector2Int, GridTileVisualEditor>();
    [SerializeField] private GameObject editorTileParent;
    public GameObject editorTilePrefab;
#endif

    // Create instance and create the data grid
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GenerateGrid();

        // Generate 2D tileGrid with variable setttings
        void GenerateGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2Int pos = new(x, y);
                    GridTile tile = new(pos); // logic data
                    tileGrid[pos] = tile;
                }
            }

            // Load level's TileData and set occupied tiles according to the data
            foreach (TileData tileData in levelTileData.Tiles)
            {
                if (tileGrid.ContainsKey(tileData.Coordinates))
                {
                    if (tileData.Occupied)
                    {
                        tileGrid[tileData.Coordinates].Occupied = true;
                    }
                }
            }
        }
    }

    // Spawn entity on tileGrid and set it's world position
    public GameObject SpawnEntityOnTile(GameObject prefab, Vector2Int spawnPosition, bool joinCombat) 
    {
        // Don't spawn entities on tiles that occupied or have occupier on them
        GridTile tile = tileGrid[spawnPosition];
        if (tile.Occupier != null || tile.Occupied == true)
            return null;

        // Spawn prefab, set it's parent and set the world position based off off spawnPosition
        GameObject spawned = Instantiate(prefab, GridToWorld(spawnPosition), Quaternion.identity);
        spawned.transform.SetParent(entitySpawnParent.transform);
        EnemyCombat enemy = spawned.GetComponent<EnemyCombat>();
        if (enemy != null)
            enemy.gridPosition = spawnPosition;

        // Update tileGrid
        tile.Occupier = enemy;

        // Add spawned enemy to combat if desired
        if (joinCombat)
            CombatManager.Instance.AddParticipant(enemy);
        return spawned;
    }

    // Destroy entity gameObject and release tile it was occupying
    public bool DestroyEntityOnTile(CombatParticipant participant, Vector2Int participantTile) 
    {
        // Don't clear already empty tiles
        if (tileGrid[participantTile].Occupier == null)
            return false;

        // Remove occupier reference on tile and destroy the gameObject of occupier
        tileGrid[participantTile].Occupier = null;
        Destroy(participant.gameObject);
        return true;
    }

    // Move entity from tile to another
    public void MoveToTile(CombatParticipant participant, Vector2Int movePosition) 
    {
        // Update tiles occupiers and participant's gridPosition
        Vector2Int currentPoint = participant.gridPosition;
        tileGrid[currentPoint].Occupier = null;
        tileGrid[movePosition].Occupier = participant;
        participant.gridPosition = movePosition;
        participant.gameObject.transform.position = GridToWorld(participant.gridPosition);
    }

    // Get single GridTile from Dictionary
    public GridTile GetTileAtPosition(Vector2Int pos)
    {
        tileGrid.TryGetValue(pos, out var tile);
        return tile;
    }

    // BFS-algorithm: Get list of movable tiles within Move Range
    public List<GridTile> GetMovableTiles(Vector2Int startPosition, int moveRange)
    {
        List<GridTile> tiles = new List<GridTile>();
        HashSet<Vector2Int> visited = new(); // HashSet to list what tiles have been visited
        Queue<(Vector2Int pos, int dist)> frontier = new(); // Queue to run the loop, contains coordinates and distance from startPosition

        // Add startPosition to visited and frontier to start the loop
        visited.Add(startPosition);
        frontier.Enqueue((startPosition, 0));

        while (frontier.Count > 0)
        {
            var (current, dist) = frontier.Dequeue();
            GridTile currentTile = tileGrid[current];
            
            // Add free and occupied tiles only to list, no tiles with occupiers
            if (dist > 0)
                if(tileGrid[current].Occupier == null)
                    tiles.Add(currentTile);

            // No adding tiles over the move range limit
            if (dist >= moveRange)
                continue;

            // Don't check adjacent tiles, if current tile is occupied
            if (currentTile.Occupied == true)
                continue;

            // Check all cardinal tiles next to current tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                // If out of bounds OR If already visited; continue
                if (!tileGrid.ContainsKey(next) || visited.Contains(next))
                    continue;

                // Don't get tiles with occupiers on them
                if (tileGrid[next].Occupier != null)
                    continue;

                // Add new tile to visited list
                visited.Add(next);

                // Add next tile to be checked to Queue
                frontier.Enqueue((next, dist + 1));
            }
        }
        return tiles;
    }

    // BFS-algorithm: Get list of all tiles within Ability Range
    public List<GridTile> GetTilesInRange(Vector2Int startPosition, int abilityRange, bool ignoreStart, bool throughObstacles)
    {
        List<GridTile> tiles = new List<GridTile>();
        HashSet<Vector2Int> visited = new(); // HashSet to list what tiles have been visited
        Queue<(Vector2Int pos, int dist)> frontier = new(); // Queue to run the loop, contains coordinates and distance from startPosition

        // Add startPosition to visited and frontier to start the loop
        visited.Add(startPosition);
        frontier.Enqueue((startPosition, 0));

        while (frontier.Count > 0)
        {
            var (current, dist) = frontier.Dequeue();

            // Add start tile to list only if desired
            if (dist > 0 || !ignoreStart)
                tiles.Add(tileGrid[current]);

            // No adding tiles over the ability range limit
            if (dist >= abilityRange)
                continue;

            // Check all cardinal tiles next to current tile
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;

                // If out of bounds OR If already visited; continue
                if (!tileGrid.ContainsKey(next) || visited.Contains(next))
                    continue;

                // Don't check adjaceen tiles around occupied tiles if ability doesn't allow it
                if (!throughObstacles && tileGrid[current].Occupied)
                    continue;

                // Add new tile to visited list
                visited.Add(next);

                // Add next tile to be checked to Queue
                frontier.Enqueue((next, dist + 1));
            }
        }
        return tiles;
    }

    // AI targeting: Check if any tile at coordinates and inside passed ability shape contain entity of target team
    public bool IsTeamOnShapeTiles(Vector2Int caster, Vector2Int targetPosition, List<Vector2Int> abilityShape, bool rotateShape, Team team)
    {
        // Create copy of shape to test if player is targeted in shape
        List<Vector2Int> copyShape = new List<Vector2Int>(abilityShape);

        // Rotate if needed and move shape to target location
        if (rotateShape)
            RotateShape(copyShape, caster, targetPosition);
        MoveShape(copyShape, targetPosition);

        // Check every grid position in shape for player
        foreach(Vector2Int tilePosition in copyShape) 
        {
            if (tileGrid.ContainsKey(tilePosition))
            {
                GridTile tile = tileGrid[tilePosition];
                // Player found; update original shape to match the copy (since it's already move and rotated, ready for use)
                if (tile.Occupier != null && tile.Occupier.team == team)
                {
                    abilityShape.Clear();
                    abilityShape.AddRange(copyShape);
                    return true;
                }
            }
        }
        return false;
    }

    // Show targetable tiles with VisualTiles and subscribe to VisualTile events
    public void ShowTargetTiles(List<GridTile> tiles, Action<GridTileVisual> onHover, Action<GridTileVisual> onExit, Action<GridTileVisual> onClick)
    {
        // Store references to new delegates
        hoverH = onHover; exitH = onExit; clickH = onClick;

        // Hide all tiles by hiding parent gameObject 
        tileParent.gameObject.SetActive(false);

        int usedCount = 0; // Number of unique tiles checked

        foreach(GridTile tile in tiles) 
        {
            // Just in case; stop if objectPool runs out of tiles
            if (usedCount >= visualTilePool.Length)
                break;

            // Get tile number usedCount and increase the counter
            GridTileVisual visual = visualTilePool[usedCount];
            usedCount++;

            // Occupied tile; only change color, no events
            if (tileGrid[tile.GridPosition].Occupied)
            {
                visual.Initialize(tile.GridPosition, occupiedColor);
            }
            else 
            {
                // Set tile coordinate and subscribe to VisualTile events
                visual.Initialize(tile.GridPosition, openColor);
                visual.OnHover += onHover;
                visual.OnExit += onExit;
                visual.OnClick += onClick;
            }

            // Activate the visualTile
            visual.gameObject.SetActive(true);
            visual.gameObject.transform.position = GridToWorld(tile.GridPosition);
        }

        // Disable any extra tiles left from previous iteration
        for (int i = usedCount; i < previousActiveTileCount; i++)
        {
            visualTilePool[i].gameObject.SetActive(false);
        }

        // Store the count how many tiles were activated
        previousActiveTileCount = usedCount;

        // Toggle tiles visible / active via parent gameObject
        tileParent.gameObject.SetActive(true);
    }

    // Show Highlight Tiles for ability targeting area / shape
    public void ShowHighlights(List<Vector2Int> abilityShape, Vector2Int startPosition, Vector2Int targetPosition, bool rotateTargeting)
    {
        // Create copy of shape to rotate it without affecting original
        List<Vector2Int> shapeCopy = new List<Vector2Int>(abilityShape);

        // Rotate shape if ability rotates with direction
        if (rotateTargeting)
            RotateShape(shapeCopy, startPosition, targetPosition);

        // Hide all tiles by hiding parent gameObject
        highlightParent.gameObject.SetActive(false);

        int usedCount = 0; // Number of unique tiles checked
        int shapeCount = shapeCopy.Count;
        
        while(usedCount < shapeCount)
        {
            // Just in case; stop if objectPool runs out of tiles
            if (usedCount >= highlightPool.Length)
                break;

            // Get tile number usedCount and increase the counter
            GridTileHighlight highlight = highlightPool[usedCount];

            // Activate the tile
            highlight.gameObject.SetActive(true);
            highlight.gameObject.transform.position = HighlightToWorld(shapeCopy[usedCount] + targetPosition);
            usedCount++;
        }

        // Disable any extra tiles left from previous iteration
        for (int i = usedCount; i < previousActiveHighlightCount; i++)
        {
            highlightPool[i].gameObject.SetActive(false);
        }

        // Store the count how many tiles were activated
        previousActiveHighlightCount = usedCount;

        // Toggle tiles visible / active via parent gameObject
        highlightParent.gameObject.SetActive(true);
    }

    // Move shape positions to reflect targetPosition
    public void MoveShape(List<Vector2Int> shape, Vector2Int targetPosition)
    {
        for (int i = 0; i < shape.Count; i++)
            shape[i] += targetPosition;
    }

    // Rotate shape to direction that start is "staring" to target
    public void RotateShape(List<Vector2Int> shape, Vector2Int startPosition, Vector2Int targetPosition)
    {
        Vector2Int direction = GetDirectionToTarget(startPosition, targetPosition);
        for (int i = 0; i < shape.Count; i++)
            shape[i] = RotatePoint(shape[i], direction);

        // Get the cardinal direction from startPosition to targetPosition
        Vector2Int GetDirectionToTarget(Vector2Int start, Vector2Int target)
        {
            Vector2Int direction = target - start;
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                return new Vector2Int(Math.Sign(direction.x), 0);
            else
                return new Vector2Int(0, Math.Sign(direction.y));
        }

        // Rotate each position in shape
        Vector2Int RotatePoint(Vector2Int position, Vector2Int direction)
        {
            if (direction == Vector2Int.up)
                return position;
            else if (direction == Vector2Int.right)
                return new Vector2Int(position.y, -position.x);
            else if (direction == Vector2Int.down)
                return new Vector2Int(-position.x, -position.y);
            else
                return new Vector2Int(-position.y, position.x);
        }
    }

    // Calculate 3D world position with Vector2Int position (Highlight tiles are bit Y-higher to avoid overlapping)
    private Vector3 HighlightToWorld(Vector2Int gridPosition)
    {
        return new Vector3(origin.x + gridPosition.x * (tileSize + tilePadding), origin.y + 0.01f, origin.z + gridPosition.y * (tileSize + tilePadding));
    }

    // Hide VisualTiles and HightlightTiles instantly and unsubscribe delegates from VisualTiles
    public void HideVisualTiles()
    {
        // Hide parent objects to instantly hide all tiles
        tileParent.SetActive(false);
        highlightParent.SetActive(false);

        // Are there references to delegates?
        if(hoverH != null && exitH != null && clickH != null) 
        {
            // Unsubscribe all delegates from tile events
            for (int i = 0; i < previousActiveTileCount; i++)
            {
                visualTilePool[i].OnHover -= hoverH;
                visualTilePool[i].OnExit -= exitH;
                visualTilePool[i].OnClick -= clickH;
            }

            // Remove references to delegates
            hoverH = null;
            exitH = null;
            clickH = null;
        }
    }

    // Calculate 3D world position with Vector2Int position
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(origin.x + gridPosition.x * (tileSize + tilePadding), origin.y, origin.z + gridPosition.y * (tileSize + tilePadding));
    }

    #region Editor Functions
#if UNITY_EDITOR
    // Generate grid for editor to help in level building
    [ContextMenu("Generate Grid in Editor")]
    public void GenerateEditorGrid()
    {
        // Generate editor tiles equal to grid size
        ClearEditorTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Generate editorTile gameObject, set it's world position, initialize visualTile and add it to editorTile dictionary
                Vector2Int pos = new(x, y);
                Vector3 worldPos = GridToWorld(pos);
                GameObject tileGO = Instantiate(editorTilePrefab, worldPos, Quaternion.Euler(90, 0, 0));
                tileGO.transform.SetParent(editorTileParent.transform, true);
                GridTileVisualEditor visual = tileGO.GetComponent<GridTileVisualEditor>();
                visual.Initialize(pos);
                editorTiles[pos] = visual;
            }
        }

        // Load occupied tiles from LevelTileData
        foreach(TileData tile in levelTileData.Tiles)
            editorTiles[tile.Coordinates].LoadOccupied();
        editorTiles[new Vector2Int(0,0)].ReadyToEnable();
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    // Generate objectPool for Visual and Highlight tiles
    [ContextMenu("Generate VisualTile Pool")]
    public void GenerateTilePool()
    {
        // Clear old pool and generate new array with fixed size
        ClearVisualTilePool();  
        visualTilePool = new GridTileVisual[visualTilePoolSize];
        highlightPool = new GridTileHighlight[highlightPoolSize];

        // Generate visual tile pool
        for (int i = 0; i < visualTilePoolSize; i++)
        {
            GameObject tileGO = Instantiate(tilePrefab, new Vector3(0, -50, 0), Quaternion.Euler(90, 0, 0), tileParent.transform);
            GridTileVisual visual = tileGO.GetComponent<GridTileVisual>();
            visualTilePool[i] = visual;
        }

        // Generate highlight tile pool
        for (int i = 0; i < highlightPoolSize; i++)
        {
            GameObject tileGO = Instantiate(highlightPrefab, new Vector3(0, -50, 0), Quaternion.Euler(90, 0, 0), highlightParent.transform);
            GridTileHighlight visual = tileGO.GetComponent<GridTileHighlight>();
            highlightPool[i] = visual;
        }
        EditorSceneManager.MarkSceneDirty(gameObject.scene);

        // Delete previous pools
        void ClearVisualTilePool()
        {
            int childCount = tileParent.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
                DestroyImmediate(tileParent.transform.GetChild(i).gameObject);

            childCount = highlightParent.transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
                DestroyImmediate(highlightParent.transform.GetChild(i).gameObject);
        }
    }

    // Delete all EditorTiles
    [ContextMenu("Clear Editor Grid")]
    private void ClearEditorTiles()
    {
        // Clear editorTile dictionary and destroy all gameObjects on editorTileParent object
        editorTiles.Clear();
        int childCount = editorTileParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
            DestroyImmediate(editorTileParent.transform.GetChild(i).gameObject);
        EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }

    // Save current occupied tiles to LevelTileData
    [ContextMenu("Save Level Tile Data")]
    private void SaveLevelTileData()
    {
        // Prevents accidentally saving if there's no editorGrid currently
        if (editorTiles.Count <= 0)
            return;

        // Clear old saved levelData
        levelTileData.Tiles.Clear();

        // Add entry to levelData for each editorTile that is occupied 
        foreach(KeyValuePair<Vector2Int, GridTileVisualEditor> tile in editorTiles) 
        {
            if (tile.Value.occupied)
                levelTileData.Tiles.Add(new TileData(tile.Key, tile.Value.occupied));
        }

        // Set scene dirty and save assets
        EditorUtility.SetDirty(levelTileData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
    #endregion
}