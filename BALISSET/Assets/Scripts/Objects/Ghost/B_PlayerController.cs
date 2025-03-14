using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class B_PlayerController : B_Ghost
{
    #region Variables

    // TODO: This is probably not the best solution long term for different acion maps
    // Find a better solution.
    [SerializeField] InputActionAsset inputActionAsset;

    /// <summary>
    /// This stores the delegates from the shell, using the InputAction as the key. We use this for unbinding on release.
    /// </summary>
    Dictionary<InputAction, System.Action<InputAction.CallbackContext>> BoundActions = new Dictionary<InputAction, System.Action<InputAction.CallbackContext>>();

    #endregion

    #region Functions
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
        var ShellActions = _PossessedShell.GrabActions();
        var unsupportedActions = new List<string>();

        foreach (var map in inputActionAsset.actionMaps)
        {
            BindActionMap(map, ShellActions, unsupportedActions);
        }

        //Log unsupported actions
        if (unsupportedActions.Count > 0)
        {
            Debug.LogWarning($"Shell {_PossessedShell.name} does not support the following actions: {string.Join(", ", unsupportedActions)}");
        }
    }

    /// <summary>
    /// Binds actions from an InputActionMap with the shell's action delegates.
    /// </summary>
    void BindActionMap(InputActionMap map, Dictionary<string, Action<InputAction.CallbackContext>> shellActions, List<string> unsupportedActions)
    {
        map.Enable();
        foreach (var action in map.actions)
        {
            if (shellActions.TryGetValue(action.name, out var result))
            {
                action.performed += result;

                //Values need to know when we cancel the action too!
                if (action.type == InputActionType.Value)
                {
                    action.canceled += result;
                }

                BoundActions[action] = result;
            }

            else
            {
                unsupportedActions.Add(action.name);
            }
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


    //TODO: This is temp behavior.
    protected override void Start()
    {
        base.Start();
    }
    #endregion
}
