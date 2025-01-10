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
        //TODO: Look intro improving support for different Input Maps
        base.Possess(Shell);

        if (inputActionAsset == null)
        {
            Debug.LogError($"Ghost {gameObject.name} does not have a valid InputActionAsset set!");
            return;
        } 

        //Asks the shell for a dictionary containing action names and their co-responding function.
        var ShellActions = _Shell.GrabActions();
        string unsupportedActions = "";

        //This Loop can be simplified using LINQ. Learn it.
        foreach (var map in inputActionAsset.actionMaps)
        {
            map.Enable();
            foreach (var action in map.actions)
            {
                if(ShellActions.TryGetValue(action.name, out var result))
                {
                    action.performed += result;
                    if (action.type == InputActionType.Value) //Values need to know when we cancel the action too!
                    {
                        action.canceled += result;
                    }
                    BoundActions[action] = result;
                }

                else
                {
                    unsupportedActions += unsupportedActions.Length > 0 ? $", {action.name}" : $"{action.name}";
                }
            }
        }

        // To simplify and seperate logic, I should probably move this debug functionality to somewhere else tbh.
        if (unsupportedActions.Length > 0)
        {
            Debug.LogWarning($"Shell {_Shell.name} does not support the following actions: {unsupportedActions}");
        }
    }

    protected override void Release()
    {
        base.Release();
        
        // We also need to make sure we unbind the actions from this Shell.
        foreach (var pair in BoundActions)
        {
            pair.Key.performed -= pair.Value;

            if (pair.Key.type == InputActionType.Value)
            {
                pair.Key.canceled -= pair.Value;
            }
        }

        BoundActions.Clear();
    }

    //TODO: This is temp debug behavior.
    protected override void Start()
    {
        base.Start();
    }
}
