using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Teleport to selected tile (only used for Teleport atm)
[CreateAssetMenu(fileName = "MoveBehaviour", menuName = "CombatData/AbilityBehaviour/MoveBehaviour")]
public class MoveBehaviour : AbilityBehaviour
{
    public override void Execute(AbilityData ability, CombatParticipant caster, List<Vector2Int> targets, int rollValue)
    {
        CombatManager.Instance.currentMessage = $"{caster.characterID} teleported: {caster.gridPosition} -> {targets[0]}\n";
        GridManager.Instance.MoveToTile(caster, targets[0]);
    }
}