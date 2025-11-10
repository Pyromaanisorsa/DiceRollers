using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbilityShape
{
    abstract List<GridTile> GetTargetTiles(Vector2Int startCoordinates, int length);
}
