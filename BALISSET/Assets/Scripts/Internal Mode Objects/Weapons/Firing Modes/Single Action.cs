using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SingleAction : FiringModes
{
    Action TriggerAction;

    public override void Initialize(B_Gun Gun)
    {
        base.Initialize(Gun);
        TriggerAction = Prime;
    }

    public override void TriggerPull()
    {
        TriggerAction.Invoke();
    }

    //TODO: Priming Time
    void Prime()
    {
        Debug.Log("Gun Primed");
        TriggerAction = Fire;
    }

    void Fire()
    {
        if (TimeSinceLastShot < TimeBetweenShots) { return; }

        _Gun.Fire();
        TimeSinceLastShot = 0;

        TriggerAction = Prime;
    }

    public override void TriggerRelease() {}
}