using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class B_PlayerController : B_Ghost
{
    [SerializeField] InputActionAsset inputActionAsset;

    Dictionary<InputAction, ActionForBinding> BoundActions = new(); //For Unbinding.

    public override void Possess(B_Shell Shell)
    {
        base.Possess(Shell);

        if (inputActionAsset == null)
        {
            Debug.LogError($"Ghost {gameObject.name} does not have a valid InputActionAsset set!");
            return;
        }

        var ShellActions = _PossessedShell.ShellActions;
        var unsupportedActions = new List<string>();

        //TODO: Menu Maps might mix this up.
        foreach (var map in inputActionAsset.actionMaps)
        {
            map.Enable();

            foreach (var action in map.actions)
            {
                if (ShellActions != null && ShellActions.TryGetValue(action.name, out var actionStruct))
                {
                    switch (actionStruct.actionType)
                    {
                        case ActionType.Button:
                            action.performed += actionStruct.action;
                            break;

                        case ActionType.OnOff:
                            action.performed += actionStruct.action;
                            action.canceled += actionStruct.action;
                            break;

                        case ActionType.Values:
                            if(actionStruct.BindValue == null)
                            {
                                Debug.Log($"Attemped to bind {action.name}, but no value binding function was found.");
                                break;
                            }
                            actionStruct.BindValue.Invoke(action);
                            break;
                    }
                    BoundActions[action] = actionStruct;
                }
                else
                {
                    unsupportedActions.Add(action.name);
                }
            }
        }

        //Log unsupported actions
        if (unsupportedActions.Count > 0)
        {
            Debug.LogWarning($"Shell {_PossessedShell.name} does not support the following actions: {string.Join(", ", unsupportedActions)}");
        }
    }

    public override void Release()
    {
        base.Release();

        foreach (var pair in BoundActions)
        {
            switch (pair.Value.actionType)
            {
                case ActionType.Button:
                    pair.Key.performed -= pair.Value.action;
                    break;
                case ActionType.OnOff:
                    pair.Key.performed -= pair.Value.action;
                    pair.Key.canceled -= pair.Value.action;
                    break;
                case ActionType.Values:
                    pair.Value.BindValue(null);
                    break;
            }
        }

        BoundActions.Clear();
    }
}

public enum ActionType
{
    Button,
    OnOff,
    Values
}

public struct ActionForBinding
{
    public Action<InputAction.CallbackContext> action;
    public Action<InputAction> BindValue;
    public ActionType actionType;
}