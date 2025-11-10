using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI Button you press to use abilities
public class AbilityButton : MonoBehaviour
{
    [SerializeField] private TMP_Text abilityName;
    [SerializeField] private TMP_Text abilityCost;
    [SerializeField] private Button abilityButton;

    // Set Button to use Movement action on click
    public void SetMoveButton(PlayerCombat combat) 
    {
        abilityName.text = "1.Move";
        abilityCost.text = "AP Cost: 3";
        abilityButton.onClick.AddListener(CombatManager.Instance.PrepareMove);
    }

    // Set Button to end turn on click
    public void SetEndTurnButton(PlayerCombat combat)
    {
        abilityName.text = "5.End Turn";
        abilityCost.text = "";
        abilityButton.onClick.AddListener(CombatManager.Instance.EndTurn);
    }

    // Set Button to use player's specific AbilitySlot on click
    public void SetAbilityButton(int index, AbilitySlot selectedAbility)
    {
        abilityName.text = $"{index+2}. {selectedAbility.abilityData.abilityStats.AbilityName}";
        abilityCost.text = $"AP Cost: {selectedAbility.abilityData.abilityStats.APCost}";
        abilityButton.onClick.AddListener(() => CombatManager.Instance.PrepareAbility(selectedAbility));
    }

    // Swap if button is interactable (not enough AP for the action)
    public void UpdateInteractability(bool active) 
    {
        abilityButton.interactable = active;
    }
}
