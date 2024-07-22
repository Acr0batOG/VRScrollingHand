using ubco.hci.OptiTrack;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OptitrackCustomSubject), true)]
public class OptitrackCustomSubjectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        OptitrackCustomSubject optitrackBody = (OptitrackCustomSubject)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Recalibrate"))
        {
            optitrackBody.Recalibrate();
        };
    }
}