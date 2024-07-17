using UnityEngine;
using UnityEngine.XR;

public class HandAlignment : MonoBehaviour
{
    public Transform openXRHandTransform; // The transform of the OpenXR hand
    public Transform optiTrackWristTransform; // The transform of the OptiTrack wrist object

    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    void Start()
    {
        // Calculate the initial offset between OpenXR hand and OptiTrack wrist
        positionOffset = optiTrackWristTransform.position - openXRHandTransform.position;
        rotationOffset = Quaternion.Inverse(openXRHandTransform.rotation) * optiTrackWristTransform.rotation;
    }

    void Update()
    {
        // Update the position and rotation of the OpenXR hand to align with the OptiTrack wrist
        Vector3 alignedPosition = optiTrackWristTransform.position - positionOffset;
        Quaternion alignedRotation = optiTrackWristTransform.rotation * Quaternion.Inverse(rotationOffset);

        // Apply the aligned position and rotation to the OpenXR hand transform
        openXRHandTransform.position = alignedPosition;
        openXRHandTransform.rotation = alignedRotation;
    }
}