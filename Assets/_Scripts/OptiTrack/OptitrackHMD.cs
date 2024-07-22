using ubco.hci.OptiTrack;
using UnityEngine;


public class OptitrackHMD : OptitrackCustomSubject
{
    [SerializeField] private Vector3 EstimatedCentreOffset;
    
    protected override void UpdatePose()
    {
        //Needs to be tested and change axes accordingly
        //This math is specifically for a HMD
        Vector3 imaginaryCentre = CurrentPose.position + (CurrentPose.forward.normalized * EstimatedCentreOffset.z +
                                                          CurrentPose.up.normalized * EstimatedCentreOffset.y +
                                                          CurrentPose.right.normalized * EstimatedCentreOffset.x);
        transform.localPosition = imaginaryCentre;
        transform.rotation = CurrentPose.rotation;
    }
}
