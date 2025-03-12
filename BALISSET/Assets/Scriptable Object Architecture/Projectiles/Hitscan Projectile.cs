using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitscanProjectile : Projectile
{
    [SerializeField] float HitForce;
    RaycastHit hitInfo;

    public override void Fire(B_Shell Source, float Range)
    {
        LayerMask HitLayers = Layers.GetLayerMask(Layers.Environment, Layers.BallisticsHitboxes, Layers.Interactive);
        if(Physics.Raycast(transform.position, transform.forward, out hitInfo, Range, HitLayers, QueryTriggerInteraction.Ignore))
        {
            OnImpact();
        }
    }

    protected override void OnImpact()
    {
        Rigidbody rb = hitInfo.rigidbody;
        if(rb != null)
        {
            rb.AddForceAtPosition(transform.forward * HitForce, hitInfo.point);
        }

        var DamagedObject = hitInfo.collider.GetComponent<BI_Damagable>();
        if(DamagedObject != null)
        {
            foreach(Damage damage in HitDamageSet)
            {
                DamagedObject.ApplyDamage(damage, _Source);
            }
        }

        Instantiate(HitEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
        Destroy(gameObject);
    }
}
