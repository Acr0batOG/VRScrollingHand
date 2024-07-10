using UnityEngine;

namespace _Scripts.OptiTrack
{
    public class HandAlignment : MonoBehaviour
    { 
        [SerializeField] private Transform scrollingFinger;
        [SerializeField] private Transform otherWrist;
        [SerializeField] private Transform otherFinger;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;
        
        void Update()
        {
            // Calculate midpoint between otherWrist and otherFinger
            Vector3 midpoint = (otherWrist.position + otherFinger.position) / 2f;
                
            // Position leftHand halfway between otherWrist and otherFinger
            leftHand.position = midpoint;
                
            // Rotate leftHand to align with the direction from otherWrist to otherFinger
            leftHand.rotation = Quaternion.LookRotation(otherFinger.position - otherWrist.position);
            
            // Position rightHand at the center of scrollingFinger
            rightHand.position = scrollingFinger.position - new Vector3(.2f, 0f, .2f);
        }
    }
}
