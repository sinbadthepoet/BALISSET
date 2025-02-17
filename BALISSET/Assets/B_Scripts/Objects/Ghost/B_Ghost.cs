using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Ghost : MonoBehaviour
{
    protected B_Shell _PossessedShell;

    /// <summary>
    /// Binds the Ghost to a new Shell, allowing it to be controlled.
    /// Will release earlier possessed shell if one exists.
    /// </summary>
    /// <param name="TargetShell"></param> The Shell to be possessed.
    protected virtual void Possess(B_Shell TargetShell)
    {
        if (TargetShell == null)
        {
            Debug.LogError($"Ghost {gameObject.name} did not get a valid Shell to possess.");
            return;
        }

        //Release the Shell we may already be bound to.
        if (_PossessedShell != null)
        {
            Release();
        }

        _PossessedShell = TargetShell;
        _PossessedShell.Possess(this);

        Debug.Log("Ghost " + gameObject.name + " has possessed Shell " + _PossessedShell.name);
    }

    /// <summary>
    /// Releases the Shell from possession.
    /// </summary>
    protected virtual void Release()
    {
        if (_PossessedShell != null)
        {
            _PossessedShell.Release();
            _PossessedShell = null;

            Debug.Log("Ghost " + gameObject.name + " has been released from Shell " + _PossessedShell.name);
        }
    }

    //TODO: this is temp behavior.
    protected virtual void Start()
    {
        Possess(FindAnyObjectByType<B_Shell>());
    }
}
