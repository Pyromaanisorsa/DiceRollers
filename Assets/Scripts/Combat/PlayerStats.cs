using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : Stats
{
    public PlayerStats(PlayerClassData classData)
    {
        StatBlock baseStats = classData.BaseStats;

        maxStats = baseStats;
        currentStats = baseStats;

        foreach (AbilityData ability in classData.StartingAbilities)
            abilities.Add(new AbilitySlot(ability));
    }
}
