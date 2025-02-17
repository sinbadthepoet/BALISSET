using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface BI_Damageable
{
    /// <summary>
    /// Damages the entity.
    /// </summary>
    /// <param name="damage"></param> 100 Health represents a standard Biped.
    /// <param name="source"></param> The source of the damage. Used for kill attribution.
    public void Damage(int damage, B_DamageType DamageType, object source);
}

public enum B_DamageType
{
    Ballistic, //Basic Bullet
    Shredder, //Anti Armor
    HollowPoint, //Anti Unarmored

    Fire,
    Electric,
    Arcane,
    
    Explosive,

    Slashing,
    Piercing,
    Blunt
}