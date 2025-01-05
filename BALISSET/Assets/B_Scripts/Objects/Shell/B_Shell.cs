using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class B_Shell : MonoBehaviour
{
    protected int health;
    protected B_Ghost _Ghost;

    /// <summary>
    /// A dictionary of actions the shell supports. Allows GrabAction() to stay the same, and we simply add new actions to the dictionary in InitializeActions.
    /// </summary>
    protected Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>> ShellActions = new();

    /// <summary>
    /// Checks for an action function by string, and returns a delegate.
    /// </summary>
    public System.Action<UnityEngine.InputSystem.InputAction.CallbackContext> GrabAction(string ActionName)
    {
        if (ShellActions.TryGetValue(ActionName, out var ActionDelegate))
        {
            return ActionDelegate;
        }
        Debug.Log($"Action {ActionName} not supported by {gameObject.name}");
        return null;
    }

    public virtual void Possess(B_Ghost Ghost)
    {
        _Ghost = Ghost;
    }

    public virtual void Release()
    {
        _Ghost = null;
    }

    protected virtual void InitializeActions()
    {
        //ShellActions.Add("Move", Move);
        ShellActions = new Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>>
        {
            //{ "Move", Move },
        };
    }
}
