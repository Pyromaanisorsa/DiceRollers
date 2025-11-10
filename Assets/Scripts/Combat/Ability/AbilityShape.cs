using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Determines ability's area of effect; subclasses can use abilityData's abilityWidth and abilityLength to influence the size
[System.Serializable]
public abstract class AbilityShape : ScriptableObject
{
    public abstract List<Vector2Int> GetShapeList(int length, int width, bool ignoreTarget);
    public abstract int GetShapeCount(bool ignoreTarget);
}
