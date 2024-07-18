using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CustomEditor(typeof(OptitrackHMD))]
    public class OpticTrackHMDEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            OptitrackHMD optitrackHMD = (OptitrackHMD)target;
            if (GUILayout.Button("Recalibrate"))
            {
                optitrackHMD.HMDRecalibrate();
            };
        }
    }
}