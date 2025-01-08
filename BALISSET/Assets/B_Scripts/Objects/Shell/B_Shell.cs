using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class B_Shell : MonoBehaviour
{
    #region Base
    protected B_Ghost _Ghost;
    protected int health;

    protected Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>> ShellActions = new();

    public Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>> GrabActions()
    {
        return ShellActions;
    }

    public virtual void Possess(B_Ghost Ghost)
    {
        _Ghost = Ghost;
        if(Ghost is B_PlayerController)
        {
            BindVirtualCamera();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public virtual void Release()
    {
        _Ghost = null;
        if (_vCam != null)
        {
            //TODO: UNBIND CAMERA
        }
    }

    protected virtual void Awake()
    {
        InitializeActions();
    }

    protected virtual void Update()
    {
        Look();
    }

    /// <summary>
    /// Sets up the dictionary ShellActions, where the key of an action name holds the value of an action function delegate.
    /// </summary>
    protected virtual void InitializeActions()
    {
        ShellActions = new Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>>
        {
            { "Look", LookInput }
        };
    }
    #endregion
    #region Camera Stuff

    // If for some reason we possess a shell that doesn't have a camera set up,
    // we can use a back up camera to make sure we still have something to use.

    protected Vector2 _lookInput = Vector2.zero;
    protected float _verticalLook = 0;
    protected float LookAngleClamp = 80;

    protected CinemachineVirtualCamera _vCam;
    protected CinemachineComponentBase _vCamBody;
    protected CinemachineComponentBase _vCamAim;

    protected virtual void BindVirtualCamera()
    {
        throw new System.NotImplementedException();
    }

    protected virtual void Look()
    {
        throw new System.NotImplementedException();
    }

    protected virtual void LookInput(InputAction.CallbackContext callbackContext)
    {
        _lookInput = callbackContext.ReadValue<Vector2>();
    }

    #endregion
}
