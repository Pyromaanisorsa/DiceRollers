using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Braindead behaviour; used for target dummies
[CreateAssetMenu(fileName = "BraindeadBehaviour", menuName = "CombatData/AIBehaviour/Braindead")]
public class BraindeadBehaviour : EnemyAIBehaviour
{
    public override void SelectAction(EnemyCombat enemy)
    {
        Debug.Log("BRAINDEAD.... END TURN...");
        CombatManager.Instance.EndTurn();
    }
}