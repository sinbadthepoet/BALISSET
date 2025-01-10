using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface BI_Damagable
{
    /// <summary>
    /// Damages the entity.
    /// </summary>
    /// <param name="damage"></param> 100 Health represents a standard Biped.
    /// <param name="source"></param> The source of the damage. Used for kill attribution.
    public void Damage(int damage, object source);
}
