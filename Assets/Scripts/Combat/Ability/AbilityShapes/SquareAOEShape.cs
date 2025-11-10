using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Square shaped area of effect shape
[CreateAssetMenu(fileName = "SquareAOEShape", menuName = "CombatData/AbilityShapes/SquareAOEShape")]
public class SquareAOEShape : AbilityShape
{
    public override List<Vector2Int> GetShapeList(int length,  int width, bool ignoreTarget)
    {
        List<Vector2Int> tiles = new List<Vector2Int>();
        int xMin = -((width - 1) / 2);
        int xMax = (width / 2);
        int yMin = -((length - 1) / 2);
        int yMax = (length / 2);

        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yMin; y <= yMax; y++)
            {
                // optionally skip the center
                if (ignoreTarget && x == 0 && y == 0)
                    continue;

                tiles.Add(new Vector2Int(x, y));
            }
        }
        return tiles;
    }

    public override int GetShapeCount(bool ignoreTarget)
    {
        return 1;
    }
}
