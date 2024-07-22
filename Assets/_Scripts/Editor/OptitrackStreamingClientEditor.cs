using UnityEditor;
using UnityEngine;


namespace ubco.ovilab.HPUI.Editor
{
    [CustomEditor(typeof(OptitrackStreamingClient), true)]
    public class OptitrackStreamingClientEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            OptitrackStreamingClient optitrackBody = (OptitrackStreamingClient)target;
            GUILayout.Space(10);
            if (GUILayout.Button("Recalibrate All Subjects"))
            {
                optitrackBody.Recalibrate();
            }
        }
    }
}