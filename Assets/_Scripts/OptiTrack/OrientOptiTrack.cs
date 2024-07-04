using _Scripts.GameState;
using UnityEngine;

namespace _Scripts.OptiTrack
{
    public class OrientOptiTrack : MonoBehaviour
    {
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;

        
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
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

            // Apply the rotation to the collider
            armUICapsuleCollider.transform.rotation = rotation;
        }
    }
}
