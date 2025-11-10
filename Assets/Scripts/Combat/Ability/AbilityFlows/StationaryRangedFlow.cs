using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "StationaryRangedFlow", menuName = "CombatData/AbilityFlow/StationaryRanged")]
public class StationaryRangedFlow : AbilityFlow
{
    // Start ability flow logic
    public override void Execute(CombatParticipant caster, AbilityData data)
    {
        // Get ability shape coordinates list
        List<Vector2Int> shape = data.shape.GetShapeList(data.abilityStats.ShapeLength, data.abilityStats.ShapeWidth, data.ignoreTargetTargeting);

        // Get tiles in cardinal Ranged range
        List<GridTile> tiles = new List<GridTile>();
        GridTile tile;
        
        // Add tile for up to ability range and skip depth based of RangedGap
        for(int i = 1 + data.abilityStats.RangedGap; i < data.abilityStats.Range + 1; i++) 
        {
            tile = GridManager.Instance.GetTileAtPosition(caster.gridPosition + new Vector2Int(0, i));
            if (tile != null)
                tiles.Add(tile);
            tile = GridManager.Instance.GetTileAtPosition(caster.gridPosition + new Vector2Int(0, -i));
            if (tile != null)
                tiles.Add(tile);
            tile = GridManager.Instance.GetTileAtPosition(caster.gridPosition + new Vector2Int(-i, 0));
            if (tile != null)
                tiles.Add(tile);
            tile = GridManager.Instance.GetTileAtPosition(caster.gridPosition + new Vector2Int(i, 0));
            if (tile != null)
                tiles.Add(tile);
        }

        // Remove tiles ignored with TargetFilters
        if (data.targetFilter != null)
        {
            for (int i = tiles.Count - 1; i > 0; i--)
                if (!data.targetFilter.IsValidTarget(tiles[i], caster))
                    tiles.RemoveAt(i);
        }

        // Generate delegates / lambda functions
        Action<GridTileVisual> hoverHandler = tile => OnHover(tile, caster, data, shape);
        Action<GridTileVisual> exitHandler = tile => OnExit(tile);
        Action<GridTileVisual> clickHandler = tile => OnClick(tile, caster, data, shape);

        // Show targetTiles and subcribe to their events with delegates
        GridManager.Instance.ShowTargetTiles(tiles, hoverHandler, exitHandler, clickHandler);
    }

    // Swap color on current hovered tile
    private void OnHover(GridTileVisual tile, CombatParticipant caster, AbilityData data, List<Vector2Int> shape)
    {
        tile.SetColor(GridManager.Instance.hoverColor);
        GridManager.Instance.ShowHighlights(shape, caster.gridPosition, tile.GridPosition, data.rotateTargeting);
    }

    // Reset color on leaving tile
    private void OnExit(GridTileVisual tile)
    {
        tile.SetColor(GridManager.Instance.openColor);
    }

    // Use the abiity on clicked tile
    private void OnClick(GridTileVisual tile, CombatParticipant caster, AbilityData data, List<Vector2Int> shape)
    {
        // Hide visual tiles (if somehow currently active)
        GridManager.Instance.HideVisualTiles();

        // Rotate the shape if ability rotates in ability execution
        if (data.rotateTargeting)
            GridManager.Instance.RotateShape(shape, caster.gridPosition, tile.GridPosition);

        // Move ability shape to target tile position and use the ability
        GridManager.Instance.MoveShape(shape, tile.GridPosition);
        CombatManager.Instance.UseAbility(shape);
    }

    // AI version of Execute; returns list of all targetable tiles
    public override List<Vector2Int> ExecuteAI(CombatParticipant caster, AbilityData data)
    {
        // Get ability shape and some other necessary variables
        int abilityShapeWidth = data.abilityStats.ShapeWidth;
        int abilityShapeLength = data.abilityStats.ShapeLength;
        bool ignoreTargetTile = data.ignoreTargetTargeting;
        bool rotateTargeting = data.rotateTargeting;
        Vector2Int casterPosition = caster.gridPosition;
        List<Vector2Int> shape = data.shape.GetShapeList(abilityShapeLength, abilityShapeWidth, ignoreTargetTile);

        // Get tiles in cardinal Ranged range
        List<GridTile> tiles = new List<GridTile>();
        GridTile tile;

        // Add tile for each range and skip depth based of RangedGap
        for (int i = 1 + data.abilityStats.RangedGap; i < data.abilityStats.Range + 1; i++)
        {
            tile = GridManager.Instance.GetTileAtPosition(casterPosition + new Vector2Int(0, i));
            if (tile != null)
                tiles.Add(tile);
            tile = GridManager.Instance.GetTileAtPosition(casterPosition + new Vector2Int(0, -i));
            if (tile != null)
                tiles.Add(tile);
            tile = GridManager.Instance.GetTileAtPosition(casterPosition + new Vector2Int(-i, 0));
            if (tile != null)
                tiles.Add(tile);
            tile = GridManager.Instance.GetTileAtPosition(casterPosition + new Vector2Int(i, 0));
            if (tile != null)
                tiles.Add(tile);
        }

        // Remove tiles ignored with TargetFilters
        if (data.targetFilter != null)
        {
            for (int i = tiles.Count - 1; i > 0; i--)
                if (!data.targetFilter.IsValidTarget(tiles[i], caster))
                    tiles.RemoveAt(i);
        }

        // Check if any targeted tile hits player with shape
        for (int i = 0; i < tiles.Count; i++)
            if (GridManager.Instance.IsTeamOnShapeTiles(casterPosition, tiles[i].GridPosition, shape, rotateTargeting, Team.Player))
                return shape;

        // Return null if no player in melee range / can't hit player
        return null;
    }

    // Find closest(?) tile to move to where enemy can hit player next turn (Temp solution for ugabuga basic enemy AI)
    public override Vector2Int CalculateMovePositionAI(CombatParticipant caster, AbilityData data)
    {
        // Get ability shape
        Vector2Int casterPosition = caster.gridPosition;
        Vector2Int playerPosition = CombatManager.Instance.player.gridPosition;
        List<Vector2Int> shape = data.shape.GetShapeList(data.abilityStats.ShapeLength, data.abilityStats.ShapeWidth, data.ignoreTargetTargeting);

        // Get all tiles that can be moved to
        List<GridTile> moveTiles = GridManager.Instance.GetMovableTiles(casterPosition, caster.stats.currentStats.MOVE);

        // Remove all tiles with occupiers in them
        for (int i = moveTiles.Count - 1; i > 0; i--)
            if (moveTiles[i].Occupier != null)
                moveTiles.RemoveAt(i);

        // Variables to store best distance and move position
        Vector2Int bestPosition = casterPosition;
        int bestDistance = int.MaxValue;
        int distance;

        // Loop through all tiles enemy could move to.
        foreach (GridTile moveTile in moveTiles)
        {
            // Tile occupied / unmovable; go to next moveTile
            if (moveTile.Occupied == true)
                continue;

            // Track if player is hittable from current target
            bool canHit = false;
            // Get all tiles in attack range and iterate through them all
            // Get tiles in cardinal Ranged range
            List<GridTile> targetTiles = new List<GridTile>();
            GridTile tile;

            // Add tile for each range and skip depth based of RangedGap
            for (int i = 1 + data.abilityStats.RangedGap; i < data.abilityStats.Range + 1; i++)
            {
                tile = GridManager.Instance.GetTileAtPosition(moveTile.GridPosition + new Vector2Int(0, i));
                if (tile != null)
                    targetTiles.Add(tile);
                tile = GridManager.Instance.GetTileAtPosition(moveTile.GridPosition + new Vector2Int(0, -i));
                if (tile != null)
                    targetTiles.Add(tile);
                tile = GridManager.Instance.GetTileAtPosition(moveTile.GridPosition + new Vector2Int(-i, 0));
                if (tile != null)
                    targetTiles.Add(tile);
                tile = GridManager.Instance.GetTileAtPosition(moveTile.GridPosition + new Vector2Int(i, 0));
                if (tile != null)
                    targetTiles.Add(tile);
            }

            // Remove tiles ignored with TargetFilters
            if (data.targetFilter != null)
            {
                for (int i = targetTiles.Count - 1; i > 0; i--)
                    if (!data.targetFilter.IsValidTarget(targetTiles[i], caster))
                        targetTiles.RemoveAt(i);
            }

            for (int i = 0; i < targetTiles.Count; i++)
            {
                // If enemy can hit from moving to that tile; break and return the tile
                if (GridManager.Instance.IsTeamOnShapeTiles(moveTile.GridPosition, targetTiles[i].GridPosition, shape, data.rotateTargeting, Team.Player))
                {
                    canHit = true;
                    break;
                }
            }

            // Can hit player; return tile position
            if (canHit)
                return moveTile.GridPosition;

            // Can't hit player from that tile; store the location if it's closest tile
            distance = ManhattanDistance(moveTile.GridPosition, playerPosition);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPosition = moveTile.GridPosition;
            }
        }

        // Return closest move tile found
        return bestPosition;
    }
}
