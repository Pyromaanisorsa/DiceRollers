using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Vertical line shape
[CreateAssetMenu(fileName = "VerticalLineShape", menuName = "CombatData/AbilityShapes/VerticalLineShape")]
public class VerticalLineShape : AbilityShape
{
    public override List<Vector2Int> GetShapeList(int length, int width, bool ignoreTarget)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        if (!ignoreTarget)
            tiles.Add(new Vector2Int(0, 0));

        for (int d = 1; d <= length; d++)
        {
            tiles.Add(new Vector2Int(0, d));
        }
        return tiles;
    }

    public override int GetShapeCount(bool ignoreTarget)
    {
        return 1;
    }
}
