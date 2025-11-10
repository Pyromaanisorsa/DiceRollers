using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    [SerializeField] private List<PlayerEntry> playerList; // Testing / demonstration purposes since Unity Inspector doesn't support dictionaries
    private Dictionary<string, PlayerCore> playerDictionary = new Dictionary<string, PlayerCore>();

    // Create instance for manager
    private void Awake()
    {
        // Create instance and ensure that THERE CAN ONLY BE ONE!
        if (Instance == null)
        {
            Instance = this;
        }
        // Instance already exists, you fool!
        else
        {
            Debug.Log("There can only be one!");
            Destroy(gameObject);
        }
    }

    // Add new / spawned player to playerDictionary(and list)
    public void AddPlayer(string playerID, PlayerCore player) 
    {
        if (playerDictionary.TryAdd(playerID, player))
            playerList.Add(new PlayerEntry(playerID, player));
        else
            Debug.Log("Player with that playerID already spawned!");
    }

    // Get reference to Player(core) via playerID
    public PlayerCore GetPlayer(string playerID) 
    {
        if (playerDictionary.TryGetValue(playerID, out PlayerCore player))
            return player;
        return null;
    }
}

// Used for testing / demonstration with the playerList
[System.Serializable]
public class PlayerEntry
{
    public PlayerEntry(string playerID, PlayerCore player) 
    {
        this.playerID = playerID;
        this.player = player;
    }

    public PlayerCore player;
    public string playerID;
}
