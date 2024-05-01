using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class B_Player : MonoBehaviour
{
    //This file makes heavy use of Regions.
    //Helpful for organization, but sometimes you want to view everything at once.
    //In Visual Studio, Ctrl + M + L uncollapses all regions and any other sections.

    #region Variables

    #region Editor Variables

    ////////////////////////
    //**EDITOR VARIABLES**//
    ////////////////////////

    //These should be assigned automatically.
    //If you add more, grab the references OnReset();
    [Header("Object References")]
    [Tooltip("The player's first person camera.")]
    [SerializeField] Camera _camera;
    [SerializeField] Rigidbody _rb;
    [Tooltip("A reference to the player collider. Used in this script for crouching.")]
    [SerializeField] CapsuleCollider _capsuleCollider;

    [Header("Player Size")]
    [SerializeField] float StandingHeight = 2.0f;

    [Header("Locomotion Parameters")]
    [Tooltip("Defines the maximum running speed of the entity in m/s.")]
    [SerializeField] float MovementSpeed = 7.0f;
    [Tooltip("An arbitrary unit defining the strength of acceleration in movement.")]
    [SerializeField] float MovementAcceleration = 100.0f;

    [Header("Crouching Parameters")]
    [SerializeField] float CrouchedHeight = 1.0f;
    [SerializeField] float CrouchedSpeedScalar = 0.75f;

    [Header("Resistant Forces")]
    [SerializeField] float GroundDrag = 1.0f;
    [SerializeField] float AirDrag = 0.0f;
    [Tooltip("Scales movement input forces while player is in the air.")]
    [SerializeField] float AirControlScalar = 0.1f;

    [Header("Camera Parameters")]
    [SerializeField] float LookAngleClamp = 80.0f;

    [Header("Jump Parameters")]
    [SerializeField] float JumpForce = 500f; //Ideally, we change this to have users define height and the game figures out the necessary force.
    [Tooltip("How far past the bottom of the collider are we checking for ground?")]
    [SerializeField] float GroundCheckAdditionalDistance = 0.1f;

    #endregion

    #region Internal Variables

    //////////////////////////
    //**INTERNAL VARIABLES**//
    //////////////////////////
    
    //Tracked Values
    float _verticalLook;

    //States
    //Eventually, this component will be moved to a state pattern.
    bool _isGrounded;
    bool _isMoving;
    bool _isCrouched;

    //Derived Values
    //We convert editor parameters into the required value in OnValidate();
    float _movementForce;

    #endregion

    #region Input Variables

    ///////////////////////
    //**INPUT VARIABLES**//
    ///////////////////////

    //These store controller values so we can use them without requiring an event callback.
    Vector2 _movementInput;
    Vector2 _lookInput;
    float _leanInput;

    #endregion

    #endregion

    #region Functions

    #region Built In Functions

    //////////////////////////
    //**BUILT IN FUNCTIONS**//
    //////////////////////////
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        GroundCheck();
        Look();
    }

    void FixedUpdate()
    {
        Movement();
    }

    void Reset()
    {
        _camera = FindObjectOfType<Camera>();
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void OnValidate()
    {
        _movementForce = _rb.mass * MovementAcceleration;
        _capsuleCollider.height = StandingHeight;
    }

    #endregion

    #region Action Functions

    ////////////////////////
    //**ACTION FUNCTIONS**//
    ////////////////////////

    // This function mostly sucks dick and I hope we replace it.
    void Movement()
    {
        _isMoving = _movementInput.magnitude > 0;

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

        if(PlanarVelocity.magnitude > MovementSpeed) 
        {
            PlanarVelocity = PlanarVelocity.normalized * MovementSpeed;
            Vector3 CappedVelocity = new Vector3(PlanarVelocity.x, _rb.velocity.y, PlanarVelocity.z);
            _rb.velocity = CappedVelocity;
        }
    }

    void Look()
    {
        //Horizontal Rotation of player GameObject
        transform.Rotate(Vector3.up, _lookInput.x);

        //Vertical Rotation of child Camera GameObject
        _verticalLook -= _lookInput.y;
        _verticalLook = Mathf.Clamp(_verticalLook, -LookAngleClamp, LookAngleClamp);
        _camera.transform.localRotation = Quaternion.Euler(_verticalLook, 0, 0);
    }

    //Using Unity Events in Input 1.7 calls these events multiple times for each phase.
    //We are manually filtering out the phases we don't need.

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        if (_isGrounded)
        {
            _rb.AddRelativeForce(Vector3.up * JumpForce, ForceMode.Impulse);
            _isGrounded = false;
            SetGrounded();
        }
    }

    public void Crouch(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
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

    public void Sprint(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        throw new System.NotImplementedException();
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Performed) return;
        throw new System.NotImplementedException();
    }

    #endregion

    #region Input Functions

    ///////////////////////
    //**INPUT FUNCTIONS**//
    ///////////////////////

    //These functions store values input values from events so we can process them seperately.

    public void onMovement(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void onLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void onLean(InputAction.CallbackContext context)
    {
        _leanInput = context.ReadValue<float>();
    }

    #endregion

    #region Internal Functions

    //////////////////////////
    //**INTERNAL FUNCTIONS**//
    //////////////////////////

    void GroundCheck()
    {
        //ISSUE: Standing on edge means raycast misses ground.
        _isGrounded = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), _capsuleCollider.height / 2 + GroundCheckAdditionalDistance);
        SetGrounded();
    }

    void SetGrounded()
    {
        if (_isGrounded)
        {
            _rb.drag = GroundDrag;
        }
        else
        {
            _rb.drag = AirDrag;
        }
    }

    #endregion

    #endregion
}
