using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class B_PhysicsProp : MonoBehaviour, IInteractive
{
    [SerializeField] [HideInInspector] Rigidbody rb;

    public string GetInteractionString()
    {
        return "Pick Up";
    }

    public void Interact(B_Shell Interactor)
    {
        if(Interactor is B_Biped biped)
        {
            biped.GrabPhysicsProp(rb);
        }
    }

    void Reset()
    {
        gameObject.layer = Layers.Interactive;
        tag = Tags.PhysicsProp;
        rb = GetComponent<Rigidbody>();
    }
}
