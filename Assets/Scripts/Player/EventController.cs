using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Used for ingame events eg. dialogue
public class EventController : ActionMapController
{
    public EventController(PlayerController controller, ControllerType type) : base(controller, type) { }

    public override void Enable()
    {
        // Enable input actions for this controller
        playerController.keybinds.Event.Enable();
        playerController.keybinds.Event.ContinueDialogue.performed += ContinueDialogue;
        playerController.keybinds.Event.ChooseOption1.performed += SelectDialogueOption1;
        playerController.keybinds.Event.ChooseOption2.performed += SelectDialogueOption2;
        playerController.keybinds.Event.ChooseOption3.performed += SelectDialogueOption3;
    }

    public override void Disable()
    {
        // Disable input actions for this controller
        playerController.keybinds.Event.ContinueDialogue.performed -= ContinueDialogue;
        playerController.keybinds.Event.ChooseOption1.performed -= SelectDialogueOption1;
        playerController.keybinds.Event.ChooseOption2.performed -= SelectDialogueOption2;
        playerController.keybinds.Event.ChooseOption3.performed -= SelectDialogueOption3;
        playerController.keybinds.Event.Disable();
    }

    // Continue active dialogue forward
    private void ContinueDialogue(InputAction.CallbackContext context)
    {
        DialogueManager.Instance.ContinueDialogue();
    }

    // Select option #1 on active dialogue when current dialogueNode is ChoiceNode
    private void SelectDialogueOption1(InputAction.CallbackContext context) 
    {
        DialogueManager.Instance.SelectChoice(0);
    }

    // Select option #2 on active dialogue when current dialogueNode is ChoiceNode
    private void SelectDialogueOption2(InputAction.CallbackContext context)
    {
        DialogueManager.Instance.SelectChoice(1);
    }

    // Select option #3 on active dialogue when current dialogueNode is ChoiceNode
    private void SelectDialogueOption3(InputAction.CallbackContext context)
    {
        DialogueManager.Instance.SelectChoice(2);
    }
}
