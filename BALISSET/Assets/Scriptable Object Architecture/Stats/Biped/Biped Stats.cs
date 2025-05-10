using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

[CreateAssetMenu]
public class BipedStats : ScriptableObject
{
    [Header("Body")]
    public float standingHeight = 1.83f;
    public float eyeHeight = 1.70f;
    public float mass = 90.0f;
    public float capsuleRadius = 0.5f;

    [Header("Movement")]
    public float movementSpeed = 2;
    public float movementAccelerationTime = 1;
    [HideInInspector] public float movementAccelerationLimit = 1;
    public float movementDecelerationTime = 1;
    [HideInInspector] public float movementDecelerationLimit = 1;

    [Header("Ground Checking")]
    public float groundCheckSphereRadius = 0.49f;
    public float groundCheckAdditionalDistance = 0.01f;

    [Header("Slipping")]
    public float slopeSlipAngle = 45;

    [Header("Stair Step Up")]
    public float stepHeight = 0.5f;
    public float stepMinimumHeight = 0.05f;

    [Header("Stair Step Down")]
    public float stepDownDistance = 1;

    [Header("Look")]
    public float lookAngleMax = 90;

    [Header("Crouching")]
    public float crouchedHeight = 1.42f;
    public float crouchedSpeed = 1;
    public float crouchedAccelerationTime = 1;
    [HideInInspector] public float crouchedAccelerationLimit = 1;

    [Header("Sprinting")]
    public float sprintingSpeed = 5;
    public float sprintMinimumSpeed = 1.75f;
    public float sprintingAccelerationTime = 1;
    [HideInInspector] public float sprintingAccelerationLimit = 1;
    public float sprintingLateralInputScalar = 0.2f;

    [Header("Jumping")]
    public float jumpHeight = 1;
    [HideInInspector] public float jumpVelocity;
    //public float airAcceleration = 2.5f;

    [Header("Interaction")]
    public float interactionSphereCastRadius = 0.1f;
    public float interactionSphereCastDistance = 3.0f;

    [Header("Physics Prop Grab")]
    public float grabSpringForceStrength = 50;
    public float grabTorqueForceStrength = 50;
    public float grabbedObjectDrag = 30;
    public float grabbedObjectAngularDrag = 30;
    public float grabAutomaticReleaseDistance = 5;
    public float grabbedThrowSpeed = 20;
    public float grabbedReleaseMaxSpeed = 20;

    void OnValidate()
    {
        movementAccelerationLimit = CalculateAccelerationLimit(movementSpeed, movementAccelerationTime);
        crouchedAccelerationLimit = CalculateAccelerationLimit(crouchedSpeed, crouchedAccelerationTime);
        sprintingAccelerationLimit = CalculateAccelerationLimit(sprintingSpeed, sprintingAccelerationTime);

        movementDecelerationLimit = CalculateAccelerationLimit(movementSpeed, movementDecelerationTime);

        jumpVelocity = MathF.Sqrt(-2 * Physics.gravity.y * jumpHeight);
    }

    float CalculateAccelerationLimit(float v, float t)
    {
        if (t == 0) t = float.MinValue;
        return mass * (v / t);
    }
}
