using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BurstFire : FiringModes
{
    Action BurstAction;

    public int RoundsPerBurst;
    int RoundsFiredThisBurst;

    public float TimeBetweenBursts;
    float TimeSinceLastBurst;

    public override void Initialize(B_Gun Gun)
    {
        base.Initialize(Gun);
        BurstAction = NotBurst;
    }

    //TODO: This should be a bufferable action, if you trigger pull while a burst is happening.
    public override void TriggerPull()
    {
        if (TimeSinceLastBurst < TimeBetweenBursts) { return; }

        RoundsFiredThisBurst = 0;
        TimeSinceLastBurst = 0;
        BurstAction = Burst;
    }

    public override void Update()
    {
        base.Update();
        BurstAction.Invoke();
    }

    void Burst()
    {
        if(TimeSinceLastShot < TimeBetweenShots) { return; }

        _Gun.Fire();
        TimeSinceLastShot = 0;
        RoundsFiredThisBurst++;

        if (RoundsFiredThisBurst == RoundsPerBurst) { BurstAction = NotBurst; }
    }

    void NotBurst()
    {
        TimeSinceLastBurst += Time.deltaTime;
    }

    public override void TriggerRelease() {}
}