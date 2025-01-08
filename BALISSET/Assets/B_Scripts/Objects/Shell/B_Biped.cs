using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class B_Biped : B_Shell
{
    #region Variables

    Rigidbody _rb;
    CapsuleCollider _capsuleCollider;
    Transform _head;

    #endregion

    #region Functions

    #region Built In Functions

    protected virtual void Reset()
    {
        _rb = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        //Make sure the component has a head. Eventually, we can replace this with an editor feature.
        _head = transform.Find("Head");
        if (_head == null)
        {
            _head = new GameObject("Head").transform;
        }
        _head.parent = transform;
        _head.localPosition = Vector3.zero;
    }

    protected override void Awake()
    {
        base.Awake();

        //Make sure the component has a head. Eventually, we can replace this with an editor feature.
        _head = transform.Find("Head");
    }


    #endregion
    #region Action Functions

    protected override void Look()
    {
        //Horizontal Rotation of player GameObject
        transform.Rotate(Vector3.up, _lookInput.x);

        //Vertical Rotation of child Camera GameObject
        _verticalLook -= _lookInput.y;
        _verticalLook = Mathf.Clamp(_verticalLook, -LookAngleClamp, LookAngleClamp);
        _head.transform.localRotation = Quaternion.Euler(_verticalLook, 0, 0);

        if (_vCamAim != null)
        {
            var aim = (CinemachinePOV)_vCamAim;
            aim.m_VerticalAxis.Value = _verticalLook;
        }
    }

    #endregion
    #region Component Use Functions

    protected override void InitializeActions()
    {
        base.InitializeActions();
    }

    protected override void BindVirtualCamera()
    {
        _vCam = GameObject.Find("Player Virtual Camera").GetComponent<CinemachineVirtualCamera>();

        //If no virtual camera can be found, we can make one for now.
        if (_vCam == null)
        {
            _vCam = new GameObject("Player Virtual Camera", typeof(CinemachineVirtualCamera)).GetComponent<CinemachineVirtualCamera>();
        }

        _vCam.Follow = _head;
        _vCam.LookAt = _head;

        _vCamBody = _vCam.AddCinemachineComponent<CinemachineHardLockToTarget>();
        _vCamAim = _vCam.AddCinemachineComponent<CinemachinePOV>();

        var aim = (CinemachinePOV)_vCamAim;
        aim.m_VerticalAxis.m_MaxValue = 80;
        aim.m_VerticalAxis.m_MinValue = -80;
        aim.m_VerticalAxis.m_AccelTime = 0;
        aim.m_VerticalAxis.m_DecelTime = 0;
        aim.m_HorizontalAxis.m_AccelTime = 0;
        aim.m_HorizontalAxis.m_DecelTime = 0;

        //Enable the camera
        if (_vCam != null)
        {
            _vCam.enabled = true;
        }
    }

    #endregion

    #endregion
}
