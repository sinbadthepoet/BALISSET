using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface BI_Damagable
{
    /// <summary>
    /// Damages the entity.
    /// </summary>
    /// <param name="damage">100 Health represents a standard Biped.</param>
    /// <param name="source">The source of the damage. Used for kill attribution.</param>
    public void ApplyDamage(Damage damage, B_Shell source);
}

[Serializable]
public struct Damage
{
    public int DamageAmount;
    public DamageTypes DamageType;
}

public enum DamageTypes
{
    Ballistic, //Bullets
    Slashing, //Blades
    Laser, //Uhh... scary lights

    // Environmental
    BluntForce, //Impact
    Fire, //Hot Hot
    Electric //Arcing Electricity, Electrified Pools
}