using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Used when in menu
public class MenuController : ActionMapController
{
    public MenuController(PlayerController controller, ControllerType type) : base(controller, type) { }

    public override void Enable()
    {
        playerController.keybinds.Menu.Enable();
        //playerController.keybinds.Menu.MenuB.performed += MenuC;
    }
    public override void Disable()
    {
        //playerController.keybinds.Menu.MenuB.performed -= MenuC;
        playerController.keybinds.Menu.Disable();
    }

    private void MenuC(InputAction.CallbackContext context) 
    {
        Debug.Log("Menuttajan pizza B");
    }
}
