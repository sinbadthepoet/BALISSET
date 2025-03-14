using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detonation : MonoBehaviour
{
    [SerializeField] GameObject ParticleSystem;

    [SerializeField] float BlastRadius;
    [SerializeField] float ExplosiveForce;

    void Start()
    {
        LayerMask mask = Layers.GetLayerMask(Layers.BallisticsHitboxes, Layers.Interactive);
        var Colliders = Physics.OverlapSphere(transform.position, BlastRadius, mask, QueryTriggerInteraction.Ignore);

        foreach(Collider collider in Colliders)
        {
            Rigidbody rb = collider.attachedRigidbody;
            if(rb == null) { continue; }

            rb.AddExplosionForce(ExplosiveForce, transform.position, BlastRadius);
        }

        Instantiate(ParticleSystem, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}
