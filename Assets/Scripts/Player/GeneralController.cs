using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

// Used for general gameplay
public class GameplayController : ActionMapController
{
    public GameplayController(PlayerController controller, ControllerType type) : base(controller, type) {}

    public override void Enable()
    {
        playerController.keybinds.Gameplay.Enable();
        //playerController.keybinds.Gameplay.MouseAction.started += MouseAction;
        playerController.keybinds.General.ScrollCamera.performed += playerController.ScrollCamera;
    }

    public override void Disable() 
    {
        playerController.keybinds.Gameplay.MouseAction.started -= MouseAction;
        //playerController.keybinds.General.ScrollCamera.performed -= playerController.ScrollCamera;
        playerController.keybinds.Gameplay.Disable();
    }

    // React to clicking something with mouse
    private void MouseAction(InputAction.CallbackContext context)
    {
        //if (EventSystem.current.IsPointerOverGameObject())
        //    return;

        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        //// If clicked Interactable object; move to Interact with it | Else; move to clicked spot
        //if (Physics.Raycast(ray, out hit))
        //{
        //    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        //    if (interactable != null)
        //        playerController.MoveToInteract(hit, interactable);
        //    else
        //        playerController.Move(hit);
        //}
    }
}
