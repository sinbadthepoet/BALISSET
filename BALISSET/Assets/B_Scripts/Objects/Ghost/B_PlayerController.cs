using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class B_PlayerController : B_Ghost
{
    [SerializeField] InputActionAsset inputActionAsset;

    /// <summary>
    /// This stores the delegates from the shell, using the InputAction as the key. We use this for unbinding on release.
    /// </summary>
    Dictionary<InputAction, System.Action<InputAction.CallbackContext>> BoundActions = new Dictionary<InputAction, System.Action<InputAction.CallbackContext>>();

    /// <summary>
    /// Upon Possession, check shell for actions that correspond with IA Asset, and store delegates to functions for callbacks.
    /// </summary>
    protected override void Possess(B_Shell Shell)
    {
        base.Possess(Shell);

        if (inputActionAsset == null)
        {
            Debug.Log($"Ghost {gameObject.name} does not have a valid InputActionAsset set!");
        } 
        else 
        {
            foreach (var map in inputActionAsset.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    var ActionDelegate = _Shell.GrabAction(action.name);
                    if (ActionDelegate != null)
                    {
                        action.performed += ActionDelegate;
                        BoundActions[action] = ActionDelegate;
                    }
                }
            }
        }
    }

    protected override void Release()
    {
        base.Release();

        foreach (var pair in BoundActions)
        {
            pair.Key.performed -= pair.Value;
        }
        BoundActions.Clear();
    }
}
