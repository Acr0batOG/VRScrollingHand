using _Scripts.GameState;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.OptiTrack
{
    public class OrientOptiTrack : MonoBehaviour
    {
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private bool isHand;
        
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
            // Calculate the middle point between startPoint and endPoint
            Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;
            Vector3 direction = (endPoint.position - startPoint.position).normalized;
            float distance = Vector3.Distance(startPoint.position, endPoint.position);
            OrientCollider(armCollider, middlePoint, direction, distance, isHand);
        }

        private void OrientCollider(CapsuleCollider armUICapsuleCollider, Vector3 middlePoint, Vector3 direction, float distance, bool hand)
        {
            armUICapsuleCollider.center = Vector3.zero; // Reset center to origin
            armUICapsuleCollider.height = distance * gameManager.UserHeight * multiplier; // Set height based on distance and user height
            armUICapsuleCollider.transform.position = middlePoint; // Position the collider at the midpoint
            // Set the direction of the collider to Y-axis
            armUICapsuleCollider.direction = 1; // 0 for X, 1 for Y, 2 for Z
            Quaternion rotation;
            // Calculate the rotation to align with the direction vector
            if (!hand)
            {
                rotation = Quaternion.FromToRotation(Vector3.up, direction);
            }
            else
            {
                rotation = Quaternion.FromToRotation(Vector3.back, direction);

                // Create a 180-degree rotation quaternion around the Y-axis (or any axis perpendicular to the original direction)
                Quaternion rotation180 = Quaternion.Euler(0, 180, 0);

                // Multiply the original rotation by the 180-degree rotation
                rotation *= rotation180;
            }

            // Apply the rotation to the collider
            armUICapsuleCollider.transform.rotation = rotation;
        }
    }
}
