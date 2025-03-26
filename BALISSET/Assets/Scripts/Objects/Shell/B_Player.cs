using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class B_Player : B_Biped
{
    [SerializeField] StringVariable InteractionString;
    [SerializeField] StringVariable DebugString;
    StringBuilder outputDebugString;

    [SerializeField] IntVariable AmmoInMagazine;
    [SerializeField] IntVariable ReserveAmmo;



    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        InteractionStringUIUpdate();

        outputDebugString.Clear();

        AddStatesToDebugString(ref outputDebugString);

        DebugString.Value = outputDebugString.ToString().TrimEnd();
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

    void AddStatesToDebugString(ref StringBuilder debugString)
    {
        debugString.AppendLine($"Movement State: {currentMovementState.Name}");
        debugString.AppendLine($"Weapon State: {currentWeaponState.Name}");
        var planarVel = rb.velocity;
        planarVel.y = 0;
        debugString.AppendLine($"Velocity: {planarVel.magnitude}");
    }

    protected override void Awake()
    {
        base.Awake();
        outputDebugString = new();
    }

    protected override void Reset()
    {
        base.Reset();
        tag = Tags.Player;
    }
}
