using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazineReload : ReloadModes
{
    int AmmoInMagazine; 
    public int MagazineCapacity;
    
    int ReserveAmmo;

    public float ReloadTime;

    public override void Initialize(B_Gun Gun, int StartingReserveAmmo)
    {
        base.Initialize(Gun, StartingReserveAmmo);
        ReserveAmmo = StartingReserveAmmo - MagazineCapacity;
        AmmoInMagazine = MagazineCapacity;
    }

    public override void ChargeGun()
    {
        if(AmmoInMagazine < 1) { return; }

        AmmoInMagazine -= 1;
        RoundChambered = true;
    }

    public override void Reload()
    {
        int AmmoToLoad = MagazineCapacity - AmmoInMagazine;

        if(AmmoToLoad <= ReserveAmmo)
        {
            ReserveAmmo -= AmmoToLoad;
            AmmoInMagazine += AmmoToLoad;
        }
    }

    public override void GunFired()
    {
        RoundChambered = false;
        ChargeGun();
    }

    public override void Update() {}

    public override int GetAmmoInMagazine()
    {
        return AmmoInMagazine + Convert.ToInt32(RoundChambered);
    }

    public override int GetReserveAmmo()
    {
        return ReserveAmmo;
    }

    public override bool CanFire()
    {
        return RoundChambered;
    }
}
