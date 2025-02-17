using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_BreakableObject : B_DynamicObject, BI_Damageable
{
    int Health;

    public void Damage(int damage, B_DamageType DamageType, object source)
    {
        throw new System.NotImplementedException();
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }

    void OnKill()
    {

    }
}
