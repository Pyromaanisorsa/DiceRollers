using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Base class for playerInputController state machine
public abstract class ActionMapController
{
    protected PlayerController playerController;
    public ControllerType controllerType { get; protected set; } 

    public ActionMapController(PlayerController controller, ControllerType type)
    {
        playerController = controller;
        controllerType = type;
    }

    public abstract void Enable();
    public abstract void Disable();
}

public enum ControllerType 
{
    Gameplay,
    Event,
    Menu
}
