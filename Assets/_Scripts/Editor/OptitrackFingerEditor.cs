using _Scripts.OptiTrack;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OptitrackFinger))]
public class OpticTrackFingerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        OptitrackFinger optitrackFinger = (OptitrackFinger)target;
        if (GUILayout.Button("Recalibrate"))
        {
            optitrackFinger.FingerRecalibrate();
        };
    }
}