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
        [SerializeField] private bool isVisible;
        [SerializeField] private CapsuleCollider armCollider;
        [SerializeField] private float multiplier = 5.2f;
        [SerializeField] private float mul = 1.5f;
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
            OrientCollider(armCollider, middlePoint, direction, distance, isHand, isVisible);
        }

        private void OrientCollider(CapsuleCollider armUICapsuleCollider, Vector3 middlePoint, Vector3 direction, float distance, bool hand, bool visible)
        {
            armUICapsuleCollider.center = Vector3.zero; // Reset center to origin
            armUICapsuleCollider.height = distance * gameManager.UserHeight * multiplier; // Set height based on distance and user height
            armUICapsuleCollider.transform.position = middlePoint; // Position the collider at the midpoint
            
            // Set the direction of the collider to Y-axis
            armUICapsuleCollider.direction = 1; // 0 for X, 1 for Y, 2 for Z
            
            // Calculate the rotation to align with the direction vector
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction); 
                

            if (hand)
            {
                armUICapsuleCollider.transform.position -= new Vector3(0.025f, 0.0f, 0.0f);
            }
            // If visible, update the local scale
            if (visible)
            {
                Transform armTransform = armUICapsuleCollider.transform;
                Vector3 newScale = armTransform.localScale;
                newScale.y = distance / mul; //Good
                newScale.x = gameManager.UserHeight / 24f;
                newScale.z = gameManager.UserHeight / 24f;
                armTransform.localScale = newScale;
                rotation = Quaternion.FromToRotation(Vector3.up, direction) * Quaternion.Euler(0, 0, 15);
            }
            
            // Apply the rotation to the collider
            armUICapsuleCollider.transform.rotation = rotation;
        }
    }
}
