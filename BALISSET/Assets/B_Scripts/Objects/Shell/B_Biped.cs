using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class B_Biped : B_Shell
{
    #region Variables

    #region References

    [SerializeField] Rigidbody _rb;
    [SerializeField] CapsuleCollider _capsuleCollider;
    [SerializeField] Transform _head;

    #endregion

    #region Values

    [SerializeField] float GroundCheckAdditionalDistance = 0.1f;

    #endregion

    #region Character Values

    [SerializeField] float MovementSpeed = 7.0f;
    [SerializeField] float MovementAcceleration = 100.0f;
    float _movementForce;

    [SerializeField] float GroundDrag = 1.0f;
    [SerializeField] float AirDrag = 0.0f;

    [SerializeField] float AirControlScalar = 0.1f;
    [SerializeField] float CrouchedSpeedScalar = 0.75f;

    [SerializeField] float CrouchedHeight = 1.0f;
    [SerializeField] float StandingHeight = 2.0f;

    [SerializeField] float JumpForce = 500.0f;


    #endregion

    #region Character States

    bool _isCrouched;
    bool _isGrounded;

    #endregion

    #endregion

    #region Functions

    #region Built In Functions

    protected virtual void Reset()
    {
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        //Make sure the component has a head. Eventually, we can replace this with an editor feature.
        _head = transform.Find("Head");
        if (_head == null)
        {
            _head = new GameObject("Head").transform;
        }
        _head.parent = transform;
        _head.localPosition = Vector3.zero;
    }

    protected override void Awake()
    {
        base.Awake();

        //Make sure the component has a head. Eventually, we can replace this with an editor feature.
        //_head = transform.Find("Head");
    }

    protected override void Update()
    {
        base.Update();
        GroundCheck();
    }

    protected void OnValidate()
    {
        _movementForce = _rb.mass * MovementAcceleration;
    }

    #endregion
    #region Action Functions

    protected override void Look()
    {
        //Horizontal Rotation of player GameObject
        transform.Rotate(Vector3.up, _lookInput.x);

        //Vertical Rotation of child Camera GameObject
        _verticalLook -= _lookInput.y;
        _verticalLook = Mathf.Clamp(_verticalLook, -LookAngleClamp, LookAngleClamp);
        _head.transform.localRotation = Quaternion.Euler(_verticalLook, 0, 0);

        if (_vCamAim != null)
        {
            //var aim = (CinemachinePOV)_vCamAim;
            //aim.m_VerticalAxis.Value = _verticalLook;
        }
    }

    protected override void Move()
    {
        //_isMoving = _movementInput.magnitude > 0;

        //Create the desired force vector in local space.
        Vector3 MovementForce = new Vector3(_movementInput.x, 0, _movementInput.y) * _movementForce;

        //Scale force based on circumstances.
        if (!_isGrounded)
        {
            MovementForce *= AirControlScalar;
        }
        else if (_isCrouched) //Air control should be regardless of whether they're crouched on not
        {
            MovementForce *= CrouchedSpeedScalar; //Eventually, we should modify the _movementForce as we switch back and forth between states.
        }

        //Add force in local space.
        _rb.AddRelativeForce(MovementForce, ForceMode.Force);

        //Check the horizontal plane's velocity and limit it to the movement speed if it's too fast.
        //This sucks because we can't shoot the player super fast out of a canon.
        Vector3 PlanarVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        if (PlanarVelocity.magnitude > MovementSpeed)
        {
            PlanarVelocity = PlanarVelocity.normalized * MovementSpeed;
            Vector3 CappedVelocity = new Vector3(PlanarVelocity.x, _rb.velocity.y, PlanarVelocity.z);
            _rb.velocity = CappedVelocity;
        }
    }

    protected virtual void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _rb.AddRelativeForce(Vector3.up * JumpForce, ForceMode.Impulse);
            SetGrounded(false);
        }
    }

    protected virtual void Crouch(InputAction.CallbackContext callback)
    {
        if (_isCrouched)
        {
            _isCrouched = false;
            _capsuleCollider.height = StandingHeight;
        }
        else
        {
            _isCrouched = true;
            _capsuleCollider.height = CrouchedHeight;
            transform.Translate(Vector3.down * CrouchedHeight / 2);
        }
    }

    protected virtual void Sprint(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void Interact(InputAction.CallbackContext context)
    {
        Debug.Log("INTERACT!");
    }

    #endregion
    #region Component Use Functions

    void GroundCheck()
    {
        //ISSUE: Standing on edge means raycast misses ground.
        bool grounding = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), _capsuleCollider.height / 2 + GroundCheckAdditionalDistance);
        SetGrounded(grounding);
    }

    void SetGrounded(bool GroundingState)
    {
        _isGrounded = GroundingState;
        _rb.drag = _isGrounded ? GroundDrag : AirDrag;
    }

    protected override void InitializeActions()
    {
        base.InitializeActions();
        ShellActions.Add("Jump", Jump);
        ShellActions.Add("Crouch", Crouch);
        ShellActions.Add("Sprint", Sprint);
        ShellActions.Add( "Interact", Interact);
    }

    protected override void BindVirtualCamera()
    {
        _vCam = GameObject.Find("Player Virtual Camera").GetComponent<CinemachineVirtualCamera>();

        //If no virtual camera can be found, we can make one for now.
        if (_vCam == null)
        {
            _vCam = new GameObject("Player Virtual Camera", typeof(CinemachineVirtualCamera)).GetComponent<CinemachineVirtualCamera>();
        }

        _vCam.Follow = _head;
        //_vCam.LookAt = _head;

        _vCamBody = _vCam.AddCinemachineComponent<CinemachineHardLockToTarget>();
        _vCamAim = _vCam.AddCinemachineComponent<CinemachineSameAsFollowTarget>();

        //var aim = (CinemachinePOV)_vCamAim;
        //aim.m_VerticalAxis.m_MaxValue = 80;
        //aim.m_VerticalAxis.m_MinValue = -80;
        //aim.m_VerticalAxis.m_AccelTime = 0;
        //aim.m_VerticalAxis.m_DecelTime = 0;
        //aim.m_HorizontalAxis.m_AccelTime = 0;s
        //aim.m_HorizontalAxis.m_DecelTime = 0;

        //Enable the camera
        if (_vCam != null)
        {
            _vCam.enabled = true;
        }
    }

    #endregion

    #endregion
}
