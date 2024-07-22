using _Scripts.GameState;
using UnityEngine;

namespace ubco.ovilab.OptiTrack
{
    public class OrientRightHand : MonoBehaviour
    {
 
        private Vector3 startPoint;
        [SerializeField] Transform endPoint;
        
        [SerializeField] private CapsuleCollider armCollider;
        [SerializeField] private float multiplier = 5.2f;
        private GameManager gameManager;
    
        // Start is called before the first frame update
        void Start()
        {
            gameManager = GameManager.instance;
        }

        // Update is called once per frame
        void Update()
        {
            
            startPoint = endPoint.position - new Vector3(.005f, 0f, .12f);
            // Calculate the middle point between startPoint and endPoint
            Vector3 middlePoint = (startPoint + endPoint.position) / 2f;
            Vector3 direction = (endPoint.position - startPoint).normalized;
            float distance = Vector3.Distance(startPoint, endPoint.position);
            OrientCollider(armCollider, middlePoint, direction, distance);
        }

        private void OrientCollider(CapsuleCollider armUICapsuleCollider, Vector3 middlePoint, Vector3 direction, float distance)
        {
            armUICapsuleCollider.center = Vector3.zero; // Reset center to origin
            armUICapsuleCollider.height = distance * gameManager.UserHeight * multiplier; // Set height based on distance and user height
            armUICapsuleCollider.transform.position = middlePoint; // Position the collider at the midpoint
            // Set the direction of the collider to Y-axis
            armUICapsuleCollider.direction = 1; // 0 for X, 1 for Y, 2 for Z
           
            // Calculate the rotation to align with the direction vector
              Quaternion  rotation = Quaternion.FromToRotation(Vector3.back, direction);

            // Create a 180-degree rotation quaternion around the Y-axis (or any axis perpendicular to the original direction)
            Quaternion rotation180 = Quaternion.Euler(0, 180, 0);

            // Multiply the original rotation by the 180-degree rotation
            rotation *= rotation180;
            

            // Apply the rotation to the collider
            armUICapsuleCollider.transform.rotation = rotation;
        }
    }
}
