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
    #region Ghost-Shell Behaviour

    #region Variables

    #region References

    protected B_Ghost _Ghost;

    #endregion

    #region Values

    protected Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>> ShellActions = new();
    
    #endregion

    #endregion

    #region Functions

    #region Public Functions

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

    #endregion
    #region Getters

    public Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>> GrabActions()
    {
        return ShellActions;
    }

    #endregion

    #endregion

    #endregion

    #region Damage

    //protected int health;
    //public abstract void Damage(int damage, object source);

    #endregion

    #region Actions

    #region Look

    #region Variables

    protected Vector2 _lookInput = Vector2.zero;
    protected float _verticalLook = 0;
    protected float LookAngleClamp = 80;

    protected CinemachineVirtualCamera _vCam;
    protected CinemachineComponentBase _vCamBody;
    protected CinemachineComponentBase _vCamAim;

    #endregion

    #region Functions

    protected virtual void BindVirtualCamera()
    {
        throw new System.NotImplementedException();
    }

    protected virtual void Look()
    {
        throw new System.NotImplementedException();
    }

    void LookInput(InputAction.CallbackContext callbackContext)
    {
        _lookInput = callbackContext.ReadValue<Vector2>();
    }

    #endregion

    #endregion

    #region Move

    #region Variables

    protected Vector2 _movementInput = Vector2.zero;

    #endregion

    #region Functions

    protected virtual void Move()
    {
        throw new System.NotImplementedException();
    }

    void MoveInput(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    #endregion

    #endregion

    /// <summary>
    /// Sets up the dictionary ShellActions, where the key of an action name holds the value of an action function delegate.
    /// </summary>
    protected virtual void InitializeActions()
    {
        ShellActions = new Dictionary<string, System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>>
        {
            { "Look", LookInput },
            { "Movement", MoveInput }
        };
    }
    
    #endregion

    #region Unity Events

    protected virtual void Awake()
    {
        InitializeActions();
    }

    protected virtual void Update()
    {
        Look();
    }

    protected virtual void FixedUpdate()
    {
        Move();
    }

    #endregion
}
