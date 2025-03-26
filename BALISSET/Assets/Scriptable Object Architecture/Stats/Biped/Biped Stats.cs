using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

[CreateAssetMenu]
public class BipedStats : ScriptableObject
{
    public float mass = 90.0f;
    public float headHeight = 0.8f;
    public float lookAngleMax = 90;

    public float movementSpeed = 2;
    public float movementAcceleration = 50;
    public float movementForce => CalculateMovementForce(movementAcceleration);
    public float standingHeight = 2;
    public float groundDrag = 5;

    public float slopeSlipAngle = 45;
    public float stepHeight = 0.5f;
    public float stepMinimumHeight = 0.05f;
    public float stepDepth = 0.1f;

    public float crouchedHeight = 1;
    public float crouchedSpeed = 1;
    public float crouchedAcceleration = 15;
    public float crouchedMovementForce => CalculateMovementForce(crouchedAcceleration);

    public float jumpHeight = 1;
    public float jumpVelocity => CalculateJumpVelocity();
    public float airAcceleration = 2.5f;
    public float airMovementForce => CalculateMovementForce(airAcceleration);
    public float airDrag = 0;

    public float sprintingSpeed = 5;
    public float sprintingAcceleration = 35;
    public float sprintForce => CalculateMovementForce(sprintingAcceleration);
    public float sprintMinimumSpeed = 1.75f;
    public float sprintingLateralInputScalar = 0.2f;
    public float sprintingDrag = 5;

    public float groundCheckAdditionalDistance = -0.35f;

    public float interactionSphereCastRadius = 0.1f;
    public float interactionSphereCastDistance = 3.0f;

    public float grabSpringForceStrength = 50;
    public float grabTorqueForceStrength = 50;
    public float grabbedObjectDrag = 30;
    public float grabbedObjectAngularDrag = 30;
    public float grabAutomaticReleaseDistance = 5;
    public float grabbedThrowSpeed = 20;
    public float grabbedReleaseMaxSpeed = 20;

    float CalculateMovementForce(float acceleration)
    {
        return mass * acceleration;
    }

    float CalculateJumpVelocity()
    {
        return MathF.Sqrt(-2 * Physics.gravity.y * jumpHeight);
    }
}
