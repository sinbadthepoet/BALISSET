using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class B_Shell : MonoBehaviour
{
    protected B_Ghost _Ghost;
    public Dictionary<string, ActionForBinding> ShellActions { get; protected set; }
    protected Transform CameraHolder = null;
    CinemachineVirtualCamera cinemachineVirtualCamera;

    public virtual void Possess(B_Ghost Ghost)
    {
        _Ghost = Ghost;
        if(Ghost is B_PlayerController)
        {
            BindVirtualCamera();
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public virtual void Release()
    {
        _Ghost = null;
        Cursor.lockState = CursorLockMode.None;
        if (cinemachineVirtualCamera != null)
        {
            cinemachineVirtualCamera.Follow = null;
            cinemachineVirtualCamera.DestroyCinemachineComponent<CinemachineHardLockToTarget>();
            cinemachineVirtualCamera.DestroyCinemachineComponent<CinemachineSameAsFollowTarget>();
            cinemachineVirtualCamera = null;
        }
    }

    protected virtual void BindVirtualCamera()
    {
        GameObject Cam = GameObject.Find("Player Virtual Camera");

        //If no virtual camera can be found, we can make one for now.
        if (Cam == null)
        {
            Cam = new GameObject("Player Virtual Camera", typeof(CinemachineVirtualCamera));

            Debug.Log("Player Virtual Camera not found in scene. Lazy initializing one.");
        }

        if (CameraHolder == null)
        {
            CameraHolder = new GameObject("Third Person Holder").transform;
            CameraHolder.parent = transform;
            CameraHolder.position = new Vector3(0.5f, 2, -3);

            Debug.Log($"{gameObject.name} has a null CameraHolder. Lazy initializing one.");
        }

        cinemachineVirtualCamera = Cam.GetComponent<CinemachineVirtualCamera>();

        cinemachineVirtualCamera.Follow = CameraHolder.transform;
        cinemachineVirtualCamera.AddCinemachineComponent<CinemachineHardLockToTarget>();
        cinemachineVirtualCamera.AddCinemachineComponent<CinemachineSameAsFollowTarget>();
    }

    protected virtual void InitializeActions()
    {
        //Change return value to return dictionary.
        ShellActions = new();
    }

    protected virtual void Awake()
    {
        InitializeActions();
    }

    protected virtual void OnDestroy()
    {
        if(_Ghost == null) { return; }
        _Ghost.Release();
    }
}