using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReloadModes : PolymorphicData
{
    protected B_Gun _Gun;

    public virtual void Initialize(B_Gun Gun)
    {
        _Gun = Gun;
    }

    abstract public void Reload();

    abstract public void Update();
}
