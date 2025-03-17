using Cinemachine;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class B_Biped : B_Shell
{
    [SerializeField] [HideInInspector] Rigidbody rb;
    [SerializeField] [HideInInspector] CapsuleCollider capsuleCollider;
    
    [SerializeField] [HideInInspector] Transform head;
    [SerializeField] float headHeight = 0.8f;

    [SerializeField] float GroundCheckAdditionalDistance = 0.1f;
    [SerializeField] LayerMaskConfig layerMasks;

    [SerializeField] [HideInInspector] Transform heldObjectPosition;
    [SerializeField] [HideInInspector] Transform heldWeaponViewmodelTransform;

    [SerializeField] float Mass = 90.0f;

    [SerializeField] float lookAngleMax = 90;
    float headPitch;

    BipedMovementState currentMovementState;
    BipedWeaponState currentWeaponState;

    BipedMovementState defaultMovementState;
    BipedCrouchedState crouchedState;
    BipedFallingState fallingState;
    BipedSprintingState sprintingState;

    BipedNoWeaponState noWeaponState;
    BipedWeaponState weaponState;
    BipedAimingState aimingState;
    BipedReloadingState reloadingState;
    BipedHoldingPropState holdingPropState;

    InputAction movementInput;
    InputAction mouseLookInput;
    InputAction analogStickLookInput;

    RaycastHit interactionCheckHit;
    protected B_Interactive lookedAtInteractive;

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
        reloadingState = new(this);
        holdingPropState = new(this);

        currentMovementState = defaultMovementState;
        currentWeaponState = noWeaponState;
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

    //TODO: Use a stack to maintain a reference to previous states
    void ChangeMovementState(BipedMovementState state)
    {
        currentMovementState.ExitState();
        currentMovementState = state;
        currentMovementState.EnterState();
    }

    void ChangeWeaponState(BipedWeaponState state)
    {
        currentWeaponState.ExitState();
        currentWeaponState = state;
        currentWeaponState.EnterState();
    }

    bool GroundCheck()
    {
        RaycastHit WhoCares;
        return Physics.SphereCast(transform.position, capsuleCollider.radius, transform.TransformDirection(Vector3.down), out WhoCares, capsuleCollider.height / 2 + GroundCheckAdditionalDistance);
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

        rb.mass = Mass;
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
        }
        heldObjectPosition.parent = head;
    }

    void OnValidate()
    {
        head.localPosition = new Vector3(0, headHeight, 0);
        rb.mass = Mass;
    }

    #endregion

    #region State Definitions

    class BipedMovementState
    {
        protected B_Biped biped;

        public float groundDrag = 5;
        public float bipedHeight = 2;

        public float movementSpeed = 2;
        public float movementAcceleration = 100;
        public float movementForce;

        public float jumpForce = 500;

        public float sprintMinimumSpeed = 1;

        public BipedMovementState(B_Biped Biped)
        {
            biped = Biped;
            CalculateMovementForce();
        }

        public virtual void EnterState() {}

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
        protected void SetMovementAcceleration(float Acceleration)
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

            var BipedVelocity = biped.rb.velocity;

            Vector3 PlanarVelocity = new Vector3(BipedVelocity.x, 0, BipedVelocity.z);
            if(PlanarVelocity.magnitude > movementSpeed)
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
            biped.rb.AddRelativeForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
            movementSpeed *= 0.5f;
            SetMovementAcceleration(GetMovementAcceleration() * 0.3f);
            crouchedHeight = bipedHeight * 0.5f;
        }

        public override void EnterState()
        {
            biped.capsuleCollider.height = crouchedHeight;
            //Push biped Down
        }

        public override void ExitState()
        {
            biped.capsuleCollider.height = bipedHeight;
            //Push biped Up
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
            SetMovementAcceleration(GetMovementAcceleration() * 0.1f);
            airDrag = 0.0f;
        }

        public override void FixedUpdate()
        {
            if (biped.GroundCheck())
            {
                biped.ChangeMovementState(biped.defaultMovementState);
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
    }

    class BipedSprintingState : BipedMovementState
    {
        public BipedSprintingState(B_Biped biped) : base(biped)
        {
            movementSpeed += 10;
            SetMovementAcceleration(GetMovementAcceleration() * 0.7f);
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

        //TODO: Override Movement to reduce lateral forces.

        public override void Sprint()
        {
            biped.ChangeMovementState(biped.defaultMovementState);
        }

    }

    class BipedWeaponState
    {
        protected B_Biped biped;

        float interactionSphereCastRadius = 0.1f;
        float interactionSphereCastMaxDistance = 3;

        public BipedWeaponState(B_Biped Biped)
        {
            biped = Biped;
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
            biped.lookedAtInteractive.Interact(biped);
        }

        public virtual void InteractionCheck()
        {
            bool Hit = Physics.SphereCast(biped.head.position, interactionSphereCastRadius, biped.head.forward, out biped.interactionCheckHit, interactionSphereCastMaxDistance, biped.layerMasks.interactive);

            if (!Hit) { return; }

            var Interactive = biped.interactionCheckHit.collider.GetComponent<B_Interactive>();
            if(Interactive == null) { return; }

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

    class BipedReloadingState : BipedWeaponState
    {
        public BipedReloadingState(B_Biped Biped) : base(Biped)
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
        float ThrowForce;
        float MaxLetGoSpeed;

        public BipedHoldingPropState(B_Biped biped) : base(biped)
        {
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

        public override void Fire(InputAction.CallbackContext context)
        {
            HeldObject.AddForce(biped.head.transform.forward * ThrowForce, ForceMode.VelocityChange);
            //biped.ChangeWeaponState(PreviousState);
        }

        public void GrabObject(Rigidbody PhysicsProp)
        {
            HeldObject = PhysicsProp;
            HeldObject.useGravity = false;

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

            HeldObject.velocity = Vector3.ClampMagnitude(HeldObject.velocity, MaxLetGoSpeed);

            HeldObject = null;
        }

        void HoldObject()
        {
            Vector3 DesiredPosition = biped.head.transform.position + biped.head.transform.TransformDirection(biped.heldObjectPosition.position);
            Quaternion DesiredRotation = biped.head.transform.rotation * Quaternion.identity; //Local Identity to World Space Quaternion.

            if (Vector3.Distance(DesiredPosition, HeldObject.position) > AutomaticReleaseDistance)
            {
                //biped.ChangeWeaponState(PreviousState);
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
            //biped.ChangeWeaponState(PreviousState);
        }

        public override void SwapWeapons()
        {
            //biped.ChangeWeaponState(PreviousState);
            biped.currentWeaponState.SwapWeapons();
        }

        public override void Reload() {}
    }

    #endregion
}