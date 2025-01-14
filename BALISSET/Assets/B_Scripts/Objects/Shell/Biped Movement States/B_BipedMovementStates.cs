using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BI_BipedMovementState
{
    CapsuleCollider _capsuleCollider;

    protected float Drag = 1.0f;
    protected float BipedHeight = 2.0f;
    protected float GroundCheckAdditionalDistance = 0.1f;

    public BI_BipedMovementState(B_Shell shell)
    {
        _capsuleCollider = shell.GetComponent<CapsuleCollider>();
    }

    public abstract void EnterState(B_Shell shell);
    public abstract void ExitState(B_Shell shell);
    public abstract void Update(B_Shell shell);

    bool GroundCheck(Transform shell)
    {
        return Physics.Raycast(shell.position, shell.TransformDirection(Vector3.down), _capsuleCollider.height / 2 + GroundCheckAdditionalDistance);
    }
}

public class B_StandingState : BI_BipedMovementState
{

}

public class B_CrouchedState : BI_BipedMovementState
{

}

public class B_FallingState : BI_BipedMovementState
{
    B_FallingState(B_Shell shell) : base(shell)
    {

    }

    public override void EnterState(B_Shell shell)
    {
        throw new System.NotImplementedException();
    }

    public override void ExitState(B_Shell shell)
    {
        throw new System.NotImplementedException();
    }

    public override void Update(B_Shell shell)
    {
        throw new System.NotImplementedException();
    }
}


