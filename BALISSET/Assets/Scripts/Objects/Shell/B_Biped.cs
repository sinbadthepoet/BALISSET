using Cinemachine;
using Cinemachine.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class B_Biped : B_Shell
{
    [SerializeField] BipedStats stats;
    [SerializeField] LayerMaskConfig layerMasks;

    [SerializeField] [HideInInspector] protected Rigidbody rb;
    [SerializeField] [HideInInspector] CapsuleCollider capsuleCollider;
    [SerializeField] [HideInInspector] Transform head;
    [SerializeField] [HideInInspector] Transform heldObjectPosition;
    [SerializeField] [HideInInspector] Transform heldWeaponViewmodelTransform;

    float headPitch;
    List<ContactPoint> contactPoints;
    Queue<Vector3> previousVelocities;

    BipedMovementState previousMovementState;
    BipedWeaponState previousWeaponState;

    protected BipedMovementState currentMovementState;
    protected BipedWeaponState currentWeaponState;

    BipedMovementState defaultMovementState;
    BipedCrouchedState crouchedState;
    BipedFallingState fallingState;
    BipedSlippingState slippingState;
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
        slippingState = new(this);
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

    void ChangeMovementState(BipedMovementState newState)
    {
        currentMovementState.ExitState();
        previousMovementState = currentMovementState;
        currentMovementState = newState;
        currentMovementState.EnterState();
    }

    void ChangeWeaponState(BipedWeaponState newState)
    {
        currentWeaponState.ExitState();
        previousWeaponState = currentWeaponState;
        currentWeaponState = newState;
        currentWeaponState.EnterState();
    }

    bool GroundCheck()
    {
        var CapsuleBottomSphereCenter = transform.TransformPoint(capsuleCollider.center) - transform.up * capsuleCollider.height * 0.5f + transform.up * capsuleCollider.radius;
        return Physics.SphereCast(CapsuleBottomSphereCenter, capsuleCollider.radius * 0.8f, transform.TransformDirection(Vector3.down), out _, stats.groundCheckAdditionalDistance);
    }

    bool SlipCheck() //https://youtu.be/8diXkicKnaM?si=HwlLhHIoVK85EZK_&t=34
    {
        RaycastHit SlopeHit;

        if(Physics.SphereCast(transform.position, capsuleCollider.radius * 0.8f, transform.TransformDirection(Vector3.down), out SlopeHit, capsuleCollider.height / 2 + stats.groundCheckAdditionalDistance))
        {
            var angle = Vector3.Angle(Vector3.up, SlopeHit.normal);
            if (angle > stats.slopeSlipAngle)
            {
                Debug.Log(angle.ToString());
                return true;
            }
        }

        return false;
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

    void StepUp()
    {
        //TODO: When you collide with steps diagonally, you snap laterally. Ideally, you should only snap forward.

        Vector3 FootPos = transform.TransformPoint(capsuleCollider.center) + -transform.up * (capsuleCollider.height / 2);
        var velocityBeforeCollision = previousVelocities.Peek();
        Vector3 StepDirection = velocityBeforeCollision.magnitude > 0.1f ? velocityBeforeCollision.normalized : transform.forward;

        Vector3 StepToClimb = Vector3.zero;

        //Step 1: Go through every collision that the capsule is experiencing.
        foreach (ContactPoint contact in contactPoints)
        {
            var thisStep = contact.point - FootPos;
            //Step 2: Find the step we care about.

            //Is the height within the step height range we care about?
            if (thisStep.y > stats.stepHeight || thisStep.y < stats.stepMinimumHeight) { continue; }

            //Is the step even in the direction we're going?
            if(Vector3.Dot(velocityBeforeCollision, thisStep) < 0) { continue; }

            //Is is the highest step of the steps I've checked?
            if (thisStep.y < StepToClimb.y) { continue; }

            StepToClimb = thisStep;
        }

        if (StepToClimb == Vector3.zero) { return; }

        //Is there enough room for us???
        //Disable the player collider for this check
        capsuleCollider.enabled = false;

        Vector3 bottomCapsuleSpherePoint = transform.TransformPoint(capsuleCollider.center) - transform.up * capsuleCollider.height * 0.5f + transform.up * capsuleCollider.radius;
        Vector3 topCapsuleSpherePoint = transform.TransformPoint(capsuleCollider.center) + transform.up * capsuleCollider.height * 0.5f - transform.up * capsuleCollider.radius;

        bottomCapsuleSpherePoint += StepToClimb + Vector3.up * 0.1f;
        topCapsuleSpherePoint += StepToClimb;

        if (Physics.CheckCapsule(bottomCapsuleSpherePoint, topCapsuleSpherePoint, capsuleCollider.radius, layerMasks.groundCheck))
        {
            capsuleCollider.enabled = true;
            return;
        }
        
        capsuleCollider.enabled = true;

        //Reduce the step stride, making the step less jarring.
        StepToClimb.x /= 2;
        StepToClimb.z /= 2;

        transform.Translate(StepToClimb, Space.World);
        rb.velocity = velocityBeforeCollision;
    }

    void StepDown()
    {

    }

    #endregion

    #region Unity Messages

    protected override void Awake()
    {
        base.Awake();
        CameraHolder = head;
        InitializeStates();
        contactPoints = new();

        previousVelocities = new();
        previousVelocities.Enqueue(Vector3.zero);
        previousVelocities.Enqueue(Vector3.zero);
        previousVelocities.Enqueue(Vector3.zero);
        previousVelocities.Enqueue(Vector3.zero);
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

        previousVelocities.Enqueue(rb.velocity);
        previousVelocities.Dequeue();
    }

    protected override void Reset()
    {
        base.Reset();

        rb = GetComponent<Rigidbody>();

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
        //head.localPosition = new Vector3(0, stats.headHeight, 0);

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
        heldObjectPosition.localPosition = new Vector3(0, -0.8f, 1.5f);
    }

    void OnValidate()
    {
        if(head == null || rb == null || stats == null) { return; }
        head.localPosition = new Vector3(0, stats.eyeHeight - capsuleCollider.height/2, 0);
        rb.mass = stats.mass;
    }

    void OnCollisionEnter(Collision collision)
    {
        //TODO: Slam Damage
        //float DamageForceMinimum;
    }

    private void OnCollisionStay(Collision collision)
    {
        collision.GetContacts(contactPoints);
        currentMovementState.OnCollisionStay();
    }

    void OnDrawGizmos()
    {

    }

    #endregion

    #region State Definitions

    protected class BipedMovementState
    {
        public string Name { get; protected set; }
        protected B_Biped biped;
        protected Func<float> GetAccelerationLimit;
        protected Func<float> GetMovementSpeed;

        public BipedMovementState(B_Biped Biped)
        {
            Name = "Default";
            biped = Biped;
            GetAccelerationLimit = (() => biped.stats.movementAccelerationLimit);
            GetMovementSpeed = (() => biped.stats.movementSpeed);
        }

        public virtual void EnterState()
        {
            biped.rb.drag = biped.stats.groundDrag;
            biped.capsuleCollider.height = biped.stats.standingHeight;
        }

        public virtual void ExitState() {}

        public virtual void Update() {}

        public virtual void FixedUpdate()
        {
            if (!biped.GroundCheck())
            {
                biped.ChangeMovementState(biped.fallingState);
            }

            if (biped.SlipCheck())
            {
                biped.ChangeMovementState(biped.slippingState);
            }
        }

        //ACTIONS

        public virtual void Move()
        {
            Vector2 Input = biped.movementInput.ReadValue<Vector2>();
            Vector3 DesiredVelocity = new Vector3(Input.x, 0, Input.y) * GetMovementSpeed.Invoke();

            Vector3 planarVelocity = biped.transform.InverseTransformDirection(biped.rb.velocity);
            planarVelocity.y = 0;

            Vector3 RequiredVelocityChange = DesiredVelocity - planarVelocity;

            Vector3 MovementForce = biped.rb.mass * RequiredVelocityChange / Time.fixedDeltaTime;

            if(MovementForce.magnitude > GetAccelerationLimit.Invoke())
            {
                MovementForce = MovementForce.normalized * GetAccelerationLimit.Invoke();
            }

            biped.rb.AddRelativeForce(MovementForce, ForceMode.Force);
        }

        public virtual void Look()
        {
            //TODO: Figure out Sensitivity Scaling
            Vector2 Input = biped.mouseLookInput.ReadValue<Vector2>();
            Input += biped.analogStickLookInput.ReadValue<Vector2>() * Time.deltaTime;

            Quaternion HorizontalElement = Quaternion.Euler(0, Input.x, 0);
            biped.rb.MoveRotation(biped.transform.rotation * HorizontalElement);

            biped.headPitch -= Input.y;
            biped.headPitch = Mathf.Clamp(biped.headPitch, -biped.stats.lookAngleMax, biped.stats.lookAngleMax);
            biped.head.localRotation = Quaternion.Euler(biped.headPitch, 0, 0);
        }

        public virtual void Jump()
        {
            biped.rb.AddRelativeForce(Vector3.up * biped.stats.jumpVelocity, ForceMode.VelocityChange);
        }

        public virtual void Crouch()
        {
            biped.ChangeMovementState(biped.crouchedState);
        }

        public virtual void Sprint()
        {
            var localVelocity = biped.transform.InverseTransformDirection(biped.rb.velocity);
            if(localVelocity.z > biped.stats.sprintMinimumSpeed)
            {
                biped.ChangeMovementState(biped.sprintingState);
            }
        }

        public virtual void OnCollisionStay()
        {
            biped.StepUp();
            biped.StepDown();
        }
    }

    protected class BipedCrouchedState : BipedMovementState
    {
        public BipedCrouchedState(B_Biped biped) : base(biped)
        {
            Name = "Crouched";
            GetAccelerationLimit = (() => biped.stats.crouchedMovementForce);
            GetMovementSpeed = (() => biped.stats.crouchedSpeed);
        }

        public override void EnterState()
        {
            biped.capsuleCollider.height = biped.stats.crouchedHeight;
            biped.transform.position -= new Vector3(0, 0.5f, 0);
        }

        public override void ExitState()
        {
            biped.capsuleCollider.height = biped.stats.standingHeight;
        }

        public override void Crouch()
        {
            biped.ChangeMovementState(biped.defaultMovementState);
        }
    }

    protected class BipedFallingState : BipedMovementState
    {
        public BipedFallingState(B_Biped biped) : base(biped)
        {
            Name = "Falling";
            GetAccelerationLimit = (() => biped.stats.airMovementForce);
            GetMovementSpeed = (() => biped.stats.sprintingSpeed);
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
            biped.rb.drag = biped.stats.airDrag;
        }

        public override void ExitState()
        {
            biped.rb.drag = biped.stats.groundDrag;
        }

        public override void Crouch() {}

        public override void Sprint() {}

        public override void Jump() {}

        public override void OnCollisionStay() {}
    }

    protected class BipedSprintingState : BipedMovementState
    {
        public BipedSprintingState(B_Biped biped) : base(biped)
        {
            Name = "Sprinting";
            GetAccelerationLimit = (() => biped.stats.sprintForce);
            GetMovementSpeed = (() => biped.stats.sprintingSpeed);
        }

        public override void EnterState()
        {
            biped.rb.drag = biped.stats.sprintingDrag;
        }

        public override void ExitState()
        {
            biped.rb.drag = biped.stats.groundDrag;
        }

        public override void Update()
        {
            base.Update();

            var localVelocity = biped.transform.InverseTransformDirection(biped.rb.velocity);
            if (localVelocity.z < biped.stats.sprintMinimumSpeed)
            {
                biped.ChangeMovementState(biped.defaultMovementState);
            }
        }

        public override void Move()
        {
            Vector2 Input = biped.movementInput.ReadValue<Vector2>();
            Input.Scale(new Vector2(biped.stats.sprintingLateralInputScalar, 1));
            Vector3 DesiredVelocity = new Vector3(Input.x, 0, Input.y) * biped.stats.sprintingSpeed;

            Vector3 planarVelocity = biped.transform.InverseTransformDirection(biped.rb.velocity);
            planarVelocity.y = 0;

            Vector3 RequiredVelocityChange = DesiredVelocity - planarVelocity;

            Vector3 MovementForce = biped.rb.mass * RequiredVelocityChange / Time.fixedDeltaTime;

            if (MovementForce.magnitude > biped.stats.movementAcceleration)
            {
                MovementForce = MovementForce.normalized * biped.stats.sprintingAcceleration;
            }

            biped.rb.AddRelativeForce(MovementForce, ForceMode.Force);

        }

        public override void Sprint()
        {
            biped.ChangeMovementState(biped.defaultMovementState);
        }

    }

    protected class BipedSlippingState : BipedMovementState
    {
        public BipedSlippingState(B_Biped Biped) : base(Biped)
        {
            Name = "Slipping";
        }

        public override void FixedUpdate()
        {
            if (!biped.SlipCheck())
            {
                biped.ChangeMovementState(biped.defaultMovementState);
            }

            if (!biped.GroundCheck())
            {
                biped.ChangeMovementState(biped.fallingState);
            }
        }

        public override void EnterState()
        {
            biped.rb.drag = 0;
        }

        public override void ExitState()
        {
            biped.rb.drag = biped.stats.groundDrag;
        }

        public override void Move() {}

        public override void Crouch() {}

        public override void Sprint() {}

        public override void Jump() {}

        public override void OnCollisionStay() {}
    }

    // WEAPON STATES //
    protected class BipedWeaponState
    {
        public string Name { get; protected set; }
        protected B_Biped biped;

        public BipedWeaponState(B_Biped Biped)
        {
            Name = "Default";
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
            biped.lookedAtInteractive?.Interact(biped);
        }

        public virtual void InteractionCheck()
        {
            bool Hit = Physics.SphereCast(biped.head.position, biped.stats.interactionSphereCastRadius, biped.head.forward, out biped.interactionCheckHit, biped.stats.interactionSphereCastDistance, biped.layerMasks.interactive);

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

    protected class BipedNoWeaponState : BipedWeaponState
    {
        public BipedNoWeaponState(B_Biped Biped) : base(Biped)
        {
            Name = "No Weapon";
        }

        public override void Fire(InputAction.CallbackContext ctx) {}

        public override void SwapWeapons() {}

        public override void Reload() {}

        public override void Aim(InputAction.CallbackContext ctx) {}
    }

    protected class BipedAimingState : BipedWeaponState
    {
        public BipedAimingState(B_Biped Biped) : base(Biped)
        {
            Name = "Aiming";
        }
    }

    protected class BipedWeaponLockedState : BipedWeaponState
    {
        public BipedWeaponLockedState(B_Biped Biped) : base(Biped)
        {
            Name = "Locked";
        }
    }

    protected class BipedHoldingPropState : BipedWeaponState
    {
        //hehe... propstate
        Rigidbody HeldObject;
        float OriginalDrag;
        float OriginalAngularDrag;

        public BipedHoldingPropState(B_Biped biped) : base(biped)
        {
            Name = "Holding Prop";
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
            HeldObject.AddForce(biped.head.transform.forward * biped.stats.grabbedThrowSpeed, ForceMode.VelocityChange);
            biped.ChangeWeaponState(biped.previousWeaponState);
        }

        public void GrabObject(Rigidbody PhysicsProp)
        {
            HeldObject = PhysicsProp;
            HeldObject.useGravity = false;
            HeldObject.interpolation = RigidbodyInterpolation.Interpolate;

            OriginalDrag = HeldObject.drag;
            HeldObject.drag = biped.stats.grabbedObjectDrag;

            OriginalAngularDrag = HeldObject.angularDrag;
            HeldObject.angularDrag = biped.stats.grabbedObjectAngularDrag;
        }

        public override void ExitState()
        {
            HeldObject.useGravity = true;
            HeldObject.interpolation = RigidbodyInterpolation.None;

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

            if (Vector3.Distance(DesiredPosition, HeldObject.position) > biped.stats.grabAutomaticReleaseDistance)
            {
                biped.ChangeWeaponState(biped.previousWeaponState);
                return;
            }

            Vector3 Force = DesiredPosition - HeldObject.transform.position;
            Force *= biped.stats.grabSpringForceStrength;

            Quaternion DeltaRotation = DesiredRotation * Quaternion.Inverse(HeldObject.transform.rotation);
            Vector3 Torque = new Vector3(DeltaRotation.x, DeltaRotation.y, DeltaRotation.z) * biped.stats.grabTorqueForceStrength;

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
            prop.velocity = Vector3.ClampMagnitude(prop.velocity, biped.stats.grabbedReleaseMaxSpeed);
        }

        public override void Reload() {}
        public override void InteractionCheck() {}
    }

    #endregion
}