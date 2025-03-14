using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SemiAutomatic : FiringModes
{
    public override void TriggerPull()
    {
        if(TimeSinceLastShot < TimeBetweenShots) { return; }

        _Gun.Fire();
        TimeSinceLastShot = 0;
    }

    public override void TriggerRelease() {}
}
