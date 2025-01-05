using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Ghost : MonoBehaviour
{
    protected B_Shell _Shell;

    protected virtual void Possess(B_Shell Shell)
    {
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

    public virtual void SendCommand(string command, object arg)
    {
        if (_Shell != null)
        {
            _Shell.ExecuteCommand(command, arg);
        }
    }
}
