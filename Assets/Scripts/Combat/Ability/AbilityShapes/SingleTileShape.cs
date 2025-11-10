using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Single tile shape
[CreateAssetMenu(fileName = "SingleTileShape", menuName = "CombatData/AbilityShapes/SingleTileShape")]
public class SingleTileShape : AbilityShape
{
    public override List<Vector2Int> GetShapeList(int length, int width, bool ignoreTarget)
    {
        List<Vector2Int> shape = new List<Vector2Int>();
        shape.Add(new Vector2Int(0, 0));
        return shape;
    }

    public override int GetShapeCount(bool ignoreTarget)
    {
        return 1;
    }
}
