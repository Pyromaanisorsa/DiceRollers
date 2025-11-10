using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : CombatParticipant
{
    public int actionPerTurn;
    public EnemyAI ai;

    private void Start()
    {
        stats = new EnemyStats(this.EnemyData);
        characterID = EnemyData.CharacterName;
    }

    public void Initialize()
    {
        stats = new EnemyStats(this.EnemyData);
        characterID = EnemyData.CharacterName;
        team = Team.Enemy;
    }

    // Enemy AI Turn start; use AI behaviour to decide action for this turn
    public override void StartTurn(int rollAP)
    {
        currentAP = stats.currentStats.AP + rollAP;
        actionPerTurn = 3;
        ai.behaviour.SelectAction(this);
    }

    public override void ContinueTurn()
    {
        actionPerTurn--;
        ai.behaviour.SelectAction(this);
    }

    public override void UpdateAP(int cost)
    {
        currentAP -= cost;
    }

    public override void TakeDamage(int damage)
    {
        // Already dead, redundant check?
        if (stats.currentStats.HP <= 0)
            return;

        int currentHP = stats.currentStats.HP;

        // Overkill? Change HP to 0 and die
        if (currentHP - damage < 0)
        {
            stats.currentStats.HP = 0;
            CombatManager.Instance.currentMessage += $"{characterID} takes {damage} damage ({stats.currentStats.HP}/{stats.maxStats.HP})\n";
            OnDeath();
        }
        // Else; take damage and check if player died
        else
        {
            stats.currentStats.HP -= damage;
            CombatManager.Instance.currentMessage += $"{characterID} takes {damage} damage ({stats.currentStats.HP}/{stats.maxStats.HP})\n";
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
    }

    // On death; add enemy reference to pendingDeaths list in combatManager
    protected override void OnDeath()
    {
        CombatManager.Instance.currentMessage += $"{characterID} {gridPosition} dies\n";
        CombatManager.Instance.pendingDeaths.Add(this);
    }
}
