using ubco.hci.OptiTrack;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OptitrackWrist))]
public class OpticTrackWristEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        OptitrackWrist optitrackWrist = (OptitrackWrist)target;
        if (GUILayout.Button("Recalibrate"))
        {
            optitrackWrist.WristRecalibrate();
        };
    }
}