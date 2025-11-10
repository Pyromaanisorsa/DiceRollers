using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatUIController : MonoBehaviour
{
    public GameObject abilitiesBox;
    public GameObject turnBox;
    public TMP_Text turnText;
    public GameObject playerStatsBox;
    public TMP_Text nameText;
    public TMP_Text apText;
    public TMP_Text hpText;
    public List<AbilityButton> abilityButtons = new();
    public AbilityButton moveButton;
    public AbilityButton endTurnButton;
    private PlayerCombat playerCombat;

    // Initialize via playerCore
    public void Initialize(PlayerCombat combat, string playerID)
    {
        // Set reference to playerCombat, subscribe to playerCombat and CombatManager events
        playerCombat = combat;
        CombatManager.Instance.OnTurnSwapped += HandleTurnSwap;
        CombatManager.Instance.OnRollStarted += HandleRollStart;
        CombatManager.Instance.OnActionPrepare += HandleActionPrepare;
        CombatManager.Instance.OnTurnEnd += HandleTurnEnd;
        playerCombat.OnAPChange += HandleAPChange;
        playerCombat.OnHPChange += HandleHPChange;
        playerCombat.OnPlayerTurnStart += HandlePlayerTurnStart;

        // Print playerID
        nameText.text = playerID;
        hpText.text = $"HP: {combat.stats.currentStats.HP}/{combat.stats.maxStats.HP}";

        // Add Listeners to abilityButtons
        moveButton.SetMoveButton(combat);
        endTurnButton.SetEndTurnButton(combat);

        for (int i = 0; i < abilityButtons.Count; i++) 
        {
            if (i > combat.stats.abilities.Count)
                return;

            abilityButtons[i].SetAbilityButton(i, combat.stats.abilities[i]);
        }
    }

    // Toggle ON/OFF entire UI
    public void ToggleUI(bool active)
    {
        abilitiesBox.SetActive(active);
        turnBox.SetActive(active);
        playerStatsBox.SetActive(active);
    }

    // Event delegate; toggle UI to player/enemy turn specifics
    private void HandleTurnSwap(Team team)
    {
        if (team == Team.Player) 
            turnText.text = "PLAYER TURN";
        else
        {
            turnText.text = "ENEMY TURN";
            apText.text = "";
            turnBox.SetActive(true);
            playerStatsBox.SetActive(true);
        }
    }

    // Event delegate; hide player abilitiesBox UI 
    private void HandleActionPrepare() 
    {
        abilitiesBox.SetActive(false);
    }

    // Event delegate; hide player abilitiesBox UI when starting a roll
    private void HandleRollStart() 
    {
        abilitiesBox.SetActive(false);
    }

    // Event delegate; hide abilitiesBox UI when player ends turn manually
    private void HandleTurnEnd()
    {
        abilitiesBox.SetActive(false);
    }

    // Event delegate; update player AP count and set interactability for each abilityButton if player has enough AP
    private void HandlePlayerTurnStart(int currentAP, List<AbilitySlot> abilitySlots)
    {
        bool active = currentAP >= 3;
        moveButton.UpdateInteractability(active);
        
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            active = currentAP >= abilitySlots[i].abilityData.abilityStats.APCost;
            abilityButtons[i].UpdateInteractability(active);
        }
        ToggleUI(true);
    }

    // Event delegate; update Player AP
    private void HandleAPChange(int ap) 
    {
        apText.text = $"AP: {ap}";
    }

    // Event delegate; update Player HP
    private void HandleHPChange(int currentHP, int maxHP)
    {
        hpText.text = $"HP: {currentHP}/{maxHP}";
    }
}
