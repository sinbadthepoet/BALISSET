using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents dynamic bodies in the game world that can be manipulated by the player.
/// Crates, Barrels, Detritus, etc.
/// </summary>

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class B_DynamicObject : MonoBehaviour, BI_Interactive
{
    public virtual string GetInteractionString()
    {
        throw new System.NotImplementedException();
    }

    public virtual void Interact()
    {
        throw new System.NotImplementedException();
    }

    void FollowPlayer()
    {
        //Behavior for when the object is being held in front of the player
    }

    void Throw()
    {
        //Add Spin to make throws look better?
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
