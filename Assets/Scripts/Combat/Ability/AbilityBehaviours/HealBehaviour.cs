using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Heal desired targets
[CreateAssetMenu(fileName = "HealBehaviour", menuName = "CombatData/AbilityBehaviour/HealBehaviour")]
public class HealBehaviour : AbilityBehaviour
{
    public override void Execute(AbilityData ability, CombatParticipant caster, List<Vector2Int> targets, int rollValue)
    {
        // Heal health to all tiles that contain occupier and ability can target (Teams)
        foreach (Vector2Int target in targets)
        {
            GridTile tile = GridManager.Instance.GetTileAtPosition(target);
            if (tile != null)
                if (tile.Occupier != null)
                {
                    if (ability.abilityStats.TargetAllies && tile.Occupier.team == caster.team) 
                    {
                        CombatManager.Instance.currentMessage += $"Attacked {tile.Occupier.characterID} at {target}\n";
                        tile.Occupier.HealHealth(ability.abilityStats.Healing);
                    }
                    else if (ability.abilityStats.TargetEnemies && tile.Occupier.team != caster.team) 
                    {
                        CombatManager.Instance.currentMessage += $"Attacked {tile.Occupier.characterID} at {target}\n";
                        tile.Occupier.HealHealth(ability.abilityStats.Healing);
                    }
                }
        }
    }
}
