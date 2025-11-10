using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base for enemy AI behaviour; contains SelectAction logic and (future proofing) base values that influence decision making
public abstract class EnemyAIBehaviour : ScriptableObject
{
    public float baseFearValue;
    public float baseRageValue;
    public abstract void SelectAction(EnemyCombat enemy);
}
