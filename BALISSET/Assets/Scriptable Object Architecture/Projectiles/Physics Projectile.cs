using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PhysicsProjectile : Projectile, BI_Damagable
{
    enum FuseType
    {
        Instant, //Starts counting on fire.
        Impact //Starts counting on impact above the trigger force.
    }

    Action ThrustAction;

    [SerializeField] Rigidbody rb;

    [SerializeField] bool UseGravity = true;
    [SerializeField] float Mass = 0.01f;
    [SerializeField] float Drag = 0;
    [SerializeField] float TriggerForce = 0;
    [SerializeField] FuseType fuseType;
    [SerializeField] float FuseTimeSeconds = 0;
    float FuseTimer = 0;
    bool FuseLit = false;
    [SerializeField] float LaunchVelocity = 730;
    [SerializeField] float ThrustForce = 0;
    [SerializeField] float ThrustDelay = 1;
    float ThrustDelayTimer = 0;
    [SerializeField] float SpinSpeed;
    [SerializeField] GameObject Detonation;
    [SerializeField] float LifetimeSeconds;

    public override void Fire(B_Shell Source, float Range)
    {
        ThrustAction = ThrustDelayTick;
        rb.AddForce(transform.forward * LaunchVelocity, ForceMode.VelocityChange);
        rb.AddTorque(transform.forward * SpinSpeed, ForceMode.VelocityChange);

        if(fuseType == FuseType.Instant)
        {
            FuseLit = true;
        }
    }

    void Thrust()
    {
        rb.AddForce(transform.forward * ThrustForce);
    }

    void ThrustDelayTick()
    {
        ThrustDelayTimer += Time.deltaTime;

        if (ThrustDelayTimer > ThrustDelay)
        {
            ThrustAction = Thrust;
            Thrust();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.impulse.magnitude > TriggerForce)
        {
            var DamageInterface = collision.gameObject.GetComponent<BI_Damagable>();
            if (DamageInterface != null)
            {
                foreach(Damage damage in HitDamageSet)
                {
                    DamageInterface.ApplyDamage(damage, _Source);
                }
            }
            FuseLit = true;
        }
    }

    void Detonate()
    {
        Debug.Log("Bye");
        if(Detonation != null)
        {
            Instantiate(Detonation, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    void FuseTick()
    {
        FuseTimer += Time.deltaTime;
        if(FuseTimer < FuseTimeSeconds) { return; }
        Detonate();
    }

    protected override void OnImpact()
    {
        throw new NotImplementedException();
    }
    
    public void ApplyDamage(Damage damage, B_Shell source)
    {
        throw new NotImplementedException();
    }

    void Start()
    {
        Debug.Log("Started");
        ThrustAction = null;
    }

    void Update()
    {
        ThrustAction?.Invoke();

        if (FuseLit)
        {
            FuseTick();
        }
    }
    
    void Reset()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = UseGravity;
        rb.drag = Drag;
        rb.mass = Mass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }
}