using ubco.hci.OptiTrack;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OptitrackElbow))]
public class OpticTrackElbowEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        OptitrackElbow optitrackElbow = (OptitrackElbow)target;
        if (GUILayout.Button("Recalibrate"))
        {
            optitrackElbow.ElbowRecalibrate();
        };
    }
}