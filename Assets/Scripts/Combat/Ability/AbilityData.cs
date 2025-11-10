using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Contains data and components of each Ability
[CreateAssetMenu(fileName = "NewAbility", menuName = "CombatData/AbilityData")]
public class AbilityData : ScriptableObject
{
    [Header("Ability Components")]
    public AbilityShape shape; // Abilities area / shape
    public AbilityFlow flow; // Flow of the ability / how it works
    public AbilityBehaviour behaviour; // Behaviour of the ability / the actual effect
    public TargetFilterBase targetFilter; // Filter out unnecessary tiles from ability
    public bool skipCombatRoll;
    public bool rotateTargeting;
    public bool ignoreStartSelection;
    public bool ignoreTargetTargeting;
    public bool throughObstacles;

    [Space(5)]
    public AbilityStatsBlock abilityStats;
}

[System.Serializable]
public struct AbilityStatsBlock
{
    public string AbilityName;
    [TextArea] public string AbilityDescription;
    public int APCost, Damage, Healing, Range, RangedGap, ShapeLength, ShapeWidth;
    public bool TargetSelf, TargetAllies, TargetEnemies;
}

[System.Serializable]
public enum TargetingMode 
{
    Any,
    EmptyOnly,
}
