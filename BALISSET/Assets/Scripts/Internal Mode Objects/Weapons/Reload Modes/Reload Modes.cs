using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReloadModes : PolymorphicData
{
    protected B_Gun _Gun;

    public bool RoundChambered = false;

    public virtual void Initialize(B_Gun Gun, int StartingAmmo)
    {
        _Gun = Gun;
    }

    abstract public void ChargeGun();

    abstract public void Reload();

    abstract public void GunFired();

    abstract public void Update();

    abstract public int GetAmmoInMagazine();

    abstract public int GetReserveAmmo();
}
