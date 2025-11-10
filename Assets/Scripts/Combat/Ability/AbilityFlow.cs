using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Determines the flow of ability / how it's used; eg. do you click on valid tile or does it auto activate on selection (in future might add confirm/cancel phase to ability choosing)
public abstract class AbilityFlow : ScriptableObject
{
    public abstract void Execute(CombatParticipant caster, AbilityData data);
    public abstract List<Vector2Int> ExecuteAI(CombatParticipant caster, AbilityData data);
    public abstract Vector2Int CalculateMovePositionAI(CombatParticipant caster, AbilityData data);
    protected virtual int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
