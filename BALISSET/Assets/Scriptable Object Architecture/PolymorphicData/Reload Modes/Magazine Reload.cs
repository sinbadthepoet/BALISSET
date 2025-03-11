using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagazineReload : ReloadModes
{
    float ReloadTime;

    public void Initialize(B_Gun Gun, float TimeToReload)
    {
        ReloadTime = TimeToReload;
        base.Initialize(Gun);
    }

    public override void Reload()
    {
        //Check Ammo, then do the reload
        //_Gun.
    }

    public override void Update() {}
}
