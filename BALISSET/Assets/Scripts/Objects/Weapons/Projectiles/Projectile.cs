using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [SerializeField] protected GameObject HitEffect;
    protected B_Shell _Source { get; private set; }

    /// <summary>
    /// Set of all damage applied on hit.
    /// Does not include effect field damage (explosions).
    /// </summary>
    [SerializeField] protected List<Damage> HitDamageSet;

    abstract public void Fire(B_Shell Source, float Range);
}