using System;
using UnityEngine;

namespace OptiTrack
{
    public class OpticTrackAlignment : MonoBehaviour
    {
        [SerializeField] private Transform headsetTransform;
        [SerializeField] private Transform ovrCameraRig;
       
        void Start()
        {
            Vector3 headsetVector = headsetTransform.position; //Position of the headset markers
            ovrCameraRig.position = headsetVector; //Position the camera rig at the headset markers
        }
        
        void Update()
         {
            Vector3 headsetVector = headsetTransform.position; //Position of the headset markers
            ovrCameraRig.position = headsetVector; //Position the camera rig at the headset marker
            //Debug.Log(Vector3.Distance(ovrCameraRig.position, headsetTransform.position));
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawSphere(ovrCameraRig.position, 0.1f);
        }
    }
}