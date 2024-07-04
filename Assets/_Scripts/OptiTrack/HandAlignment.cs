using UnityEngine;

namespace _Scripts.OptiTrack
{
    public class HandAlignment : MonoBehaviour
    {
        [SerializeField] private Transform wristTransform;
        [SerializeField] private Transform leftHand;
       
        void Start()
        {
            Vector3 wristVector = wristTransform.position; //Position of the headset markers
            leftHand.position = wristVector; //Position the camera rig at the headset markers
        }
        
        void Update()
        {
            Vector3 wristVector = wristTransform.position; //Position of the headset markers
            leftHand.position = wristVector; //Position the camera rig at the headset marker
        }
    }
}
