using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FullyAutomatic : FiringModes
{
    Action TriggerAction = null;

    public override void TriggerPull()
    {
        TriggerAction = PulledTrigger;
    }

    void PulledTrigger()
    {
        if (TimeSinceLastShot < TimeBetweenShots) { return; }

        _Gun.Fire();
        TimeSinceLastShot = 0;
    }

    public override void TriggerRelease()
    {
        TriggerAction = null;
    }

    public override void Update()
    {
        base.Update();
        TriggerAction?.Invoke();
    }
}