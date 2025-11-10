using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Horizontal line shape
[CreateAssetMenu(fileName = "HorizontalLineShape", menuName = "CombatData/AbilityShapes/HorizontalLineShape")]
public class HorizontalLineShape : AbilityShape
{
    public override List<Vector2Int> GetShapeList(int length, int width, bool ignoreTarget)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        if (!ignoreTarget)
            tiles.Add(new Vector2Int(0,0));

        for(int d = 1; d <= width; d++)
        {
            tiles.Add(new Vector2Int(d, 0));
            tiles.Add(new Vector2Int(-d, 0));
        }
        return tiles;
    }

    public override int GetShapeCount(bool ignoreTarget)
    {
        return 1;
    }
}
