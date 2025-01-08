using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Ghost : MonoBehaviour
{
    protected B_Shell _Shell;

    protected virtual void Possess(B_Shell Shell)
    {
        if (Shell == null)
        {
            Debug.LogError($"Ghost {gameObject.name} did not get a valid Shell to possess.");
            return;
        }

        if (_Shell != null)
        {
            _Shell.Release();
        }

        _Shell = Shell;
        Shell.Possess(this);

        Debug.Log("Ghost " + gameObject.name + " has possessed Shell " + _Shell.name);
    }

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
