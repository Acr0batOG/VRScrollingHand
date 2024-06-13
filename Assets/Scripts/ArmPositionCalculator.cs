using UnityEngine;

public static class ArmPositionCalculator
{
    /// <summary>
    /// calculates vector from elbow to finger
    /// projects elbow2Finger onto elbow2Wrist
    /// divides elbow2Finger.mag by elbow2Wrist.mag and returns float
    /// </summary>
    /// <param name="wristPivot">current world position of wrist</param>
    /// <param name="elbowPivot">current world position of elbow</param>
    /// <param name="fingerPosition">current world position of finger</param>
    /// <returns>normalised distance value of finger on arm</returns>
    public static float GetNormalisedPositionOnArm(Vector3 wristPivot, Vector3 elbowPivot, Vector3 fingerPosition)
    {
        Vector3 elbowToWrist = wristPivot - elbowPivot;
        Vector3 elbowToFinger = fingerPosition - elbowPivot;
        Vector3 elbowToFingerOnArm = Vector3.Project(elbowToFinger, elbowToWrist);
        return elbowToFingerOnArm.magnitude / elbowToWrist.magnitude;
    }
}
