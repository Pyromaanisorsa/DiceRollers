using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableObject, IInteractable
{
    [SerializeField] private bool interactable;
    [SerializeField] private Dialogue dialogue;

    // Player interacts with door; swap player controls to Event ActionMap and start dialogue
    public void Interact(PlayerCore player)
    {
        player.controller.SwapActionMap(ControllerType.Event);
        DialogueManager.Instance.StartDialogue(dialogue, this, player);
    }

    // Open the door and disable interacting with door
    public void Success()
    {
        interactable = false;
        StartCoroutine(OpenDoorCoroutine());
    }

    // Irrelevant in this demo, must mandatory to be implemented
    public void Failure()
    {
        Debug.Log("Oot huono heitt‰‰ noppaa luuseri :P");
    }
    
    // Called after dialogue ends; set player controls back to General ActionMap
    public void Exit(PlayerCore player)
    {
        player.controller.SwapActionMap(ControllerType.Gameplay);
    }

    // Coroutine to open the door on success
    private IEnumerator OpenDoorCoroutine()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, -90, 0));

        float elapsedTime = 0f;

        while(elapsedTime < 3f) 
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / 3f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
    }
}
