using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class that represents a game piece on the board
public abstract class CombatParticipant : MonoBehaviour
{
    public string characterID;
    public Team team;
    public int currentAP;
    public Vector2Int gridPosition;
    public Stats stats;
    public CharacterData originData;

    // Methods to get CharacterData as it's true class
    public PlayerClassData PlayerClass => originData as PlayerClassData;
    public EnemyData EnemyData => originData as EnemyData;

    public abstract void StartTurn(int rollAP);
    public virtual void EndTurn() 
    {
        CombatManager.Instance.EndTurn();
    }
    public abstract void ContinueTurn();
    public abstract void UpdateAP(int cost);
    public abstract void TakeDamage(int damage);
    public abstract void HealHealth(int heal);
    protected abstract void OnDeath();
}
