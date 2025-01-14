using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Gun : MonoBehaviour
{
    public struct B_GunMagazine
    {
        int Capacity;
        int RoundsLoaded;
    }

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

    float AimVarianceDegrees;

    #endregion

    #region Reticle Bloom

    float BloomPerShot, BloomDecayPerSeconds, BloomMax;

    #endregion


    float RedReticleRange;


    float AimAssistDistance;
    
    bool ChamberLoaded;

    B_GunMagazine[] magazines;

    void Fire()
    {

    }

    void Reload()
    {
        
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