using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractive
{
    public string GetInteractionString();

    public void Interact(B_Shell Interactor);
}
