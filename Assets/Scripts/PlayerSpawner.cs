using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject spawnWindow;
    [SerializeField] private GameObject scenarioWindow;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private CameraFollow mainCamera;
    [SerializeField] private CombatUIController combatUI;
    public List<PlayerClassData> playerClasses;
    public List<GameObject> enemyPrefabs;

    // Placeholder player spawning method; spawn player prefab, add spawned player to playerManager and pass necessary references to playerCore
    public void SpawnPlayer(int classIndex)
    {
        // Username in inputfield too short or long
        if (inputField.text.Length < 3 || inputField.text.Length > 20)
            return;

        // Set spawnUI inactive, spawn player prefab, add player to playerManager player list and initialize it
        spawnWindow.SetActive(false);
        GameObject player = GridManager.Instance.SpawnEntityOnTile(playerPrefab, new Vector2Int(14, 2), false);
        PlayerCore core = player.GetComponent<PlayerCore>();
        PlayerManager.Instance.AddPlayer(inputField.text, core);
        core.InitializePlayer(inputField.text, new Vector2Int(14,2), playerClasses[classIndex], combatUI, mainCamera);

        // Add player to combatManager player reference (temp solution)
        CombatManager.Instance.AddPlayer(core.combat);

        // Show all scenario difficulty buttons to select difficulty and start combat
        scenarioWindow.SetActive(true);
    }

    // Spawn enemies for each difficulty / scenario (placeholder for demonstration purposes)
    public void StartScenario(int difficulty)
    {
        scenarioWindow.SetActive(false);
        switch (difficulty) 
        {
            case 1:
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(10, 3), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(5, 4), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(15, 5), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(9, 9), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(2, 9), true);
                break;
            case 2:
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(6, 4), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[2], new Vector2Int(10, 13), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(15, 5), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(9, 9), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(2, 9), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(13, 7), true);
                break;
            case 3:
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(14, 5), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(10, 4), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(17, 3), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[2], new Vector2Int(3, 6), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[2], new Vector2Int(11, 13), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(3, 9), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(18, 9), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[3], new Vector2Int(3, 13), true);
                break;
            case 4:
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[3], new Vector2Int(10, 4), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[2], new Vector2Int(15, 7), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(3, 4), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(5, 6), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(13, 9), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[1], new Vector2Int(18, 1), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[2], new Vector2Int(6, 10), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[2], new Vector2Int(1, 13), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[3], new Vector2Int(18, 13), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[0], new Vector2Int(10, 12), true);
                break;
            case 5:
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(19, 14), true);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(10, 3), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(10, 4), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(10, 5), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(10, 6), false);

                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(3, 4), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(3, 5), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(3, 6), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(4, 4), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(4, 5), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(4, 6), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(5, 4), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(5, 5), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(5, 6), false);

                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(18, 9), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(19, 7), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(18, 5), false);

                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(15, 0), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(16, 0), false);
                GridManager.Instance.SpawnEntityOnTile(enemyPrefabs[4], new Vector2Int(17, 0), false);
                break;
        }
        CombatManager.Instance.ToggleCombat(true);
    }
}
