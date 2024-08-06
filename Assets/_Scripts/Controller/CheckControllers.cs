using UnityEngine;

namespace _Scripts.Controller
{
    public class CheckControllers : MonoBehaviour
    {
  
        void Start()
        {
            CheckController();
        }

        void CheckController()
        {
            if (OVRInput.IsControllerConnected(OVRInput.Controller.RTouch))
            {
                Debug.Log("Right controller connected");
            }
            else
            {
                Debug.LogError("Right controller not connected");
            }

            if (OVRInput.IsControllerConnected(OVRInput.Controller.LTouch))
            {
                Debug.Log("Left controller connected");
            }
            else
            {
                Debug.LogError("Left controller not connected");
            }
        }
    }
}

