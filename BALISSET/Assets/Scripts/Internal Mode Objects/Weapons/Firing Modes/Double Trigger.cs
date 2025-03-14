using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DoubleTrigger : FiringModes
{
    public GameObject SecondProjectile;

    public override void TriggerPull()
    {
        if (TimeSinceLastShot < TimeBetweenShots) { return; }

        _Gun.Fire();
        TimeSinceLastShot = 0;
    }

    public override void TriggerRelease()
    {
        _Gun.Fire(SecondProjectile);
    }
}