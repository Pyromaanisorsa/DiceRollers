using UnityEngine;

public class GridTile
{
    public Vector2Int GridPosition { get; private set; }    // XY-position on the grid
    public bool Occupied { get; set; } = false;             // Check if tile is blocked by eg. walls, rocks
    public CombatParticipant Occupier { get; set; }         // Check if tile is blocked by player / enemy / NPC

    public GridTile(Vector2Int gridPos)
    {
        GridPosition = gridPos;
    }
}