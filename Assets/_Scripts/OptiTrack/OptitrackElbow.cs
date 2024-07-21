using System;
using UnityEngine;

namespace ubco.hci.OptiTrack
{
/// <summary>
/// Implements live tracking of streamed OptiTrack rigid body data onto an object.
/// </summary>
public class OptitrackElbow : OptitrackCustomSubject
{
    [Tooltip("Change Scale of movement")] [SerializeField]
    private float movementScale;

    private Vector3 origin;

    [SerializeField] private Transform Elbow;

    void Start()
    {
        // If the user didn't explicitly associate a client, find a suitable default.
        if (this.StreamingClient == null)
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if (this.StreamingClient == null)
            {
                Debug.LogError(
                    GetType().FullName + ": Streaming client not set, and no " +
                    typeof(OptitrackStreamingClient).FullName +
                    " components found in scene; disabling this component.", this);
                this.enabled = false;
                return;
            }
        }

        this.StreamingClient.RegisterRigidBody(this, RigidBodyId);
    }


#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        UpdatePose();
    }
#endif


    void Update()
    {
        UpdatePose();
    }


    void UpdatePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation);
        if (rbState == null) return;

        var newPosition = new Vector3(-rbState.Pose.Position.x, rbState.Pose.Position.y, -rbState.Pose.Position.z);

        transform.position = origin + newPosition * movementScale;
        transform.localRotation = rbState.Pose.Orientation;
    }

    public void ElbowRecalibrate()
    {
        origin = Elbow.transform.position;
    }
}
}
