using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunStats : ScriptableObject
{
    public string Name;
    public GameObject Model;

    public float Mass;

    [SerializeReference]
    public FiringModes FiringMode;
    [SerializeReference]
    public ReloadModes ReloadMode;

    #region Damage

    public Projectile projectile;
    public float RoundsPerMinute;

    #endregion

    #region Accuracy

    public float HorizontalSway, VerticalSway;
    public float MovementSwayScalar;
    public float ADSSwayScalar;
    public float CrouchSwayScalar;

    public float HorizontalRecoilPerShot, VerticalRecoilPerShot;
    public float RecoilDecayPerSecond;
    public float RecoilOffsetMagnitudeLimit;

    public float AimVarianceDegrees;
    public float BloomPerShot;
    public float BloomDecayPerSecond;
    public float BloomMax;

    public float ADSMagnification;
    public float TimeToADS;

    #endregion

    #region Handling

    public float TimeToSwitchToSeconds;
    public float TimeToSwitchFromSeconds;

    public float TimeToEnterSprintSeconds;
    public float TimeToExitSprintSeconds;

    #endregion
}
