using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // Static singleton instance
    public static DialogueManager Instance;

    [Header("UI Elements")]
    public TMP_Text dialog;
    public GameObject textBox;
    public GameObject continueBox;
    
    // References
    private Dialogue dialogue;
    private IInteractable interacter;
    private PlayerCore player;

    // Helper variables
    private NodeEntry currentNode;
    private int selectedChoiceIndex;
    private bool rollResult;
    private bool interactable = false;

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

    // Start new dialogue; enable Dialog UI and store necessary references
    public void StartDialogue(Dialogue dialogue, IInteractable interacter, PlayerCore player)
    {
        textBox.SetActive(true);
        this.player = player;
        this.interacter = interacter;
        this.dialogue = dialogue;
        currentNode = dialogue.StartNode;
        DisplayDialogNode();
    }

    // Change current dialogue node
    private void ChangeNode(int index)
    {
        if (currentNode.NodeOptions(index).EndNode)
            EndDialogue();
        else
        {
            currentNode = dialogue.Nodes(currentNode.NodeOptions(index).Index);
            DisplayDialogNode();
        }
    }

    // End dialogue; hide Dialog UI and clean up any references
    private void EndDialogue()
    {
        textBox.SetActive(false);
        continueBox.SetActive(false);
        
        dialogue = null;
        currentNode = null;
        interacter.Exit(player);
        player = null;
        interacter = null;
    }

    // Show current dialogue node
    private void DisplayDialogNode()
    {
        // Action is based off of currentNodes type
        switch (currentNode.DialogNode)
        {
            // Show staticTextNode's dialog
            case StaticTextNode staticTextNode:
                dialog.text = staticTextNode.Dialog;
                continueBox.SetActive(true);
                break;

            // Show all choiceNode's choices
            case ChoiceNode choiceNode:
                continueBox.SetActive(false);
                int length = choiceNode.Length;
                dialog.text = "";

                // Display text based off of choiceNodeAction enum value
                for (int i = 0; i < length; i++)
                {
                    if (choiceNode.Choices(i).NodeType == ChoiceNodeAction.RollDice)
                        dialog.text += $"{i + 1}. {choiceNode.Choices(i).Dialog} ({currentNode.NodeOptions(i).AttributeType} DC {currentNode.NodeOptions(i).RollRequired})\n";
                    else
                        dialog.text += $"{i + 1}. {choiceNode.Choices(i).Dialog}\n";
                }
                break;

            // Show result text based off of result
            case ResultNode resultNode:
                if (rollResult)
                    dialog.text = resultNode.Success;
                else
                    dialog.text = resultNode.Failure;
                continueBox.SetActive(true);
                break;
        }
        interactable = true;
    }

    // StaticText and ResultNodes: progress dialogue (triggered by player input)
    public void ContinueDialogue()
    {
        if (interactable && currentNode.DialogNode.GetType() != typeof(ChoiceNode))
        {
            interactable = false;
            ChangeNode(0);
        }
    }

    // ChoiceNode: select option to progress dialogue / call for roll (triggered by player input)
    public void SelectChoice(int index)
    {
        // Ensure that this function is not run unless needed
        if (interactable && currentNode.DialogNode.GetType() == typeof(ChoiceNode))
        {
            // Run action based off choice index's choiceNodeAction enum value
            interactable = false;
            ChoiceNode node = currentNode.DialogNode as ChoiceNode;       
            switch (node.Choices(index).NodeType) 
            {
                case ChoiceNodeAction.Normal: // Progress to next dialogNode
                    ChangeNode(index);
                    break;

                case ChoiceNodeAction.RollDice: // Start a roll with RollManager
                    selectedChoiceIndex = index;
                    int rollRequired = currentNode.NodeOptions(index).RollRequired;
                    AttributeType rollAttribute = currentNode.NodeOptions(index).AttributeType;
                    int playerAttribute = player.stats.GetAttributeValue(rollAttribute);
                    string playerID = player.playerID;
                    RollManager.Instance.StartDialogueRoll(rollRequired, rollAttribute, playerAttribute, playerID);
                    break;
            }
        }
    }

    // ResultNode: trigger IInteractable object's Success/Failure based off of roll result, called from RollManager
    public void ResultProcess(bool result)
    {
        if (result)
            interacter.Success();
        else
            interacter.Failure();
        rollResult = result;
        ChangeNode(selectedChoiceIndex);
    }
}
