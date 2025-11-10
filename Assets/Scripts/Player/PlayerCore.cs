using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Master / Core player class; initializes everything and has playerID + references to all player components
public class PlayerCore : MonoBehaviour
{
    public string playerID;
    public PlayerCombat combat;
    public PlayerController controller;
    public PlayerStats stats;
    public PlayerUIController uiController;

    // Iniatialize spawned Player; give necessary references via playerSpawner
    public void InitializePlayer(string playerID, Vector2Int spawnPoint, PlayerClassData classData, CombatUIController combatUI, CameraFollow camera) 
    {
        this.playerID = playerID;
        combat.Initialize(playerID, classData, spawnPoint);
        uiController.Initialize(combatUI, combat, playerID);
        controller.Initialize(this, camera);
    }
}
