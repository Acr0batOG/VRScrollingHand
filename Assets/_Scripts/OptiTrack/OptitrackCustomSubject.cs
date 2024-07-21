using System;
using UnityEngine;

namespace ubco.hci.OptiTrack
{
    public class OptitrackCustomSubject : MonoBehaviour
    {
        public Pose CurrentPose => currentPose;

        [Tooltip(("Provide a Motion Capture System Config"))] [SerializeField]
        protected SystemAxisSO systemConfig;
            
        [Tooltip("The object containing the OptiTrackStreamingClient script.")]
        public OptitrackStreamingClient StreamingClient;

        [Tooltip("The Streaming ID of the rigid body in Motive")]
        public Int32 RigidBodyId;

        [Tooltip("Subscribes to this asset when using Unicast streaming.")]
        public bool NetworkCompensation = true;
        
        [Tooltip("Use for referencing an XR Origin")][SerializeField] 
        protected Transform referenceTransform;
        
        private Pose currentPose;
        protected virtual void Start()
        {
            if (StreamingClient == null)
            {
                StreamingClient = OptitrackStreamingClient.FindDefaultClient();
                if (StreamingClient == null)
                {
                    Debug.LogError(GetType().FullName + ": Streaming client not set, and no " +
                                   typeof(OptitrackStreamingClient).FullName +
                                   " components found in scene; disabling this component.", this);
                    enabled = false;
                    return;
                }
            }
            StreamingClient.RegisterRigidBody(this, RigidBodyId);
        }


        protected virtual void Update()
        {
            FetchPose();
            UpdatePose();
        }


        protected virtual void FetchPose()
        {
            OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation);
            if (rbState == null)
            {
                Debug.LogError($"Lost Tracking of the Rigid Body : {RigidBodyId}");
                return;
            }
            currentPose.position = new Vector3(-rbState.Pose.Position.x, rbState.Pose.Position.y, -rbState.Pose.Position.z);
            currentPose.rotation = rbState.Pose.Orientation;
        }

        protected virtual void UpdatePose()
        {
            transform.localPosition = currentPose.position * systemConfig.movementScale;
            transform.localRotation = currentPose.rotation;
        }
    }
}