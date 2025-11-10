using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Damage desired targets
[CreateAssetMenu(fileName = "DamageBehaviour", menuName = "CombatData/AbilityBehaviour/DamageBehaviour")]
public class DamageBehaviour : AbilityBehaviour
{
    public override void Execute(AbilityData ability, CombatParticipant caster, List<Vector2Int> targets, int rollValue)
    {
        // Deal damage to all tiles that contain occupier and ability can target (Teams)
        foreach (Vector2Int target in targets)
        {
            GridTile tile = GridManager.Instance.GetTileAtPosition(target);
            if (tile != null)
                if(tile.Occupier != null)
                {
                    if (ability.abilityStats.TargetAllies && tile.Occupier.team == caster.team) 
                    {
                        if (rollValue >= tile.Occupier.stats.currentStats.DEX) 
                        {
                            CombatManager.Instance.currentMessage += $"Attacked {tile.Occupier.characterID} at {target}\n";
                            tile.Occupier.TakeDamage(ability.abilityStats.Damage);
                        }
                    }
                    else if (ability.abilityStats.TargetEnemies && tile.Occupier.team != caster.team) 
                    {
                        if (rollValue >= tile.Occupier.stats.currentStats.DEX)
                        {
                            CombatManager.Instance.currentMessage += $"Attacked {tile.Occupier.characterID} at {target}\n";
                            tile.Occupier.TakeDamage(ability.abilityStats.Damage);
                        }
                    }
                }
        }
    }
}