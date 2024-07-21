using System;
using ubco.hci.OptiTrack;
using UnityEngine;


/// <summary>
/// Implements live tracking of streamed OptiTrack rigid body data onto an object.
/// </summary>
public class OptitrackHMD : OptitrackCustomSubject
{
    [Tooltip("Change Scale of movement")] [SerializeField]
    private float movementScale;

    private Vector3 origin;
    
    protected void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    protected void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        UpdatePose();
    }
    
    protected override void Update()
    {
        FetchPose();
        UpdatePose();
    }

    public void HMDRecalibrate()
    {
        origin = referenceTransform.transform.position;
    }
}
