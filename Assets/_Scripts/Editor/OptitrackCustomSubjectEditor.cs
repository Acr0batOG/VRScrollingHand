using UnityEditor;
using UnityEngine;
using ubco.ovilab.OptiTrack;

namespace ubco.ovilab.HPUI.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(OptitrackCustomSubject), true)]
    public class OptitrackCustomSubjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            OptitrackCustomSubject optitrackBody = (OptitrackCustomSubject)target;
            GUILayout.Space(10);
            if (GUILayout.Button("Recalibrate"))
            {
                optitrackBody.Recalibrate();
            }
        }
    } 
}
