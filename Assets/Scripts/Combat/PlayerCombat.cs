using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerCombat : CombatParticipant
{
    public event Action<int> OnAPChange;
    public event Action<int, int> OnHPChange;
    public event Action<int, List<AbilitySlot>> OnPlayerTurnStart;
    int turn = 0;

    // Initialize playerCombat; add reference to base class and create starting stats
    public void Initialize(string playerID, PlayerClassData classData, Vector2Int spawnPoint)
    {
        characterID = playerID;
        stats = new PlayerStats(classData);
        originData = classData;
        team = Team.Player;

        // Move player to starting point (TEMP SOLUTION FOR TESTING)
        GridManager.Instance.MoveToTile(this, spawnPoint);
    }

    // Player turn start; invoke OnPlayerTurnStart to enable combat UI for player to choose action
    public override void StartTurn(int rollAP)
    {
        currentAP = rollAP + stats.currentStats.AP;
        OnAPChange?.Invoke(currentAP);
        OnPlayerTurnStart?.Invoke(currentAP, stats.abilities);
    }

    public override void ContinueTurn()
    {
        OnPlayerTurnStart?.Invoke(currentAP ,stats.abilities);
    }

    // Reduce AP-cost of ability and update AP-counter via invoke
    public override void UpdateAP(int cost)
    {
        currentAP -= cost;
        OnAPChange?.Invoke(currentAP);
    }

    // Take damage, if player dies; tell CombatManager so to end the game (TEMP TESTING SOLUTION)
    public override void TakeDamage(int damage)
    {
        // Already dead, redundant check?
        if (stats.currentStats.HP <= 0)
            return;
        
        int currentHP = stats.currentStats.HP;

        // Overkill? Change HP to 0 and die
        if(currentHP - damage < 0)
        {
            stats.currentStats.HP = 0;
            CombatManager.Instance.currentMessage += $"{characterID} takes {damage} damage ({stats.currentStats.HP}/{stats.maxStats.HP})\n";
            OnHPChange?.Invoke(stats.currentStats.HP, stats.maxStats.HP);
            OnDeath();
        }
        // Else; take damage and check if player died
        else 
        {
            stats.currentStats.HP -= damage;
            CombatManager.Instance.currentMessage += $"{characterID} takes {damage} damage ({stats.currentStats.HP}/{stats.maxStats.HP})\n";
            OnHPChange?.Invoke(stats.currentStats.HP, stats.maxStats.HP);
            if (stats.currentStats.HP == 0)
                OnDeath();
        }
    }

    // Heal health, no overhealing allowed and update HP counter via invoke
    public override void HealHealth(int heal)
    {
        CombatManager.Instance.currentMessage += $"{characterID} healed {heal}HP\n";
        int currentHP = stats.currentStats.HP;
        if (currentHP + heal > stats.maxStats.HP)
            stats.currentStats.HP = stats.maxStats.HP;
        else
            stats.currentStats.HP += heal;
        OnHPChange?.Invoke(stats.currentStats.HP, stats.maxStats.HP);
    }

    // Player died; tell COmbatManager to end the game when checking deaths after action (TEMP TESTING)
    protected override void OnDeath()
    {
        CombatManager.Instance.currentMessage += $"{characterID}{gridPosition} dies\n";
        CombatManager.Instance.playerDead = true;
    }
}
