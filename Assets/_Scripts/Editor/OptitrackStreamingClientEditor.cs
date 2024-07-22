using ubco.hci.OptiTrack;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OptitrackStreamingClient), true)]
public class OptitrackStreamingClientEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        OptitrackStreamingClient optitrackBody = (OptitrackStreamingClient)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Recalibrate All Subjects"))
        {
            optitrackBody.Recalibrate();
        };
    }
}