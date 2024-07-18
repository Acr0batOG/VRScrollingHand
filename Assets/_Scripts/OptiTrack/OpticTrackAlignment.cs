using UnityEngine;

namespace _Scripts.OptiTrack
{
    public class OpticTrackAlignment : MonoBehaviour
    {
        [SerializeField] private Transform optitrackHMD;
        [SerializeField] private Transform XRCameraRig;

        [SerializeField] private Transform leftHand;

        private Vector3 xrOptitrackDifference;
        
        void Update()
        {
            xrOptitrackDifference = optitrackHMD.position - XRCameraRig.position;
            leftHand.position += xrOptitrackDifference;
        }
        
    }
}