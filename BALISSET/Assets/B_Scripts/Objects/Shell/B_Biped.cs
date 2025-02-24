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
    #region States

    private class BipedDefaultState
    {
        protected B_Biped biped;

        public BipedDefaultState(B_Biped biped)
        {
            this.biped = biped;
            CalculateMovementForce();
        }

        //// STATE CHANGE ////
        public virtual void EnterState()
        {
            biped._rb.drag = GroundDrag;
            biped._capsuleCollider.height = PlayerHeight;
        }

        public virtual void ExitState(){}

        //// MOVEMENT ////

        protected float MovementSpeed = 2.0f;

        protected void SetMovementAcceleration(float Acceleration)
        {
            MovementAcceleration = Acceleration;
            CalculateMovementForce();
        }
        protected float GetMovementAcceleration() { return MovementAcceleration; }
        private float MovementAcceleration = 100.0f;

        protected float GroundDrag = 5.0f;
        
        private void CalculateMovementForce()
        {
            _movementForce = biped._rb.mass * MovementAcceleration;
        }
        protected float _movementForce;

        //// JUMP ////

        protected float JumpForce = 500.0f;
        protected float JumpDelay = 0.1f;

        //// CROUCH ////

        protected float PlayerHeight = 2.0f;

        //// SPRINT ////

        protected float SprintMinimumSpeed = 1.5f;

        public virtual void Update()
        {
            if (!biped.GroundCheck())
            {
                biped.ChangeState(biped._FallingState);
            }
        }

        public virtual void Move(Vector2 input)
        {
            //Create the desired force vector in local space.
            Vector3 MovementForce = new Vector3(input.x, 0, input.y) * _movementForce;

            //Add force in local space.
            biped._rb.AddRelativeForce(MovementForce, ForceMode.Force);

            //Check the horizontal plane's velocity and limit it to the movement speed if it's too fast.
            //May have some issues.
            //Crouching may kill velocity in a way that feels bad. Let's see.
            Vector3 PlanarVelocity = new Vector3(biped._rb.velocity.x, 0, biped._rb.velocity.z);

            if (PlanarVelocity.magnitude > MovementSpeed)
            {
                PlanarVelocity = PlanarVelocity.normalized * MovementSpeed;
                Vector3 CappedVelocity = new Vector3(PlanarVelocity.x, biped._rb.velocity.y, PlanarVelocity.z);
                biped._rb.velocity = CappedVelocity;
            }
        }

        public virtual void Look(Vector2 input)
        {
            //Horizontal Rotation of player GameObject
            biped.transform.Rotate(Vector3.up, input.x);

            //Vertical Rotation of child Camera GameObject
            biped._verticalLook -= input.y;
            biped._verticalLook = Mathf.Clamp(biped._verticalLook, -biped.LookAngleClamp, biped.LookAngleClamp);
            biped._head.transform.localRotation = Quaternion.Euler(biped._verticalLook, 0, 0);

            if (biped._vCamAim != null)
            {
                //var aim = (CinemachinePOV)_vCamAim;
                //aim.m_VerticalAxis.Value = _verticalLook;
            }
        }

        public virtual void Jump()
        {
            if(biped.JumpTimer > JumpDelay)
            {
                biped._rb.AddRelativeForce(Vector3.up * JumpForce, ForceMode.Impulse);
                biped.JumpTimer = 0;
            }
        }
        
        public virtual void Crouch()
        {
            biped.ChangeState(biped._CrouchedState);
        }

        public virtual void Sprint()
        {
            if (biped._rb.velocity.magnitude > SprintMinimumSpeed)
            {
                biped.ChangeState(biped._SprintingState);
            }
        }

        public virtual void Interact()
        {
            throw new System.NotImplementedException();
        }
    }

    private class BipedCrouchedState : BipedDefaultState
    {
        //Keep an eye on jumping from crouch, might be whacky.

        float CrouchedHeight;

        //TODO: Temp
        float FallCheckTimer;

        public BipedCrouchedState(B_Biped biped) : base(biped)
        {
            MovementSpeed *= 0.5f;
            SetMovementAcceleration(GetMovementAcceleration() * 0.3f);
            CrouchedHeight = PlayerHeight * 0.5f;
        }

        public override void EnterState()
        {
            biped._capsuleCollider.height = CrouchedHeight;

            // Move the player down by half the difference in height between states.
            // Capsule Height changes from the center(?)
            //biped.transform.Translate(Vector3.down * (PlayerHeight - CrouchedHeight) / 2);

            //TODO: NOT WORKING
            FallCheckTimer = 0;
        }

        public override void ExitState()
        {
            //biped.transform.Translate(Vector3.up * (PlayerHeight - CrouchedHeight) / 2);
        }

        public override void Crouch()
        {
            biped.ChangeState(biped._DefaultState);
        }

        public override void Update()
        {
            //TODO: This is temp behavior
            if (FallCheckTimer > 1)
            {
                if (!biped.GroundCheck())
                {
                    biped.ChangeState(biped._FallingState);
                }
            }
            FallCheckTimer += Time.deltaTime;
        }
    }

    private class BipedFallingState : BipedDefaultState
    {
        float AirDrag;

        public BipedFallingState(B_Biped biped): base(biped)
        {
            SetMovementAcceleration(GetMovementAcceleration() * 0.1f);
            AirDrag = 0.0f;
        }

        public override void Update()
        {
            if (biped.GroundCheck())
            {
                biped.ChangeState(biped._DefaultState);
            }
        }

        public override void EnterState()
        {
            biped._rb.drag = AirDrag;
        }

        public override void Crouch(){}

        public override void Sprint(){}

        public override void Jump(){}
    }

    private class BipedSprintingState : BipedDefaultState
    {
        public BipedSprintingState(B_Biped biped) : base(biped)
        {
            MovementSpeed += 10;
            SetMovementAcceleration(GetMovementAcceleration() * 0.7f);
        }

        public override void Update()
        {
            base.Update();

            if (biped._rb.velocity.magnitude < SprintMinimumSpeed)
            {
                biped.ChangeState(biped._DefaultState);
            }
        }

        public override void Sprint()
        {
            biped.ChangeState(biped._DefaultState);
        }
    }

    #region Fields

    BipedDefaultState _CurrentState;

    //Create state objects for each state, and make sure to initialize them.
    BipedDefaultState _DefaultState;
    BipedCrouchedState _CrouchedState;
    BipedFallingState _FallingState;
    BipedSprintingState _SprintingState;

    #endregion

    #region Functions

    void InitializeStates()
    {
        _DefaultState = new BipedDefaultState(this);
        _CrouchedState = new BipedCrouchedState(this);
        _FallingState = new BipedFallingState(this);
        _SprintingState = new BipedSprintingState(this);

        _CurrentState = _DefaultState;
        _CurrentState.EnterState();
    }

    void ChangeState(BipedDefaultState newState)
    {
        _CurrentState.ExitState();
        _CurrentState = newState;
        _CurrentState.EnterState();

        Debug.Log($"Now Entering {_CurrentState.GetType()}");
    }

    #endregion

    #endregion

    #region Fields
    //Currently Using SerializeField, Create Custom Editors Once Biped is complete.

    [SerializeField] Rigidbody _rb;
    [SerializeField] CapsuleCollider _capsuleCollider;
    [SerializeField] Transform _head;

    [SerializeField] float _headHeight = 0.8f;
    [SerializeField] float GroundCheckAdditionalDistance = 0.1f;

    float JumpTimer;

    #endregion

    #region Actions

    protected override void Move()
    {
        _CurrentState.Move(_movementInput);
    }

    protected override void Look()
    {
        _CurrentState.Look(_lookInput);
    }

    protected virtual void Jump(InputAction.CallbackContext context)
    {
        _CurrentState.Jump();
    }

    protected virtual void Crouch(InputAction.CallbackContext callback)
    {
        _CurrentState.Crouch();
    }

    protected virtual void Sprint(InputAction.CallbackContext context)
    {
        _CurrentState.Sprint();
    }

    protected virtual void Interact(InputAction.CallbackContext context)
    {
        _CurrentState.Interact();
    }

    #endregion

    bool GroundCheck()
    {
        //Maybe Divide Radius by 2?
        RaycastHit WhoCares;
        return Physics.SphereCast(transform.position, _capsuleCollider.radius, transform.TransformDirection(Vector3.down), out WhoCares, _capsuleCollider.height / 2 + GroundCheckAdditionalDistance);
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

        _vCamBody = _vCam.AddCinemachineComponent<CinemachineHardLockToTarget>();
        _vCamAim = _vCam.AddCinemachineComponent<CinemachineSameAsFollowTarget>();

        //Enable the camera
        if (_vCam != null)
        {
            _vCam.enabled = true;
        }
    }

    #region Unity Events

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
        _head.localPosition = new Vector3(0, _headHeight, 0);
    }

    protected override void Awake()
    {
        base.Awake();
        InitializeStates();
    }

    protected override void Update()
    {
        base.Update();
        _CurrentState.Update();
        JumpTimer += Time.deltaTime;
    }

    #endregion

    protected override void InitializeActions()
    {
        base.InitializeActions();
        ShellActions.Add("Jump", Jump);
        ShellActions.Add("Crouch", Crouch);
        ShellActions.Add("Sprint", Sprint);
        ShellActions.Add( "Interact", Interact);
    }
}
