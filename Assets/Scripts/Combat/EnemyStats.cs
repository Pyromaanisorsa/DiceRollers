using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : Stats
{
    public EnemyStats(EnemyData enemyData)
    {
        StatBlock baseStats = enemyData.BaseStats;

        maxStats = baseStats;
        currentStats = baseStats;

        foreach (AbilityData ability in enemyData.StartingAbilities)
            abilities.Add(new AbilitySlot(ability));
    }
}
