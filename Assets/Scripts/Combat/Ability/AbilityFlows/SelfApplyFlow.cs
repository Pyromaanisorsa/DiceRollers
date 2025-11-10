using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SelfApplyFlow", menuName = "CombatData/AbilityFlow/SelfApply")]
public class SelfApplyFlow : AbilityFlow
{
    // Start ability flow logic
    public override void Execute(CombatParticipant caster, AbilityData data)
    {
        // Get ability shape coordinates list
        List<Vector2Int> shape = data.shape.GetShapeList(data.abilityStats.ShapeLength, data.abilityStats.ShapeWidth, data.ignoreTargetTargeting);

        // Move shape in caster position
        GridManager.Instance.MoveShape(shape, caster.gridPosition);

        // Use Ability straight away
        CombatManager.Instance.UseAbility(shape);
    }

    // AI version of Execute; returns list of all targetable tiles
    public override List<Vector2Int> ExecuteAI(CombatParticipant caster, AbilityData data)
    {
        // Get ability shape coordinates list
        List<Vector2Int> shape = data.shape.GetShapeList(data.abilityStats.ShapeLength, data.abilityStats.ShapeWidth, data.ignoreTargetTargeting);

        // Move shape in caster position
        GridManager.Instance.MoveShape(shape, caster.gridPosition);

        return shape;
    }

    public override Vector2Int CalculateMovePositionAI(CombatParticipant caster, AbilityData data)
    {
        // Not Implementing for this version
        return caster.gridPosition;
    }
}
