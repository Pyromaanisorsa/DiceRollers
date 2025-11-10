using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInputs keybinds;
    private ActionMapController currentActionMap;
    public CameraFollow followCamera;
    private PlayerCore core;

    // Inner variables
    private bool allowCameraControl = true;
    private float interactDistance = 2f;
    public IInteractable currentInteractible;
    public Coroutine currentCoroutine;

    private void Awake()
    {
        // Initialize playerInputs
        keybinds = new PlayerInputs();
        
        // Testing purposes; swapping ActionMapController by force
        //keybinds.General.Swap1.started += ctx => SwapActionMap(new GeneralController(this));
        //keybinds.General.Swap2.started += ctx => SwapActionMap(new MenuController(this));
        //keybinds.General.Swap3.started += ctx => SwapActionMap(new EventController(this));
    }

    // Rotate player camera if currentActionMap allows camera control
    void Update()
    {
        if (allowCameraControl && followCamera != null)
        {
            float rotateValue = keybinds.General.RotateCamera.ReadValue<float>();
            if (rotateValue != 0)
                followCamera.Rotate(rotateValue);
            //followCamera.Rotate(rotateValue * Time.deltaTime); TOO SLOW AND THIS IS A DEMO SO WHATEVER
        }
    }

    // Swap current actionMap
    public void SwapActionMap(ControllerType controllerType)
    {
        // Prevent "swapping" same action map
        Debug.Log($"Swapping to {controllerType}");
        if (currentActionMap.controllerType == controllerType)
            return;

        // Allow camera control only in GeneralController / Gameplay ActionMap
        if (controllerType == ControllerType.Gameplay)
            allowCameraControl = true;
        else
            allowCameraControl = false;

        // Create new ActionMapController based off Enum type
        ActionMapController newController = CreateActionMapInstance();

        // Unsubcribe current action map inputs and subcribe to new action map inputs
        currentActionMap.Disable();
        currentActionMap = newController;
        currentActionMap.Enable();

        ActionMapController CreateActionMapInstance() 
        {
            switch (controllerType) 
            {
                case ControllerType.Gameplay:
                    return new GameplayController(this, ControllerType.Gameplay);
                case ControllerType.Event:
                    return new EventController(this, ControllerType.Event);
                case ControllerType.Menu:
                    return new MenuController(this, ControllerType.Menu);
                default:
                    return null;
            }
        }
    }

    // Zoom in/out player camera if currentActionMap allows camera control
    public void ScrollCamera(InputAction.CallbackContext context)
    {
        if (allowCameraControl && followCamera != null) 
        {
            if (context.ReadValue<Vector2>().y < 0)
                followCamera.Zoom(-1);
            else
                followCamera.Zoom(1);
        }
    }

    // Used to pass reference to camera+attributeBox and spawn setup on player spawn
    public void Initialize(PlayerCore core, CameraFollow camera) 
    {
        this.core = core;
        if(followCamera == null) 
        {
            followCamera = camera;
            camera.player = gameObject.transform;

            currentActionMap = new GameplayController(this, ControllerType.Gameplay);
            currentActionMap.Enable();
            keybinds.General.Enable();
        }
    }

    // Move next to clicked Interactable object and Interact() with it.
    public void MoveToInteract(RaycastHit hit, IInteractable interactable)
    {
        //// Start generic move and start the MoveToInteract Coroutine logic
        //Move(hit);
        //currentInteractible = interactable;
        //Vector3 interactablePosition = hit.collider.gameObject.transform.position;
        ////Vector2 interact2 = new Vector2(hit.collider.gameObject.transform.position.x, hit.collider.gameObject.transform.position.z);
        //currentCoroutine = StartCoroutine(MoveToInteractRoutine());

        //// Move close enough to clicked Interactable object to Interact with it.
        //IEnumerator MoveToInteractRoutine()
        //{
        //    while (Vector3.Distance(transform.position, interactablePosition) >= interactDistance) 
        //    {
        //        Debug.Log(Vector3.Distance(transform.position, interactablePosition));
        //        yield return null;
        //    }
                

        //    //while (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), interact2) >= interactDistance) 
        //    //{
        //    //    Debug.Log(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), interact2));
        //    //    yield return null;
        //    //}

        //    agent.isStopped = true;
        //    currentInteractible.Interact(playerID);
        //    currentInteractible = null;
        //}
    }
}