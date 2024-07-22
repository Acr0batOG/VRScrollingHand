using System;
using UnityEngine;

namespace ubco.hci.OptiTrack
{
    public class OptitrackCustomSubject : MonoBehaviour
    {
        public Pose CurrentPose => currentPose;
        
        [Header("Optitrack Settings")]
        [Tooltip(("Provide a Motion Capture System Config"))] [SerializeField]
        protected SystemAxisSO systemConfig;
            
        [HideInInspector]
        public OptitrackStreamingClient StreamingClient;

        [Tooltip("The Streaming ID of the rigid body in Motive")]
        public Int32 RigidBodyId;

        [Tooltip("Subscribes to this asset when using Unicast streaming.")]
        public bool NetworkCompensation = true;
        
        [Header("Other Settings")]
        [Tooltip("Use for referencing an XR Origin")][SerializeField] 
        protected Transform referenceTransform;
        
        private Pose currentPose;
        protected Vector3 origin;

        #region UnityFunctions
        
        protected virtual void OnEnable()
        {
            Application.onBeforeRender += OnBeforeRender;
        }
        
        protected virtual void Start()
        {
            RegisterClient();
            if (referenceTransform == null)
            {
                Debug.Log("No reference Transform Assigned, Using Streaming Client as default");
                referenceTransform = StreamingClient.transform;
            }
            origin = referenceTransform.position;
        }
        
        protected virtual void Update()
        {
            FetchPose();
            UpdatePose();
        }
        

        protected virtual void OnDisable()
        {
            Application.onBeforeRender -= OnBeforeRender;
        }


        protected virtual void OnBeforeRender()
        {
            UpdatePose();
        }

        #endregion
        
        private void RegisterClient()
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
        

        protected virtual void FetchPose()
        {
            OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation);
            if (rbState == null)
            {
                Debug.LogError($"Lost Tracking of the Rigid Body : {RigidBodyId}");
                return;
            }
            
            //Todo: add some utility functions to change the config easier
            currentPose.position = new Vector3(-rbState.Pose.Position.x, rbState.Pose.Position.y, -rbState.Pose.Position.z);
            currentPose.rotation = rbState.Pose.Orientation;
        }

        protected virtual void UpdatePose()
        {
            transform.localPosition = origin + currentPose.position * systemConfig.movementScale;
            transform.localRotation = currentPose.rotation;
        }

        public virtual void Recalibrate()
        {
            origin = referenceTransform.position;
        }
    }
}