using System;
using System.Collections.Generic;
using UnityEngine;

namespace ubco.ovilab.OptiTrack
{
    [CreateAssetMenu(fileName = "SystemAxis", menuName = "SystemOrientation/AxisRemapper", order = 0)]
    public class SystemAxisSO : ScriptableObject
    {
        public MocapSystem MocapSystem;
        // public Axes XAxis;
        // public Axes YAxis;
        // public Axes ZAxis;
        public float movementScale;
        
        public static void FetchAxisMapping(Pose pose)
        {
            
        }
    }
    
    [Serializable]
    public enum MocapSystem
    {
        Invalid = -1,
        Vicon = 0,
        OptiTrack = 1
    }

    public enum Axes
    {
        PositiveX = 0,
        PositiveY = 1,
        PositiveZ = 2,
        NegativeX = 3,
        NegativeY = 4,
        NegativeZ = 6,
    }
}
