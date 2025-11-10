using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable 
{
    // Need to be implemented per class
    void Interact(PlayerCore player);
    void Success();
    void Failure();
    void Exit(PlayerCore player);
}