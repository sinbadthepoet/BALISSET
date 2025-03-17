using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class B_Gun : MonoBehaviour, IInteractive
{
    #region Gun

    [SerializeField] GameObject _Projectile;
    [SerializeField] Transform Muzzle;

    [SerializeReference] FiringModes FiringMode = new FullyAutomatic();
    [SerializeReference] ReloadModes ReloadMode = new MagazineReload();

    [SerializeField] int StartingAmmo = 120;

    public void PullTrigger()
    {
        FiringMode.TriggerPull();
    }

    public void ReleaseTrigger()
    {
        FiringMode.TriggerRelease();
    }

    public void Fire(GameObject proj = null)
    {
        if(proj == null) { proj = _Projectile; }

        if (!ReloadMode.CanFire())
        {
            FiringMode.TriggerRelease();
            return;
        }

        Instantiate(proj, Muzzle.position + Muzzle.forward, Muzzle.rotation).GetComponent<Projectile>().Fire(Biped, 10000);
        ReloadMode.GunFired();
    }

    public void ChargeGun()
    {
        ReloadMode.ChargeGun();
    }

    public void Reload()
    {
        ReloadMode.Reload();
        if (!GetRoundChambered())
        {
            ChargeGun();
        }
    }

    #endregion

    #region Getters

    public int GetAmmoInMagazine()
    {
        return ReloadMode.GetAmmoInMagazine();
    }

    public int GetReserveAmmo()
    {
        return ReloadMode.GetReserveAmmo();
    }

    public bool GetRoundChambered()
    {
        return ReloadMode.RoundChambered;
    }

    #endregion

    #region Physics

    public Rigidbody rb { get; private set; }
    public Transform OriginalTransform { get; private set; }
    public List<Collider> Colliders { get; private set; }

    #endregion

    #region Interaction

    B_Biped Biped;

    #endregion

    #region Unity Events

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        OriginalTransform = transform.parent;

        Colliders = new List<Collider>();

        foreach (Collider collider in GetComponents<Collider>())
        {
            Colliders.Add(collider);
        }

        FiringMode.Initialize(this);
        ReloadMode.Initialize(this, StartingAmmo);
    }
    
    void Update()
    {
        FiringMode.Update();
        //Put in Reset
        Muzzle = transform.Find("Muzzle");
    }

    
    public void Interact(B_Shell Interactor)
    {
        throw new NotImplementedException();
    }

    public string GetInteractionString()
    {
        throw new NotImplementedException();
    }

    #endregion

    /*
    
    #region Parameters
    
    #region AimAssist

    #region Sticky Camera Magnetism
    //The camera slowing down when passing a valid target.


    #endregion

    #region Aim Magnetism
    //The aiming vector snapping to a target.
    //Bullet Magnetism is handled in B_Projectile.

    #endregion

    #region Strafe Tracking
    //The camera turning to follow the target while the player is strafing.


    #endregion

    #region ADS Snap
    //ADS Auto Aim :clueless:


    #endregion

    #endregion

    #region Accuracy
    //Consider the possibility of gun weight, and weight distribution affecting recoil!

    float AimVarianceDegrees; //The amount of variance in direction the bullet may take as the bullet exits the barrel.

    float HorizontalSway, VerticalSway; //The amount the barrel will sway while held at hip.
    float MovementSwayScalar; //The amount that moving at a standard walking speed will affect sway. Scales with movement speed.
    float ADSSwayScalar; //Sway is reduced while aiming down the sights.

    float CrouchSwayScalar; 

    float HorizontalRecoilRate, VerticalRecoilRate;
    float RecoilDecayPerSecond;

    Vector2 RecoilOffset;

    #region Reticle Bloom

    float BloomPerShot, BloomDecayPerSecond, BloomMax;

    #endregion

    #endregion

    #region RateOfFire
    
    float RateOfFire;
    float TimeBetweenShots;
    float TimeSinceLastShot;

    //Weapon Jamming?

    #endregion

    float EffectiveRange; //Used for Red Reticle Range, Activating Aim Assist, and Bullet Magnetism.
    float ReloadTime; //May need to replace this with an array for each stage. Later, this could be tied to animation, or vice versa.
    
    #region ADS

    float ADSTime;
    float ADSMagnification;

    #endregion

    #region Switching

    //Switch Time = othergun.TimeToSwitchFrom + this.TimeToSwitchTo;

    float TimeToSwitchTo;
    float TimeToSwitchFrom;

    #endregion

    #region Sprint

    float TimeToEnterSprint;
    float TimeToExitSprint;

    #endregion

    #endregion

    #region Private Variables
    bool ChamberLoaded;

    Transform BarrelPosition;

    ReloadState ReloadStage;

    #endregion

    B_GunMagazine[] magazines; //These should come from the Inventory.

    void Fire()
    {
        //Fire Bullet from Transform Position
        //
    }

    void Reload()
    {
        
    }

    void LoadNextRound()
    {
        //Checks Magazine for next round and attempts to load it.

    }
}

enum B_DamageType
{
    Ballistic,
    Fire,
    Explosive,
    Slashing,
    Piercing,
    Electric
}

public abstract class B_Projectile : MonoBehaviour
{
    int penetrationStength;

    B_DamageType damageType;
    object Attribution;

    object Target; //Bullet Magnetism Strength
}

public class B_HitscanProjectile : B_Projectile
{

}

public class B_PhysicalProjectile : B_Projectile
{
    float GravityArc;
    float EffectiveRange;

    float TrackingStrength;
}

public class Attachments
{
    B_Gun.AttachmentPoints attachmentPoint;
    virtual void AttachToGun(); //Modify the gun stats when attached.
    virtual void RemoveAttachment(); 
}

public abstract B_GunMagazine
{
    int Capacity; 
    int RoundsLoaded;
    float UnloadTime;
    float LoadingTime;

    public abstract void Reload(); // Custom Implementation for standard, taped, drum, and alternative magazines.
    */
}