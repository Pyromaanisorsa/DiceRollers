using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for Ability filters
public abstract class TargetFilterBase : ScriptableObject, ITargetFilter
{
    public abstract bool IsValidTarget(GridTile tile, CombatParticipant caster);
}
