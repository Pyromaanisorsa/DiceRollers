using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Determines the effect ability has in game logic eg. deal damage or heal health
public abstract class AbilityBehaviour : ScriptableObject
{
    public abstract void Execute(AbilityData ability, CombatParticipant caster, List<Vector2Int> targets, int rollValue);
}
