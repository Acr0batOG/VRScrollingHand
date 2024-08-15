using System.Collections;
using System.Collections.Generic;
using _Scripts.GameState;
using _Scripts.ListPopulator;
using UnityEngine;

namespace _Scripts.OptiTrack
{
    public class StarterAlignment : Singleton<StarterAlignment>
    {
        [SerializeField] private Transform armCollider;
        [SerializeField] public CapsuleCollider startCollider;
        [SerializeField] public MeshRenderer startRenderer;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        public ScrollableListPopulator scrollList;
        private GameManager gameManager;
        private GameStart gameStart;
        private StarterHandAlignment starterHandAlignment;

        private void Start()
        {
            gameManager = GameManager.instance;
            scrollList = ScrollableListPopulator.instance;
            gameStart = GameStart.instance;
            starterHandAlignment = StarterHandAlignment.instance;
            gameStart.DisableColliders();
            Renderer objectRenderer = GetComponent<Renderer>();
            objectRenderer.material.SetColor("_Color", Color.green);
        }

        // Update is called once per frame
        void Update()
        {
            
            if (gameStart.numberArrayIndex > gameStart.numberArray.Count)
            {
                gameStart.selectNumber.text = "No more items to select.";
            
                gameStart.stopGame = true;
            }

            //Debug.Log("Old "+ gameManager.PreviousTechnique + " Actual " + gameManager.TechniqueNumber );
            if(gameManager.PreviousArea!=gameManager.AreaNumber||gameManager.PreviousTechnique!=gameManager.TechniqueNumber)
            {
                gameStart.DisableColliders();
                scrollList.RemoveListItems();
                if (gameManager.AreaNumber == 1||gameManager.AreaNumber == 3)
                {
                    startCollider.enabled = true;
                    startRenderer.enabled = true;
                }else if (gameManager.AreaNumber == 2)
                {
                    starterHandAlignment.startCollider.enabled = true;
                    starterHandAlignment.startRenderer.enabled = true;
                }
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
            
            StartCoroutine(WaitBeforeLoadList());
            gameStart.SetNumber();

            StartCoroutine(Wait());
        }

        private IEnumerator Wait()
        {
            yield return new WaitForSeconds(.5f);
            gameManager.EnableSelectedController();
        }
        private IEnumerator WaitBeforeLoadList()
        {
            scrollList.InitArray();
            scrollList.RemoveListItems();
            yield return new WaitForSeconds(.2f);
            scrollList.InitArray();
            gameManager.InitalizeList = true;
            
        }
    }
}
