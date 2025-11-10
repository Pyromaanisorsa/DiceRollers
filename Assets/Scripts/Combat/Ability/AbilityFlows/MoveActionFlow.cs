using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Used only for Basic Move action / Hard coded Move action
[CreateAssetMenu(fileName = "MoveActionFlow", menuName = "CombatData/AbilityFlow/MoveAction")]
public class MoveActionFlow : AbilityFlow
{
    // Start ability flow logic
    public override void Execute(CombatParticipant caster, AbilityData data)
    {
        // Get tiles in Move range
        List<GridTile> tiles = GridManager.Instance.GetMovableTiles(caster.gridPosition, caster.stats.currentStats.MOVE);

        // Remove all tiles with occupiers in them (Leftover / redundant)
        for (int i = tiles.Count - 1; i > 0; i--)
            if (tiles[i].Occupier != null)
                tiles.RemoveAt(i);

        // Generate delegates / lambda functions
        Action<GridTileVisual> hoverHandler = tile => OnHover(tile);
        Action<GridTileVisual> exitHandler = tile => OnExit(tile);
        Action<GridTileVisual> clickHandler = tile => OnClick(tile);

        // Show targetTiles and subcribe to their events with delegates
        GridManager.Instance.ShowTargetTiles(tiles, hoverHandler, exitHandler, clickHandler);
    }

    // Swap color on current hovered tile
    private void OnHover(GridTileVisual tile)
    {
        tile.SetColor(GridManager.Instance.hoverColor);
    }

    // Reset color on leaving tile
    private void OnExit(GridTileVisual tile)
    {
        tile.SetColor(GridManager.Instance.openColor);
    }

    // Move player to clicked tile
    private void OnClick(GridTileVisual tile)
    {
        GridManager.Instance.HideVisualTiles();
        CombatManager.Instance.UseMove(tile.GridPosition);
    }

    public override List<Vector2Int> ExecuteAI(CombatParticipant caster, AbilityData data)
    {
        // NO AI IMPLEMENTATION
        return null;
    }

    public override Vector2Int CalculateMovePositionAI(CombatParticipant caster, AbilityData data)
    {
        // NO AI IMPLEMENTATION
        return Vector2Int.zero;
    }
}
