using System;
using System.Collections;
using System.ComponentModel.Design;
using _Scripts.GameState;
using _Scripts.ListPopulator;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.OptiTrack
{
    public class StarterAlignment : MonoBehaviour
    {
        [SerializeField] private Transform armCollider;
        [SerializeField] private CapsuleCollider startCollider;
        [SerializeField] private MeshRenderer startRenderer;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        private ScrollableListPopulator scrollList;
        private GameManager gameManager;
        private GameStart gameStart;

        private void Start()
        {
            gameManager = GameManager.instance;
            scrollList = ScrollableListPopulator.instance;
            gameStart = GameStart.instance;
            Renderer objectRenderer = GetComponent<Renderer>();
            objectRenderer.material.SetColor("_Color", Color.green);
        }

        // Update is called once per frame
        void Update()
        {

            //Debug.Log("Old "+ gameManager.PreviousTechnique + " Actual " + gameManager.TechniqueNumber );
            if(gameManager.PreviousArea!=gameManager.AreaNumber||gameManager.PreviousTechnique!=gameManager.TechniqueNumber){
                startCollider.enabled = true;
                startRenderer.enabled = true;
                scrollList.RemoveListItems();
                gameManager.PreviousArea = gameManager.AreaNumber;
                gameManager.PreviousTechnique = gameManager.TechniqueNumber;
            }
            // Calculate the middle point between startPoint and endPoint
            Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;
            Vector3 direction = (endPoint.position - startPoint.position).normalized;
            OrientCollider(startCollider, middlePoint, direction);
          
        }

        private void OrientCollider(CapsuleCollider armUICapsuleCollider, Vector3 middlePoint, Vector3 direction)
        {
            armUICapsuleCollider.center = Vector3.zero; // Reset center to origin
           // armUICapsuleCollider.height = distance * gameManager.UserHeight; // Set height based on distance and user height
            armUICapsuleCollider.transform.position = middlePoint; // Position the collider at the midpoint
            armUICapsuleCollider.transform.localScale = new Vector3(.08f, .01f, .08f);
            // Set the direction of the collider to Y-axis
            armUICapsuleCollider.direction = 1; // 0 for X, 1 for Y, 2 for Z
            
            // Calculate the rotation to align with the direction vector
            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
            
            // Apply the rotation to the collider
            armUICapsuleCollider.transform.rotation = rotation * Quaternion.Euler(0, 0, 10);
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
            
                Renderer objectRenderer = GetComponent<Renderer>();
                objectRenderer.material.SetColor("_Color", Color.magenta);
        
        }

        private void OnCollisionExit(Collision other)
        {
            startCollider.enabled = false;
            startRenderer.enabled = false;
            Debug.Log("Init Array");
            Renderer objectRenderer = GetComponent<Renderer>();
            objectRenderer.material.SetColor("_Color", Color.green);
            scrollList.InitArray();
            gameManager.InitalizeList = true;
            gameStart.SetNumber();
            gameManager.EnableSelectedController();
        }
    }
}
