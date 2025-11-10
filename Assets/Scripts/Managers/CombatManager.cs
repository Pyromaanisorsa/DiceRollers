using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using TMPro;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public PlayerCombat player;
    public bool playerDead = false; // Testing purposes since only one player at the moment
    private List<EnemyCombat> enemyList = new();
    public List<EnemyCombat> pendingDeaths = new();
    private Queue<EnemyCombat> enemyTurnQueue = new();
    [SerializeField] private MoveActionFlow moveFlow; // Built-in move flow for player movement action
    [SerializeField] private CameraFollow camera;

    // Current turn related variables
    private bool combatActive;
    private Team currentTeamTurn;
    public CombatParticipant currentParticipant;
    private AbilitySlot selectedAbility;
    private List<Vector2Int> targetPositions;

    // Public events; used to update the UI
    public event Action<Team> OnTurnSwapped;
    public event Action OnRollStarted;
    public event Action OnActionPrepare;
    public event Action OnTurnEnd;

    // Quick message panel setup to show what happened between actions
    public GameObject messagePanel;
    public TMP_Text messagePanelText;
    public string currentMessage;

    private void Awake()
    {
        // Create instance and ensure that THERE CAN ONLY BE ONE!
        if (Instance == null)
        {
            Instance = this;

            // Initialize team lists and turn order list
            //teams = new Dictionary<Team, List<CombatParticipant>>();
            //foreach (Team t in Enum.GetValues(typeof(Team))) 
            //{
            //    teams[t] = new List<CombatParticipant>();
            //    turnOrder.Add(t);
            //}
            //teams[Team.Player] = new List<CombatParticipant>();
            //teams[Team.Enemy] = new List<CombatParticipant>();

        }
        // Instance already exists, you fool!
        else
        {
            UnityEngine.Debug.Log("There can only be one!");
            Destroy(gameObject);
        }
    }

    // Add PlayerCombat reference to CombatManager
    public void AddPlayer(PlayerCombat player)
    {
        this.player = player;
    }

    // Add Enemy to EnemyList
    public void AddParticipant(EnemyCombat participant)
    {
        enemyList.Add(participant);
    }

    // Remove Enemy from EnemyList
    public void RemoveParticipant(EnemyCombat participant)
    {
        enemyList.Remove(participant);
    }

    // TEST SCENE RESTART
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Toggle Combat ON/OFF | DEMO SETUP
    public void ToggleCombat(bool active)
    {
        if (active)
        {
            combatActive = true;
            currentTeamTurn = Team.Player;
            StartCoroutine(StartTurn());
        }
        else 
        {
        }
    }

    // Start NEW player / enemy turn
    private IEnumerator StartTurn()
    {
        // Change which team's turn it is
        if (currentTeamTurn == Team.Player)
            currentParticipant = player;
        else
            currentParticipant = enemyTurnQueue.Dequeue();

        // Invoke Turn swap event and change camera's follow gameObject to currentParticipant
        OnTurnSwapped?.Invoke(currentTeamTurn);
        camera.player = currentParticipant.gameObject.transform;

        // Display ending and starting turn messages
        messagePanelText.text += $"Next turn: {currentParticipant.characterID} at {currentParticipant.gridPosition}";
        messagePanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        messagePanel.gameObject.SetActive(false);

        // Start roll request for Start Turn AP via RollManager
        OnRollStarted?.Invoke();
        RollManager.Instance.StartTurnRoll(player.characterID, currentParticipant.characterID, currentParticipant.stats.currentStats.AP, StartTurnRollResult);
    }

    // Continue currentParticipant's turn
    private IEnumerator ContinueTurn()
    {
        // Wait 1 frame and cut call stack
        yield return null;

        // Continue turn (same as StartTurn but no AP roll)
        currentParticipant.ContinueTurn();
    }

    // Callback via RollManager; pass rolled AP to currentParticipant's startTurn
    private void StartTurnRollResult(int rollValue, bool rollSuccess)
    {
        currentParticipant.StartTurn(rollValue);
    }

    // End current turn
    public void EndTurn()
    {
        // Invoke EndTurn event, hide all visual tiles
        OnTurnEnd?.Invoke();
        GridManager.Instance.HideVisualTiles();

        // Swap currentTeamTurn, create a queue for Enemy Team's Turn
        if (currentTeamTurn == Team.Player) 
        {
            currentTeamTurn = Team.Enemy;
            foreach (EnemyCombat enemy in enemyList)
                enemyTurnQueue.Enqueue(enemy);
        }
        else
        {
            if(enemyTurnQueue.Count <= 0)
                currentTeamTurn = Team.Player;
        }

        // Add who ended their turn message to currentMessage
        messagePanelText.text = $"{currentParticipant.characterID} {currentParticipant.gridPosition} ended their turn\n";
        StartCoroutine(StartTurn());
    }

    // Check if currentParticipant has to end or continue their turn
    private void EndOrContinueTurn()
    {
        if (currentParticipant.currentAP <= 0)
            EndTurn();
        else
            StartCoroutine(ContinueTurn());
    }

    // Prepare Player Move; show movable tiles with moveFlow
    public void PrepareMove()
    {
        OnActionPrepare?.Invoke();
        moveFlow.Execute(currentParticipant, null); // AbilityData parameter is null since we won't need it for a Move Action
    }

    // Move current participant to selected tile
    public void UseMove(Vector2Int movePosition)
    {
        // Use coroutine to "pause" the game while displaying messages and to break up the call stack
        StartCoroutine(UseMoveRoutine());

        IEnumerator UseMoveRoutine() 
        {
            // Store the message, before moving currentParticipant
            messagePanelText.text = $"{currentParticipant.characterID} moved: {currentParticipant.gridPosition} -> {movePosition}";

            // Reduce AP cost of move and move currentParticipant on tileGrid
            currentParticipant.UpdateAP(3);
            GridManager.Instance.MoveToTile(currentParticipant, movePosition);

            // Display action message
            messagePanel.gameObject.SetActive(true);
            yield return new WaitForSeconds(3);
            messagePanel.gameObject.SetActive(false);
            EndOrContinueTurn();
        }
    }

    // Prepare Player Ability
    public void PrepareAbility(AbilitySlot ability)
    {
        // Store reference to selected player abiltitySlot and execute it's flow behaviour
        OnActionPrepare?.Invoke();
        selectedAbility = ability;
        selectedAbility.abilityData.flow.Execute(currentParticipant, ability.abilityData);
    }

    // Player version: Execute currentParticipant ability at target tiles
    public void UseAbility(List<Vector2Int> targetPositions)
    {
        // Store targets list and check if ability skips combat roll / always succeeds
        this.targetPositions = targetPositions;
        if (!selectedAbility.abilityData.skipCombatRoll)
            RollManager.Instance.StartAbilityRoll(player.characterID, currentParticipant.characterID, selectedAbility.abilityData.abilityStats.AbilityName, UseAbilityRollResult);
        else
            StartCoroutine(UseAbilitySkipRollRoutine());
    }

    // AI overload version: Store enemy currentParticipant ability and execute it at target tiles
    public void UseAbility(AbilitySlot enemyAbility, List<Vector2Int> targetPositions)
    {
        // Store targets list and ability selected by enemy AI
        this.targetPositions = targetPositions;
        selectedAbility = enemyAbility;

        // Check if ability skips combat roll / always succeeds
        if (!selectedAbility.abilityData.skipCombatRoll)
            RollManager.Instance.StartAbilityRoll(player.characterID, currentParticipant.characterID, selectedAbility.abilityData.abilityStats.AbilityName, UseAbilityRollResult);
        else
            StartCoroutine(UseAbilitySkipRollRoutine());
    }

    // Skip combat roll and execute ability behaviour
    IEnumerator UseAbilitySkipRollRoutine()
    {
        // Add casting message to currentMessage, reduce AP cost of ability from caster and execute the ability behaviour
        currentMessage = $"{currentParticipant.characterID} cast {selectedAbility.abilityData.abilityStats.AbilityName} at {targetPositions[0]}\n";
        currentParticipant.UpdateAP(selectedAbility.abilityData.abilityStats.APCost);
        selectedAbility.abilityData.behaviour.Execute(selectedAbility.abilityData, currentParticipant, targetPositions, 20); // We pass 20 to guarantee successful ability usage

        // Display the currentMessage
        messagePanelText.text = currentMessage;
        messagePanel.gameObject.SetActive(true);
        yield return new WaitForSeconds(5);
        messagePanel.gameObject.SetActive(false);

        // Check if player died; game over
        if (playerDead)
        {
            messagePanelText.text = "GAME OVER! Restart to try again.";
            messagePanel.gameObject.SetActive(true);
        }
        else 
        {
            // Check all the pending deaths if any
            if (pendingDeaths.Count > 0)
                ProcessDeaths();
            // No deaths; just continue turn flow
            else
                EndOrContinueTurn();
        }
    }

    // Callback via RollManager; reduce AP cost of ability and execute it's behaviour
    private void UseAbilityRollResult(int rollValue, bool rollSuccess) 
    {
        StartCoroutine(UseAbilityRollResultRoutine());

        IEnumerator UseAbilityRollResultRoutine() 
        {
            // Add casting message to currentMessage, reduce AP cost of ability from caster and execute the ability behaviour
            currentMessage = $"{currentParticipant.characterID} cast {selectedAbility.abilityData.abilityStats.AbilityName} at {targetPositions[0]}\n";
            currentParticipant.UpdateAP(selectedAbility.abilityData.abilityStats.APCost);
            selectedAbility.abilityData.behaviour.Execute(selectedAbility.abilityData, currentParticipant, targetPositions, rollValue);

            // Display the currentMessage
            messagePanelText.text = currentMessage;
            messagePanel.gameObject.SetActive(true);
            yield return new WaitForSeconds(5);
            messagePanel.gameObject.SetActive(false);
            messagePanelText.text = "";

            // Check if player died; game over
            if (playerDead)
            {
                messagePanelText.text = "GAME OVER! Restart to try again.";
                messagePanel.gameObject.SetActive(true);
            }
            else
            {
                // Check all the pending deaths
                if (pendingDeaths.Count > 0)
                    ProcessDeaths();
                // No deaths; just continue turn flow
                else
                    EndOrContinueTurn();
            }
        }
    }

    // Process enemy deaths after ability execution
    private void ProcessDeaths()
    {
        // Make temporary filtered queue with no dead enemies
        Queue<EnemyCombat> filteredQueue = new Queue<EnemyCombat>();
        // Make queue of dead enemies with pendingDeaths list
        Queue<EnemyCombat> deadEnemyQueue = new Queue<EnemyCombat>(pendingDeaths);

        // Remove dead enemies from enemyList
        foreach (EnemyCombat deadEnemy in pendingDeaths)
            enemyList.Remove(deadEnemy);

        // Remove dead enemies from tileGrid and destroy their gameObjects
        while (deadEnemyQueue.Count > 0)
        {
            EnemyCombat enemy = deadEnemyQueue.Dequeue();
            GridManager.Instance.DestroyEntityOnTile(enemy, enemy.gridPosition);
        }

        // Filter dead enemies out from enemyTurnQueue
        while (enemyTurnQueue.Count > 0)
        {
            EnemyCombat enemy = enemyTurnQueue.Dequeue();
            if (!pendingDeaths.Contains(enemy))
                filteredQueue.Enqueue(enemy);
        }

        // Clear pendingDeaths list
        pendingDeaths.Clear();

        // Copy filtered queue to enemyTurnQueue
        enemyTurnQueue = new Queue<EnemyCombat>(filteredQueue);

        // All enemies dead; combat over, game won
        if (enemyList.Count == 0) 
        {
            messagePanelText.text = "Victory! Restart to try again.";
            messagePanel.gameObject.SetActive(true);
        }
        // Still enemies alive; continue combat
        else
            EndOrContinueTurn();
    }
}

// Enum for combat teams
public enum Team 
{
    Player,
    Enemy
}
