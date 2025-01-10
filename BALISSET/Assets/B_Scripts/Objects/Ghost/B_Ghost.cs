using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Ghost : MonoBehaviour
{
    protected B_Shell _Shell;

    /// <summary>
    /// Binds the Ghost to a new Shell, allowing it to be controlled.
    /// Will release earlier possessed shell if one exists.
    /// </summary>
    /// <param name="Shell"></param> The Shell to be possessed.
    protected virtual void Possess(B_Shell Shell)
    {
        if (Shell == null)
        {
            Debug.LogError($"Ghost {gameObject.name} did not get a valid Shell to possess.");
            return;
        }

        //Release the Shell we may already be bound to.
        if (_Shell != null)
        {
            Release();
        }

        _Shell = Shell;
        _Shell.Possess(this);

        Debug.Log("Ghost " + gameObject.name + " has possessed Shell " + _Shell.name);
    }

    /// <summary>
    /// Releases the Shell from possession.
    /// </summary>
    protected virtual void Release()
    {
        if (_Shell != null)
        {
            _Shell.Release();
            _Shell = null;

            Debug.Log("Ghost " + gameObject.name + " has been released from Shell " + _Shell.name);
        }
    }

    //TODO: this is temp debug behavior.
    protected virtual void Start()
    {
        Possess(FindAnyObjectByType<B_Shell>());
    }
}
