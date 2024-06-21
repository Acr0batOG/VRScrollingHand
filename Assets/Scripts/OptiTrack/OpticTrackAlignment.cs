using UnityEngine;

namespace OptiTrack
{
    public class OpticTrackAlignment : MonoBehaviour
    {
        [SerializeField] private Transform headsetTransform;
        [SerializeField] private Transform ovrCameraRig;
        [SerializeField] private Transform highFidelityRig;
       
        void Start()
        {
            Vector3 headsetVector = headsetTransform.position; //Position of the headset markers
            ovrCameraRig.position = headsetVector - new Vector3(0.0f, 1.8f, 0.0f); //Position the camera rig at the headset markers
            highFidelityRig.position = headsetVector - new Vector3(0.0f, 1.8f, 0.0f);

        }
        
        void Update()
        {
            Vector3 headsetVector = headsetTransform.position; //Position of the headset markers
            ovrCameraRig.position = headsetVector - new Vector3(0.0f, 1.8f, 0.0f); //Position the camera rig at the headset markers
            highFidelityRig.position = headsetVector - new Vector3(0.0f, 1.8f, 0.0f);
        }
    }
}