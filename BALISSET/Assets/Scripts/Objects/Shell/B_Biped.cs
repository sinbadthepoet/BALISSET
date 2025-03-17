using Cinemachine;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class B_Biped : B_Shell
{
    [SerializeField] float mass = 90.0f;
    [SerializeField] float headHeight = 0.8f;
    [SerializeField] float lookAngleMax = 90;

    [SerializeField] float movementSpeed = 2;
    [SerializeField] float movementAcceleration = 50;
    [SerializeField] float standingHeight = 2;
    [SerializeField] float groundDrag = 5;

    [SerializeField] float crouchedHeight = 1;
    [SerializeField] float crouchedSpeedScalar = 0.5f;
    [SerializeField] float crouchedAccelerationScalar = 0.3f;

    [SerializeField] float jumpHeight = 1;
    [SerializeField] float airAccelerationScalar = 0.05f;
    [SerializeField] float airDrag = 0;

    [SerializeField] float sprintingSpeed = 5;
    [SerializeField] float sprintingAccelerationScalar = 0.7f;
    [SerializeField] float sprintMinimumSpeed = 1.75f;
    [SerializeField] float sprintingLateralInputScalar = 0.2f;
    [SerializeField] float sprintingDrag = 5;

    [SerializeField] float groundCheckAdditionalDistance = -0.35f;

    [SerializeField] float interactionSphereCastRadius = 0.1f;
    [SerializeField] float interactionSphereCastDistance = 3.0f;

    [SerializeField] float grabSpringForceStrength = 50;
    [SerializeField] float grabTorqueForceStrength = 50;
    [SerializeField] float grabbedObjectDrag = 30;
    [SerializeField] float grabbedObjectAngularDrag = 30;
    [SerializeField] float grabAutomaticReleaseDistance = 5;
    [SerializeField] float grabbedThrowSpeed = 20;
    [SerializeField] float grabbedReleaseMaxSpeed = 20;

    [SerializeField] LayerMaskConfig layerMasks;

    [SerializeField] [HideInInspector] protected Rigidbody rb;
    [SerializeField] [HideInInspector] CapsuleCollider capsuleCollider;
    [SerializeField] [HideInInspector] Transform head;
    [SerializeField] [HideInInspector] Transform heldObjectPosition;
    [SerializeField] [HideInInspector] Transform heldWeaponViewmodelTransform;

    float headPitch;

    BipedMovementState previousMovementState;
    BipedWeaponState previousWeaponState;

    BipedMovementState currentMovementState;
    BipedWeaponState currentWeaponState;

    BipedMovementState defaultMovementState;
    BipedCrouchedState crouchedState;
    BipedFallingState fallingState;
    BipedSprintingState sprintingState;

    BipedNoWeaponState noWeaponState;
    BipedWeaponState weaponState;
    BipedAimingState aimingState;
    BipedWeaponLockedState weaponLockedState;
    BipedHoldingPropState holdingPropState;

    InputAction movementInput;
    InputAction mouseLookInput;
    InputAction analogStickLookInput;

    RaycastHit interactionCheckHit;
    protected IInteractive lookedAtInteractive;

    B_Gun heldWeapon;
    B_Gun reserveWeapon;

    #region Actions

    void BindMovement(InputAction action)
    {
        movementInput = action;
    }

    void BindMouseLook(InputAction action)
    {
        mouseLookInput = action;
    }

    void BindAnalogStickLook(InputAction action)
    {
        analogStickLookInput = action;
    }

    void Jump(InputAction.CallbackContext ctx)
    {
        currentMovementState.Jump();
    }

    void Crouch(InputAction.CallbackContext ctx)
    {
        currentMovementState.Crouch();
    }

    void SwapWeapons(InputAction.CallbackContext ctx)
    {
        currentWeaponState.SwapWeapons();
    }

    void Reload(InputAction.CallbackContext ctx)
    {
        currentWeaponState.Reload();
    }

    void Interact(InputAction.CallbackContext ctx)
    {
        currentWeaponState.Interact();
    }

    void Sprint(InputAction.CallbackContext ctx)
    {
        currentMovementState.Sprint();
    }

    void Fire(InputAction.CallbackContext ctx)
    {
        currentWeaponState.Fire(ctx);
    }

    void Aim(InputAction.CallbackContext ctx)
    {
        currentWeaponState.Aim(ctx);
    }

    #endregion

    #region Interactions

    public void GrabPhysicsProp(Rigidbody Prop)
    {
        ChangeWeaponState(holdingPropState);
        holdingPropState.GrabObject(Prop);
    }

    public void PickUpWeapon(B_Gun Gun)
    {
        //TODO: Duplicate Weapons

        //No Gun
        if (heldWeapon == null)
        {
            heldWeapon = Gun;
        }

        // One Gun
        else if (reserveWeapon == null)
        {
            reserveWeapon = Gun;
            currentWeaponState.SwapWeapons();
        }

        //Two Guns
        else
        {
            DropWeapon();
            heldWeapon = Gun;
        }

        Gun.transform.parent = heldWeaponViewmodelTransform;

        Gun.transform.localPosition = Vector3.zero;
        Gun.transform.localRotation = Quaternion.identity;

        Gun.rb.isKinematic = true;
        foreach (Collider collider in Gun.Colliders)
        {
            collider.enabled = false;
        }

        if (!heldWeapon.GetRoundChambered())
        {
            heldWeapon.ChargeGun();
        }
    }

    #endregion

    #region Internal Functions

    void InitializeStates()
    {
        defaultMovementState = new(this);
        crouchedState = new(this);
        fallingState = new(this);
        sprintingState = new(this);

        noWeaponState = new(this);
        weaponState = new(this);
        aimingState = new(this);
        weaponLockedState = new(this);
        holdingPropState = new(this);

        currentMovementState = defaultMovementState;
        currentWeaponState = noWeaponState;

        currentMovementState.EnterState();
        currentWeaponState.EnterState();
    }

    protected override void InitializeActions()
    {
        base.InitializeActions();

        ShellActions.Add("Movement", new ActionForBinding(null, BindMovement, ActionType.Values));
        ShellActions.Add("Mouse Look", new ActionForBinding(null, BindMouseLook, ActionType.Values));
        ShellActions.Add("Stick Look", new ActionForBinding(null, BindAnalogStickLook, ActionType.Values));

        ShellActions.Add("Jump", new ActionForBinding(Jump, null, ActionType.Button));
        ShellActions.Add("Crouch", new ActionForBinding(Crouch, null, ActionType.Button));
        ShellActions.Add("Swap Weapons", new ActionForBinding(SwapWeapons, null, ActionType.Button));
        ShellActions.Add("Reload", new ActionForBinding(Reload, null, ActionType.Button));
        ShellActions.Add("Interact", new ActionForBinding(Interact, null, ActionType.Button));
        ShellActions.Add("Sprint", new ActionForBinding(Sprint, null, ActionType.Button));
        ShellActions.Add("Fire", new ActionForBinding(Fire, null, ActionType.OnOff));
        ShellActions.Add("Aim", new ActionForBinding(Aim, null, ActionType.OnOff));
    }

    void ChangeMovementState(BipedMovementState state)
    {
        currentMovementState.ExitState();
        previousMovementState = currentMovementState;
        currentMovementState = state;
        currentMovementState.EnterState();
    }

    void ChangeWeaponState(BipedWeaponState state)
    {
        currentWeaponState.ExitState();
        previousWeaponState = currentWeaponState;
        currentWeaponState = state;
        currentWeaponState.EnterState();
    }

    bool GroundCheck()
    {
        return Physics.SphereCast(transform.position, capsuleCollider.radius * 0.8f, transform.TransformDirection(Vector3.down), out _, capsuleCollider.height / 2 + groundCheckAdditionalDistance);
    }
    
    void DropWeapon()
    {
        heldWeapon.transform.parent = heldWeapon.OriginalTransform;
        heldWeapon.transform.position = head.position + head.forward * 0.5f;
        heldWeapon.rb.isKinematic = false;
        foreach (Collider collider in heldWeapon.Colliders)
        {
            collider.enabled = true;
        }

        heldWeapon.rb.AddForce(head.transform.forward * 500);
    }

    #endregion

    #region Unity Events

    protected override void Awake()
    {
        base.Awake();
        CameraHolder = head;
        InitializeStates();
    }

    void Start()
    {

    }

    protected virtual void Update()
    {
        currentMovementState.Update();
        currentWeaponState.Update();

        currentMovementState.Look();
    }

    protected virtual void FixedUpdate()
    {
        currentMovementState.FixedUpdate();
        currentWeaponState.FixedUpdate();

        currentMovementState.Move();
        currentWeaponState.InteractionCheck();
    }

    protected virtual void Reset()
    {
        rb = GetComponent<Rigidbody>();

        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        capsuleCollider = GetComponent<CapsuleCollider>();

        head = transform.Find("Head");
        if (head == null)
        {
            head = new GameObject("Head").transform;
        }
        head.parent = transform;
        head.localPosition = new Vector3(0, headHeight, 0);

        heldWeaponViewmodelTransform = head.Find("Gun Position");
        if (heldWeaponViewmodelTransform == null)
        {
            heldWeaponViewmodelTransform = new GameObject("Gun Position").transform;
        }
        heldWeaponViewmodelTransform.parent = head;
        heldWeaponViewmodelTransform.localPosition = new Vector3(0.36f, -0.21f, 0.75f);

        heldObjectPosition = head.Find("Physics Grab Position");
        if (heldObjectPosition == null)
        {
            heldObjectPosition = new GameObject("Physics Grab Position").transform;
            heldObjectPosition.localPosition = new Vector3(0, -0.8f, 1.5f);
        }
        heldObjectPosition.parent = head;
    }

    void OnValidate()
    {
        head.localPosition = new Vector3(0, headHeight, 0);
        rb.mass = mass;
    }

    #endregion

    #region State Definitions

    class BipedMovementState
    {
        protected B_Biped biped;

        public float movementSpeed;
        public float movementAcceleration;
        public float movementForce;

        public float bipedHeight;
        public float groundDrag;

        public float sprintMinimumSpeed;
        public float jumpVelocity;

        public BipedMovementState(B_Biped Biped)
        {
            biped = Biped;

            movementSpeed = biped.movementSpeed;
            movementAcceleration = biped.movementAcceleration;
            CalculateMovementForce();

            bipedHeight = biped.standingHeight;
            groundDrag = biped.groundDrag;

            sprintMinimumSpeed = biped.sprintMinimumSpeed;
            jumpVelocity = MathF.Sqrt(-2 * Physics.gravity.y * biped.jumpHeight);
        }

        public virtual void EnterState()
        {
            biped.rb.drag = groundDrag;
            biped.capsuleCollider.height = bipedHeight;
        }

        public virtual void ExitState() {}

        public virtual void Update() {}

        public virtual void FixedUpdate()
        {
            if (!biped.GroundCheck())
            {
                biped.ChangeMovementState(biped.fallingState);
            }
        }

        //INTERNAL

        void CalculateMovementForce()
        {
            movementForce = biped.rb.mass * movementAcceleration;
        }

        public float GetMovementAcceleration() { return movementAcceleration; }
        public void SetMovementAcceleration(float Acceleration)
        {
            movementAcceleration = Acceleration;
            CalculateMovementForce();
        }

        //ACTIONS

        public virtual void Move()
        {
            Vector2 Input = biped.movementInput.ReadValue<Vector2>();
            Vector3 MovementForce = new Vector3(Input.x, 0, Input.y) * movementForce;

            biped.rb.AddRelativeForce(MovementForce, ForceMode.Force);
            SpeedCap();
        }

        protected virtual void SpeedCap()
        {
            var BipedVelocity = biped.rb.velocity;
            Vector3 PlanarVelocity = new Vector3(BipedVelocity.x, 0, BipedVelocity.z);
            if (PlanarVelocity.magnitude > movementSpeed)
            {
                PlanarVelocity = PlanarVelocity.normalized * movementSpeed;
                Vector3 CappedVelocity = new Vector3(PlanarVelocity.x, BipedVelocity.y, PlanarVelocity.z);
                biped.rb.velocity = CappedVelocity;
            }
        }

        public virtual void Look()
        {
            //TODO: Figure out Sensitivity Scaling
            Vector2 Input = biped.mouseLookInput.ReadValue<Vector2>();
            Input += biped.analogStickLookInput.ReadValue<Vector2>() * Time.deltaTime;

            Quaternion HorizontalElement = Quaternion.Euler(0, Input.x, 0);
            biped.rb.MoveRotation(biped.transform.rotation * HorizontalElement);

            biped.headPitch -= Input.y;
            biped.headPitch = Mathf.Clamp(biped.headPitch, -biped.lookAngleMax, biped.lookAngleMax);
            biped.head.localRotation = Quaternion.Euler(biped.headPitch, 0, 0);
        }

        public virtual void Jump()
        {
            biped.rb.AddRelativeForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
        }

        public virtual void Crouch()
        {
            biped.ChangeMovementState(biped.crouchedState);
        }

        public virtual void Sprint()
        {
            var localVelocity = biped.transform.InverseTransformDirection(biped.rb.velocity);
            if(localVelocity.z > sprintMinimumSpeed)
            {
                biped.ChangeMovementState(biped.sprintingState);
            }
        }
    }

    class BipedCrouchedState : BipedMovementState
    {
        float crouchedHeight;

        public BipedCrouchedState(B_Biped biped) : base(biped)
        {
            movementSpeed *= biped.crouchedSpeedScalar;
            SetMovementAcceleration(GetMovementAcceleration() * biped.crouchedAccelerationScalar);
            crouchedHeight = biped.crouchedHeight;
        }

        public override void EnterState()
        {
            biped.capsuleCollider.height = crouchedHeight;
            biped.transform.position -= new Vector3(0, 0.5f, 0);
        }

        public override void ExitState()
        {
            biped.capsuleCollider.height = bipedHeight;
        }

        public override void Crouch()
        {
            biped.ChangeMovementState(biped.defaultMovementState);
        }
    }

    class BipedFallingState : BipedMovementState
    {
        float airDrag;

        public BipedFallingState(B_Biped biped) : base(biped)
        {
            SetMovementAcceleration(GetMovementAcceleration() * biped.airAccelerationScalar);
            airDrag = biped.airDrag;
        }

        public override void FixedUpdate()
        {
            if (biped.GroundCheck())
            {
                biped.ChangeMovementState(biped.previousMovementState);
            }
        }

        public override void EnterState()
        {
            biped.rb.drag = airDrag;
        }

        public override void ExitState()
        {
            biped.rb.drag = groundDrag;
        }

        public override void Crouch() {}

        public override void Sprint() {}

        public override void Jump() {}

        protected override void SpeedCap() {}
    }

    class BipedSprintingState : BipedMovementState
    {
        public BipedSprintingState(B_Biped biped) : base(biped)
        {
            movementSpeed = biped.sprintingSpeed;
            SetMovementAcceleration(GetMovementAcceleration() * biped.sprintingAccelerationScalar);
        }

        public override void EnterState()
        {
            biped.rb.drag = biped.sprintingDrag;
        }

        public override void ExitState()
        {
            biped.rb.drag = groundDrag;
        }

        public override void Update()
        {
            base.Update();

            var localVelocity = biped.transform.InverseTransformDirection(biped.rb.velocity);
            if (localVelocity.z < sprintMinimumSpeed)
            {
                biped.ChangeMovementState(biped.defaultMovementState);
            }
        }

        public override void Move()
        {
            Vector2 Input = biped.movementInput.ReadValue<Vector2>();
            Input.Scale(new Vector2(biped.sprintingLateralInputScalar, 1));

            Vector3 MovementForce = new Vector3(Input.x, 0, Input.y) * movementForce;

            biped.rb.AddRelativeForce(MovementForce, ForceMode.Force);
            SpeedCap();
        }

        public override void Sprint()
        {
            biped.ChangeMovementState(biped.defaultMovementState);
        }

    }

    // WEAPON STATES //
    class BipedWeaponState
    {
        protected B_Biped biped;

        float interactionSphereCastRadius;
        float interactionSphereCastDistance;

        public BipedWeaponState(B_Biped Biped)
        {
            biped = Biped;

            interactionSphereCastRadius = biped.interactionSphereCastRadius;
            interactionSphereCastDistance = biped.interactionSphereCastDistance;
        }

        public virtual void EnterState()
        {

        }

        public virtual void ExitState()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void FixedUpdate()
        {

        }

        public virtual void Interact()
        {
            biped.lookedAtInteractive?.Interact(biped);
        }

        public virtual void InteractionCheck()
        {
            bool Hit = Physics.SphereCast(biped.head.position, interactionSphereCastRadius, biped.head.forward, out biped.interactionCheckHit, interactionSphereCastDistance, biped.layerMasks.interactive);

            if (!Hit)
            {
                biped.lookedAtInteractive = null;
                return;
            }

            var Interactive = biped.interactionCheckHit.collider.GetComponent<IInteractive>();
            if(Interactive == null)
            {
                biped.lookedAtInteractive = null;
                return;
            }

            biped.lookedAtInteractive = Interactive;
        }

        public virtual void Fire(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                biped.heldWeapon.PullTrigger();
            }
            else
            {
                biped.heldWeapon.ReleaseTrigger();
            }
        }

        public virtual void SwapWeapons()
        {
            if(biped.reserveWeapon == null) { return; }

            (biped.heldWeapon, biped.reserveWeapon) = (biped.reserveWeapon, biped.heldWeapon);
            biped.heldWeapon.gameObject.SetActive(true);
            biped.reserveWeapon.gameObject.SetActive(false);
        }

        public virtual void Reload()
        {
            biped.heldWeapon.Reload();
        }

        public virtual void Aim(InputAction.CallbackContext ctx)
        {

        }
    }

    class BipedNoWeaponState : BipedWeaponState
    {
        public BipedNoWeaponState(B_Biped Biped) : base(Biped)
        {
        }

        public override void Fire(InputAction.CallbackContext ctx) {}

        public override void SwapWeapons() {}

        public override void Reload() {}

        public override void Aim(InputAction.CallbackContext ctx) {}
    }

    class BipedAimingState : BipedWeaponState
    {
        public BipedAimingState(B_Biped Biped) : base(Biped)
        {
        }
    }

    class BipedWeaponLockedState : BipedWeaponState
    {
        public BipedWeaponLockedState(B_Biped Biped) : base(Biped)
        {

        }
    }

    class BipedHoldingPropState : BipedWeaponState
    {
        //hehe... propstate

        Rigidbody HeldObject;
        float OriginalDrag;
        float OriginalAngularDrag;

        float SpringForceStrength;
        float TorqueForceStrength;
        float NewDrag;
        float NewAngularDrag;
        float AutomaticReleaseDistance;
        float ThrowSpeed;
        float MaxLetGoSpeed;

        public BipedHoldingPropState(B_Biped biped) : base(biped)
        {
            SpringForceStrength = biped.grabSpringForceStrength;
            TorqueForceStrength = biped.grabTorqueForceStrength;
            NewDrag = biped.grabbedObjectDrag;
            NewAngularDrag = biped.grabbedObjectAngularDrag;
            AutomaticReleaseDistance = biped.grabAutomaticReleaseDistance;
            ThrowSpeed = biped.grabbedThrowSpeed;
            MaxLetGoSpeed = biped.grabbedReleaseMaxSpeed;
        }

        public override void EnterState()
        {
            base.EnterState();
            biped.lookedAtInteractive = null;
        }

        public override void FixedUpdate()
        {
            HoldObject();
        }

        public override void Fire(InputAction.CallbackContext context)
        {
            HeldObject.AddForce(biped.head.transform.forward * ThrowSpeed, ForceMode.VelocityChange);
            biped.ChangeWeaponState(biped.previousWeaponState);
        }

        public void GrabObject(Rigidbody PhysicsProp)
        {
            HeldObject = PhysicsProp;
            HeldObject.useGravity = false;
            HeldObject.interpolation = RigidbodyInterpolation.Interpolate;

            OriginalDrag = HeldObject.drag;
            HeldObject.drag = NewDrag;

            OriginalAngularDrag = HeldObject.angularDrag;
            HeldObject.angularDrag = NewAngularDrag;
        }

        public override void ExitState()
        {
            HeldObject.useGravity = true;

            HeldObject.drag = OriginalDrag;
            OriginalDrag = 1;

            HeldObject.angularDrag = OriginalAngularDrag;
            OriginalAngularDrag = 0.05f;

            HeldObject = null;
        }

        void HoldObject()
        {
            Vector3 DesiredPosition = biped.heldObjectPosition.position;
            Quaternion DesiredRotation = biped.head.transform.rotation * Quaternion.identity; //Local Identity to World Space Quaternion.

            if (Vector3.Distance(DesiredPosition, HeldObject.position) > AutomaticReleaseDistance)
            {
                biped.ChangeWeaponState(biped.previousWeaponState);
                return;
            }

            Vector3 Force = DesiredPosition - HeldObject.transform.position;
            Force *= SpringForceStrength;

            Quaternion DeltaRotation = DesiredRotation * Quaternion.Inverse(HeldObject.transform.rotation);
            Vector3 Torque = new Vector3(DeltaRotation.x, DeltaRotation.y, DeltaRotation.z) * TorqueForceStrength;

            HeldObject.AddForce(Force, ForceMode.VelocityChange);
            HeldObject.AddTorque(Torque, ForceMode.VelocityChange);
        }

        public override void Interact()
        {
            DropObject();
        }

        public override void SwapWeapons()
        {
            DropObject();
            //biped.currentWeaponState.SwapWeapons();
        }
        
        void DropObject()
        {
            var prop = HeldObject;
            biped.ChangeWeaponState(biped.previousWeaponState);
            prop.velocity = Vector3.ClampMagnitude(prop.velocity, MaxLetGoSpeed);
        }

        public override void Reload() {}
        public override void InteractionCheck() {}
    }

    #endregion
}