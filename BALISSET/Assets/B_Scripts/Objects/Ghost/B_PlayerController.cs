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
        } 
        else 
        {
            var ShellActions = _Shell.GrabActions();
            string unsupportedActions = "";
            foreach (var map in inputActionAsset.actionMaps)
            {
                map.Enable();
                foreach (var action in map.actions)
                {
                    if(ShellActions.TryGetValue(action.name, out var result))
                    {
                        action.performed += result;
                        BoundActions[action] = result;
                    }
                    else
                    {
                        if (unsupportedActions.Length > 0)
                        {
                            unsupportedActions += $", {action.name}";
                        }
                        else
                        {
                            unsupportedActions += $"{action.name}";
                        }
                    }
                }
            }
            if (unsupportedActions.Length > 0)
            {
                Debug.LogWarning($"Shell {_Shell.name} does not support the following actions: {unsupportedActions}");
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

    protected override void Start()
    {
        base.Start();
    }
}
