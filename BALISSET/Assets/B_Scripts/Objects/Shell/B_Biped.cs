using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        /// <summary>
        /// Target Speed of the player.
        /// </summary>
        protected float MovementSpeed = 2.0f;

        /// <summary>
        /// Maximum Acceleration of the player. F=ma
        /// </summary>
        protected void SetMovementAcceleration(float Acceleration)
        {
            MovementAcceleration = Acceleration;
            CalculateMovementForce();
        }
        protected float GetMovementAcceleration() { return MovementAcceleration; }
        private float MovementAcceleration = 100.0f;

        protected float GroundDrag = 5.0f;
        
        protected virtual void CalculateMovementForce()
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

        //// INTERACTION ////
        
        protected float InteractionSphereCastRadius = 0.3f;
        protected float InteractioNSphereCastMaxDistance = 3.0f;

        public virtual void Update()
        {
            if (!biped.GroundCheck())
            {
                biped.ChangeState(biped._FallingState);
            }
        }

        public virtual void FixedUpdate()
        {
            InteractionCheck();
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
            if(biped.LookedAtInteractive != null)
            {
                biped.LookedAtInteractive.Interact(biped);
            }
        }

        public virtual void InteractionCheck()
        {
            int LayersMask = Layers.GetLayerMask(Layers.Interactive, Layers.Environment);
            bool hit = Physics.SphereCast(biped._head.position, InteractionSphereCastRadius, biped._head.transform.forward, out biped.InteractionCheckHit, InteractioNSphereCastMaxDistance, LayersMask);

            if (hit)
            {
                var interactive = biped.InteractionCheckHit.collider.gameObject.GetComponent<B_Interactive>();
                if (interactive != null)
                {
                    if (interactive != biped.LookedAtInteractive)
                    {
                        biped.LookedAtInteractive = interactive;
                        biped.InteractionString.Value = interactive.GetInteractionString();
                    }
                    return;
                }
            }

            biped.LookedAtInteractive = null;
            biped.InteractionString.Value = "";
        }

        public virtual void Fire(InputAction.CallbackContext context)
        {
            if (biped.HeldWeapon == null) { return; }
            if (context.performed)
            {
                biped.HeldWeapon.PullTrigger();
            }
            else
            {
                biped.HeldWeapon.ReleaseTrigger();
            }
        }

        public virtual void SwapWeapons()
        {
            //Maybe we need a separate state for weapons

            if(biped.ReserveWeapon == null || biped.HeldWeapon == null) { return; }
            (biped.HeldWeapon, biped.ReserveWeapon) = (biped.ReserveWeapon, biped.HeldWeapon);
            biped.HeldWeapon.gameObject.SetActive(true);
            biped.ReserveWeapon.gameObject.SetActive(false);
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

    private class BipedHoldingPropState : BipedDefaultState
    {
        //hehe... propstate
        
        Rigidbody HeldObject;
        float OriginalDrag;
        float OriginalAngularDrag;

        //The position of the held object relative to the first person view.
        Vector3 GrabbedObjectPosition;

        float SpringForceStrength;
        float TorqueForceStrength;
        float NewDrag;
        float NewAngularDrag;
        float AutomaticReleaseDistance;
        float ThrowForce;
        float MaxLetGoSpeed;

        public BipedHoldingPropState(B_Biped biped) : base(biped)
        {
            MovementSpeed *= 0.8f;
            SetMovementAcceleration(GetMovementAcceleration() * 0.5f);

            GrabbedObjectPosition = new Vector3(0, -0.5f, 5);
            SpringForceStrength = 50.0f;
            TorqueForceStrength = 50.0f;
            NewDrag = 30.0f;
            NewAngularDrag = 30.0f;
            AutomaticReleaseDistance = 5.0f;
            ThrowForce = 20.0f;
            MaxLetGoSpeed = 20.0f;
        }
        
        public override void FixedUpdate()
        {
            HoldObject();
        }

        public override void Sprint() {}

        public override void Interact()
        {
            biped.ChangeState(biped._DefaultState);
        }

        public override void SwapWeapons()
        {
            biped.ChangeState(biped._DefaultState);
        }

        public override void InteractionCheck() {}

        public override void Fire(InputAction.CallbackContext context)
        {
            HeldObject.AddForce(biped._head.transform.forward * ThrowForce, ForceMode.VelocityChange);
            biped.ChangeState(biped._DefaultState);
        }

        /// <summary>
        /// Called on entry to the state. Sets object to be held.
        /// </summary>
        /// <param name="PhysicsProp"></param>
        public void GrabObject(Rigidbody PhysicsProp)
        {
            HeldObject = PhysicsProp;
            HeldObject.useGravity = false;

            //HeldObject.constraints = RigidbodyConstraints.FreezeRotation;

            OriginalDrag = HeldObject.drag;
            HeldObject.drag = NewDrag;

            OriginalAngularDrag = HeldObject.angularDrag;
            HeldObject.angularDrag = NewAngularDrag;
        }

        public override void ExitState()
        {
            HeldObject.useGravity = true;
            //HeldObject.constraints = RigidbodyConstraints.None;
            
            HeldObject.drag = OriginalDrag;
            OriginalDrag = 1;

            HeldObject.angularDrag = OriginalAngularDrag;
            OriginalAngularDrag = 0.05f;

            HeldObject.velocity = Vector3.ClampMagnitude(HeldObject.velocity, MaxLetGoSpeed);

            HeldObject = null;
        }

        /// <summary>
        /// Gravity Gun Update Code. Currently not Physics Based, but would love for it to be eventually.
        /// </summary>
        void HoldObject()
        {
            Vector3 DesiredPosition = biped._head.transform.position + biped._head.transform.TransformDirection(GrabbedObjectPosition);
            Quaternion DesiredRotation = biped._head.transform.rotation * Quaternion.identity; //Local Identity to World Space Quaternion.

            if (Vector3.Distance(DesiredPosition, HeldObject.position) > AutomaticReleaseDistance)
            {
                biped.ChangeState(biped._DefaultState);
                return;
            }

            Vector3 Force = DesiredPosition - HeldObject.transform.position;
            Force *= SpringForceStrength;

            Quaternion DeltaRotation = DesiredRotation * Quaternion.Inverse(HeldObject.transform.rotation);
            Vector3 Torque = new Vector3(DeltaRotation.x, DeltaRotation.y, DeltaRotation.z) * TorqueForceStrength;

            HeldObject.AddForce(Force, ForceMode.VelocityChange);
            HeldObject.AddTorque(Torque, ForceMode.VelocityChange);
        }
    }

    #region Fields

    BipedDefaultState _CurrentState;

    //Create state objects for each state, and make sure to initialize them.
    BipedDefaultState _DefaultState;
    BipedCrouchedState _CrouchedState;
    BipedFallingState _FallingState;
    BipedSprintingState _SprintingState;
    BipedHoldingPropState _HoldingPropState;

    #endregion

    #region Functions

    void InitializeStates()
    {
        _DefaultState = new BipedDefaultState(this);
        _CrouchedState = new BipedCrouchedState(this);
        _FallingState = new BipedFallingState(this);
        _SprintingState = new BipedSprintingState(this);
        _HoldingPropState = new BipedHoldingPropState(this);

        _CurrentState = _DefaultState;
        _CurrentState.EnterState();
    }

    void ChangeState(BipedDefaultState newState)
    {
        _CurrentState.ExitState();
        _CurrentState = newState;
        _CurrentState.EnterState();

        //Debug.Log($"Now Entering {_CurrentState.GetType()}");
    }

    #endregion

    #endregion

    #region Fields
    //Currently Using SerializeField, Create Custom Editors Once Biped is complete.

    [SerializeField] Rigidbody _rb;
    [SerializeField] CapsuleCollider _capsuleCollider;
    [SerializeField] public Transform _head;

    [SerializeField] float _headHeight = 0.8f;
    [SerializeField] float GroundCheckAdditionalDistance = 0.1f;

    [SerializeField] AlertStringVariable InteractionString;
    B_Interactive LookedAtInteractive;

    float JumpTimer;

    RaycastHit InteractionCheckHit;
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

    protected virtual void Fire(InputAction.CallbackContext context)
    {
        _CurrentState.Fire(context);
    }

    protected virtual void SwapWeapons(InputAction.CallbackContext context)
    {
        _CurrentState.SwapWeapons();
    }

    #endregion

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

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        _CurrentState.FixedUpdate();
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

    protected override void InitializeActions()
    {
        base.InitializeActions();
        ShellActions.Add("Jump", Jump);
        ShellActions.Add("Crouch", Crouch);
        ShellActions.Add("Sprint", Sprint);
        ShellActions.Add("Interact", Interact);
        ShellActions.Add("Fire", Fire);
        ShellActions.Add("Swap Weapons", SwapWeapons);
    }

    public void GrabObject(Rigidbody PhysicsProp)
    {
        ChangeState(_HoldingPropState);
        _HoldingPropState.GrabObject(PhysicsProp);
    }

    B_Gun HeldWeapon;
    B_Gun ReserveWeapon;
    [SerializeField] Transform HeldWeaponViewmodelTransform;

    public void PickUpWeapon(B_Gun Gun)
    {
        //TODO: Duplicate Weapons

        //No Gun
        if(HeldWeapon == null)
        {
            HeldWeapon = Gun;
        }

        // One Gun
        else if(ReserveWeapon == null)
        {
            ReserveWeapon = Gun;
            _CurrentState.SwapWeapons();
        }

        //Two Guns
        else
        {
            DropWeapon();
            HeldWeapon = Gun;
        }

        Gun.transform.parent = HeldWeaponViewmodelTransform;

        Gun.transform.localPosition = Vector3.zero;
        Gun.transform.localRotation = Quaternion.identity;

        Gun.rb.isKinematic = true;
        foreach(Collider collider in Gun.Colliders)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// Drops currently held weapon. Does not swap weapons after drop.
    /// For that, use SwapWeapon();
    /// </summary>
    void DropWeapon()
    {
        HeldWeapon.transform.parent = HeldWeapon.OriginalTransform;
        HeldWeapon.transform.position = _head.position + _head.forward * 0.5f;
        HeldWeapon.rb.isKinematic = false;
        foreach(Collider collider in HeldWeapon.Colliders)
        {
            collider.enabled = true;
        }

        HeldWeapon.rb.AddForce(_head.transform.forward * 500);
    }

    /*
    B_Gun CurrentWeapon;
    B_Gun ReserveWeapon;

    [SerializeField] Transform GunHeldPosition;

    void SwapWeapons()
    {
        var Holder = CurrentWeapon;
        CurrentWeapon = ReserveWeapon;
        ReserveWeapon = Holder;

        //NULL REF EXCEPTION
        CurrentWeapon.gameObject.SetActive(true);
        ReserveWeapon.gameObject.SetActive(false);
    }

    void DropWeapon()
    {

    }

    public void PickUpWeapon(B_Gun Gun)
    {
        if (ReserveWeapon == null)
        {
            SwapWeapons();
        }

        if (CurrentWeapon != null)
        {
            DropWeapon();
        }

        CurrentWeapon = Gun;

        Gun.transform.parent = GunHeldPosition;

        Gun.transform.position = Vector3.zero;
        Gun.transform.rotation = Quaternion.identity;

        var GunColliders = Gun.GetComponents<Collider>();

        foreach(Collider collider in GunColliders)
        {
            collider.enabled = false;
        }
    }
    */
}
