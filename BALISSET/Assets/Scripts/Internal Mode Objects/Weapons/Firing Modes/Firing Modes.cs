using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class FiringModes : PolymorphicData
{
    protected B_Gun _Gun { get; private set; }

    /// <summary>
    /// This value should not be used in script,
    /// it is simply more convenient to define rate of fire like this
    /// and then derive the time between shots.
    /// </summary>
    public float RoundsPerMinute = 600;

    protected float TimeBetweenShots;
    protected float TimeSinceLastShot;

    public virtual void Initialize(B_Gun Gun)
    {
        _Gun = Gun;
        TimeBetweenShots = 60 / RoundsPerMinute;
    }

    public virtual void Update()
    {
        TimeSinceLastShot += Time.deltaTime;
    }

    abstract public void TriggerPull();
    abstract public void TriggerRelease();
}
