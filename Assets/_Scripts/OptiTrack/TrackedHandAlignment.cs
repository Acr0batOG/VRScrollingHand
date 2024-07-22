using UnityEngine;

namespace ubco.ovilab.OptiTrack
{
    public class TrackedHandAlignment : MonoBehaviour
    {
        [SerializeField] private Transform handMesh;
        [SerializeField] private Transform handTracking;
        [SerializeField] private Transform optiTrackWrist;
        // Update is called once per frame
        void Update()
        {
             handMesh.position = optiTrackWrist.position; //Position the camera rig at the headset marker
             handTracking.position = optiTrackWrist.position;
        }
    }
}
