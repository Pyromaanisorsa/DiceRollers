using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Behaviour for GoblinRanger (Currently same as GoblinBehaviour)
[CreateAssetMenu(fileName = "GoblinRangerBehaviour", menuName = "CombatData/AIBehaviour/GoblinRanger")]
public class GoblinRangerBehaviour : EnemyAIBehaviour
{
    public override void SelectAction(EnemyCombat enemy)
    {
        // Not enough AP or Actions left to use; EndTurn
        Debug.Log($"Goblin | AP: {enemy.currentAP} | Actions: {enemy.actionPerTurn}");
        if (enemy.actionPerTurn <= 0 || enemy.currentAP < 3)
        {
            Debug.Log("GOBLIN END TURNI, FEELING LAZY");
            CombatManager.Instance.EndTurn();
            return;
        }

        // Enough AP for attack; try attacking if possible
        if (enemy.currentAP >= 5)
        {
            Debug.Log("ME STRUNG FUR ATTACKING! YES YES!");
            // Is player in attack range; Attack or Move closer
            List<Vector2Int> shape = enemy.stats.abilities[0].abilityData.flow.ExecuteAI(enemy, enemy.stats.abilities[0].abilityData);
            if (shape != null)
            {
                Debug.Log("ITTI PLAYER, ME PUCNHI");
                CombatManager.Instance.UseAbility(enemy.stats.abilities[0], shape);
            }
            else
            {
                Debug.Log("PLAYER NO HERE ME GO CLOSER");
                Vector2Int moveToTile = enemy.stats.abilities[0].abilityData.flow.CalculateMovePositionAI(enemy, enemy.stats.abilities[0].abilityData);
                CombatManager.Instance.UseMove(moveToTile);
            }
        }
        // Not enough AP for attack; move to better spot
        else if (enemy.currentAP >= 3)
        {
            Debug.Log("GOBLIN NO ENOUGH AP FOR PUNCHI PUNCHI");
            Vector2Int moveToTile = enemy.stats.abilities[0].abilityData.flow.CalculateMovePositionAI(enemy, enemy.stats.abilities[0].abilityData);
            CombatManager.Instance.UseMove(moveToTile);
        }
        else
            CombatManager.Instance.EndTurn();
    }
}
