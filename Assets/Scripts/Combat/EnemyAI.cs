using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy AI container; contains it's behaviour and runtime/current AI decision making values
public class EnemyAI : MonoBehaviour
{
    public EnemyCombat combat;
    public EnemyAIBehaviour behaviour;
    public float fearValue;
    public float rageValue;

    public void ChangeAIBehaviour(EnemyAIBehaviour behaviour) 
    {
        this.behaviour = behaviour;
    }
}
