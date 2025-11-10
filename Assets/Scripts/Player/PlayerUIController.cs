using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Has references to all UI elements player needs to control
public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private CombatUIController combatUI;

    public void Initialize(CombatUIController combatUI, PlayerCombat combat, string playerID)
    {
        this.combatUI = combatUI;
        combatUI.Initialize(combat, playerID);
    }
}
