using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class B_PhysicsProp : B_Interactive
{
    public override string GetInteractionString()
    {
        return "Pick Up";
    }

    protected override void Interact(B_Shell Interactor)
    {
        throw new System.NotImplementedException();
    }

    protected override void Reset()
    {
        base.Reset();
        gameObject.tag = Tags.PhysicsProp;
    }
}
