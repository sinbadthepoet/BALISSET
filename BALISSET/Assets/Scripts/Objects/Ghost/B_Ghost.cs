using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class B_Ghost : MonoBehaviour
{
    protected B_Shell _PossessedShell;

    public virtual void Possess(B_Shell TargetShell)
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

    public virtual void Release()
    {
        if (_PossessedShell == null) { return; }

        _PossessedShell.Release();

        Debug.Log("Ghost " + gameObject.name + " has been released from Shell " + _PossessedShell.name);

        _PossessedShell = null;
    }

    //TODO: this is temp behavior.
    protected virtual void Start()
    {
        Possess(FindAnyObjectByType<B_Shell>());
    }
}
