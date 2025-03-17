using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class B_PhysicsProp : B_Interactive
{
    [SerializeField] Rigidbody rb;

    public override string GetInteractionString()
    {
        return "Pick Up";
    }

    public override void Interact(B_Biped Interactor)
    {
        //Interactor.GrabObject(rb);
    }

    protected override void Reset()
    {
        base.Reset();
        gameObject.tag = Tags.PhysicsProp;
        rb = GetComponent<Rigidbody>();
    }
}
