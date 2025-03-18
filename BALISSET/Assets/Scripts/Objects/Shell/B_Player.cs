using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class B_Player : B_Biped
{
    [SerializeField] StringVariable InteractionString;

    [SerializeField] IntVariable AmmoInMagazine;
    [SerializeField] IntVariable ReserveAmmo;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        InteractionStringUIUpdate();
    }

    void InteractionStringUIUpdate()
    {
        if (lookedAtInteractive == null)
        {
            InteractionString.Value = "";
        }
        else
        {
            InteractionString.Value = lookedAtInteractive.GetInteractionString();
        }
    }

    protected override void Reset()
    {
        base.Reset();
        tag = Tags.Player;
    }
}
