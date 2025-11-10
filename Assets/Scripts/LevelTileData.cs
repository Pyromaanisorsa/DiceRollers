using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewLevelTileData", menuName = "LevelData/LevelTileData")]
[System.Serializable]
public class LevelTileData : ScriptableObject
{
    [SerializeField] private List<TileData> tiles;
    public List<TileData> Tiles => tiles;
}

[System.Serializable]
public class TileData
{
    [SerializeField] private Vector2Int coordinates;
    [SerializeField] private bool occupied;

    public Vector2Int Coordinates => coordinates;
    public bool Occupied => occupied;

    public TileData(Vector2Int coordinates, bool occupied) 
    {
        this.coordinates = coordinates;
        this.occupied = occupied;
    }
}
