using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for player class data and enemy data
public abstract class CharacterData : ScriptableObject
{
    [field: SerializeField] public string CharacterName { get; private set; }
    [field: SerializeField] public StatBlock BaseStats { get; private set; }
    [field: Space(10)]
    [field: SerializeField] public AbilityData[] StartingAbilities { get; private set; }
}
