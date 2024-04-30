using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody))]
public class B_Player : MonoBehaviour
{
    #region Variables

    #region Serialized Varibales

    ////////////////////////
    //**EDITOR VARIABLES**//
    ////////////////////////

    [Header("Object References")]
    [SerializeField] Camera _camera;
    [SerializeField] Rigidbody _rb;
    [SerializeField] CapsuleCollider _capsuleCollider;

    [Header("Player Size")]
    [SerializeField] float StandingHeight = 2.0f;

    [Header("Locomotion Parameters")]
    [Tooltip("Defines the movement speed of the entity in m/s")]
    [SerializeField] float MovementSpeed = 5.0f;
    [Tooltip("Defines how long the entity should take under ideal conditions to accelerate to their movement speed from rest.")]
    [SerializeField] float MovementAcceleration = 1.0f;

    [Header("Crouching Parameters")]
    [SerializeField] float CrouchedHeight = 1.0f;
    [SerializeField] float CrouchedSpeedScalar = 0.5f;

    [Header("Resistant Forces")]
    [SerializeField] float GroundDrag = 1.0f;
    [SerializeField] float AirDrag = 0.0f;
    [SerializeField] float AirControlScalar = 1.0f;

    [Header("Camera Parameters")]
    [SerializeField] float LookAngleClamp = 80.0f;

    [Header("Jump Parameters")]
    [SerializeField] float JumpForce = 1000f; //Ideally, we change this to have users define height and the game figures out the necessary force.
    [SerializeField] float GroundCheckAdditionalDistance = 0.1f;

    #endregion

    #region Internal Variables

    //////////////////////////
    //**INTERNAL VARIABLES**//
    //////////////////////////
    
    float _verticalLook;

    //States
    bool _isGrounded;
    bool _isMoving;
    bool _isCrouched;

    //Derived Values
    float _movementForce;

    #endregion

    #region Input Variables

    ///////////////////////
    //**INPUT VARIABLES**//
    ///////////////////////

    Vector2 _movementInput;
    Vector2 _lookInput;

    #endregion

    #endregion

    #region Functions

    #region Built In Functions
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
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void OnValidate()
    {
        _movementForce = _rb.mass * MovementAcceleration;
        Debug.Log("Set Movement Force to " + _movementForce);

        _capsuleCollider.height = StandingHeight;
    }

    #endregion

    #region Action Functions

    /// <summary>
    /// This function sucks dick and I hope we replace it.
    /// </summary>
    void Movement()
    {
        _isMoving = _movementInput.magnitude > 0;

        Vector3 MovementForce = new Vector3(_movementInput.x, 0, _movementInput.y) * _movementForce * 1;// Time.fixedDeltaTime;

        if (!_isGrounded)
        {
            MovementForce *= AirControlScalar;
        }
        else if (_isCrouched) //Air control should be regardless of whether they're crouched on not
        {
            MovementForce *= CrouchedSpeedScalar; //Eventually, we should modify the _movementForce as we switch back and forth between states.
        }

        _rb.AddRelativeForce(MovementForce, ForceMode.Force);

        Vector3 PlanarVelocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);

        if(PlanarVelocity.magnitude > MovementSpeed) 
        {
            PlanarVelocity = PlanarVelocity.normalized * MovementSpeed;
            Vector3 CappedVelocity = new Vector3(PlanarVelocity.x, _rb.velocity.y, PlanarVelocity.z);
            _rb.velocity = CappedVelocity;
        }

        //Debug.Log("Player Velocity: " + _rb.velocity.magnitude);
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

    public void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            //Debug.Log("Jumping");
            _rb.AddRelativeForce(Vector3.up * JumpForce, ForceMode.Impulse);
            _isGrounded = false;
            SetGrounded();
        }
    }

    public void Crouch(InputAction.CallbackContext context)
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
        }
    }

    public void Sprint(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException;
    }

    public void Interact(InputAction.CallbackContext context)
    {

    }
    #endregion

    #region Input Functions
    public void onMovement(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void onLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }
    #endregion

    #region Internal Use Functions

    void GroundCheck()
    {
        _isGrounded = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), _capsuleCollider.height / 2 + GroundCheckAdditionalDistance);
        SetGrounded();
        Debug.Log(_isGrounded);
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
