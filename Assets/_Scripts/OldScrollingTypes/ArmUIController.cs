using System.Collections;
using _Scripts.GameState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.OldScrollingTypes
{
    public class ArmUIController : MonoBehaviour
    {
        [SerializeField] protected Transform startPoint;
        [SerializeField] protected Transform endPoint;
        [SerializeField] protected ScrollRect scrollableList;
        [SerializeField] protected CapsuleCollider capsuleCollider;
        [SerializeField] protected TextMeshPro distText;
        [SerializeField] protected TextMeshProUGUI menuText;
        protected GameManager GameManager;
        protected Slider SelectionBar;
        protected TextMeshPro SelectText;
        protected float UserHeight;
        protected int AreaNum;
        protected int SelectedItem;
        protected readonly float ARMModify = 4.75f; //Works
        protected readonly float HandModify = 4.75f; //Works
        protected readonly float FingerModify = 5.0f; //Works
        protected readonly float FingertipModify = 7.5f; //Works, except tracking sucks
        protected readonly float ItemHeight = 55f; //Block item height
        protected float ItemCountMultiplier = 1.3f; //Multiplier for items
        protected float PreviousScrollPosition; // Previous scroll position for dwell check
        protected float ContentSize;
        protected int ItemCount;
        private float[] correctArray = new float [51];
        private float itemDistanceInit = (2454.621f/49f);
        

        protected void Start()
        {
            GameManager = GameManager.instance;
            AreaNum = GameManager.AreaNumber; // Get area being used
            SelectedItem = GameManager.SelectedItem;
            SelectionBar = GameObject.FindWithTag("SelectionBar").GetComponent<Slider>();
            InitializeArray();
            SelectionBar.value = 0f;
            ItemCount = GameManager.NumberOfItems;
            
            // switch (AreaNum) // Switch to set the start and end points
            // {
            //     case 1:
            //         startPoint = GameObject.FindWithTag("Elbow").transform; // Arm scrolling
            //         endPoint = GameObject.FindWithTag("Wrist").transform;
            //         break;
            //     case 2:
            //         startPoint = GameObject.FindWithTag("Wrist").transform; //Hand scrolling
            //         endPoint = GameObject.FindWithTag("LFingertip").transform;
            //         break;
            //     case 3:
            //         startPoint = GameObject.FindWithTag("Fingerbase").transform; //Finger scrolling
            //         endPoint = GameObject.FindWithTag("RFingertip").transform;
            //         break;
            //     case 4:
            //         startPoint = GameObject.FindWithTag("Fingermid").transform; //Fingertip scrolling
            //         endPoint = GameObject.FindWithTag("RFingertip").transform;
            //         break;
            //}
        }

        void Update()
        {
            // Calculate the middle point between startPoint and endPoint
            Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;
            Vector3 direction = (endPoint.position - startPoint.position).normalized;
            float distance = Vector3.Distance(startPoint.position, endPoint.position);
            UserHeight = GameManager.UserHeight; // Get height of character
            int previousArea = AreaNum;
            AreaNum = GameManager.AreaNumber;
            SelectedItem = GameManager.SelectedItem;
            ItemCount = GameManager.NumberOfItems; //Replace itemCount if NumberOfItems ever changes

            // AreaCheck(previousArea, AreaNum); //Check if area has changed
            //
            // switch (AreaNum) // Switch to set the start and end points
            // {
            //     case 1: // Arm scrolling
            //         OrientCollider(capsuleCollider, middlePoint, direction, distance, UserHeight, ARMModify);
            //         break;
            //     case 2: // Hand scrolling
            //         OrientCollider(capsuleCollider, middlePoint, direction, distance, UserHeight, HandModify);
            //         break;
            //     case 3: // Finger scrolling. Shift the capsule slightly
            //         OrientCollider(capsuleCollider, middlePoint, direction, distance, UserHeight, FingerModify);
            //         break;
            //     case 4: // Fingertip scrolling
            //         OrientCollider(capsuleCollider, middlePoint, direction, distance, UserHeight, FingertipModify);
            //         break;
            // }
        }

        // void OrientCollider(CapsuleCollider armUICapsuleCollider, Vector3 middlePoint, Vector3 direction, float distance, float armUiUserHeight, float multiplier)
        // {
        //     armUICapsuleCollider.center = Vector3.zero; // Reset center to origin
        //     armUICapsuleCollider.height = distance * armUiUserHeight * multiplier; // Set height based on distance and user height
        //     armUICapsuleCollider.transform.position = middlePoint; // Position the collider at the midpoint
        //     // Set the direction of the collider to Y-axis
        //     armUICapsuleCollider.direction = 1; // 0 for X, 1 for Y, 2 for Z
        //
        //     // Calculate the rotation to align with the direction vector
        //     Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);
        //
        //     // Apply the rotation to the collider
        //     armUICapsuleCollider.transform.rotation = rotation;
        // }
        // void AreaCheck(int previousArea, int armUIAreaNum){
        //     if(armUIAreaNum!=previousArea){
        //         switch (armUIAreaNum) // Switch to set the start and end points
        //         {
        //             case 1:
        //                 startPoint = GameObject.FindWithTag("Elbow").transform; // Arm scrolling
        //                 endPoint = GameObject.FindWithTag("Wrist").transform;
        //                 break;
        //             case 2:
        //                 startPoint = GameObject.FindWithTag("Wrist").transform; //Hand scrolling
        //                 endPoint = GameObject.FindWithTag("LFingertip").transform;
        //                 break;
        //             case 3:
        //                 startPoint = GameObject.FindWithTag("Fingerbase").transform; //Finger scrolling
        //                 endPoint = GameObject.FindWithTag("RFingertip").transform;
        //                 break;
        //             case 4:
        //                 startPoint = GameObject.FindWithTag("Fingermid").transform; //Fingertip scrolling
        //                 endPoint = GameObject.FindWithTag("RFingertip").transform;
        //                 break;
        //         }
        //     }
        // } 

        protected virtual void Scroll(Collider fingerCollider)
        {
            //Just to be inherited
        }
        
       
            
           
     


       
        void InitializeArray()
        {
            for (int i = 0; i <= 50; i++)
            {
                correctArray[i] = i * itemDistanceInit - 25f;
            }
        }
        protected void SelectItem()
        {

            GameObject selectTextObject = GameObject.FindWithTag("ItemSelect"); //Get item to show selection
            float currentPositionY = scrollableList.content.anchoredPosition.y;

            // Find the index of the color range that currentPositionY is within
            for (int i = 0; i < correctArray.Length - 1; i++)
            {
                if (currentPositionY >= correctArray[i] && currentPositionY <= correctArray[i + 1])
                {
                    SelectedItem = i + 1;
                }
            }
            
            GameManager.SelectedItem = SelectedItem;
            // Calculate the selected item index based on the scroll position and item height
            // Check if the GameObject was found
            if (selectTextObject)
            {
                // Get the TextMeshProUGUI component from the GameObject
                SelectText = selectTextObject.GetComponent<TextMeshPro>();

                // Check if the component was found
                if (SelectText)
                {
                    // Set text to the item selected
                    SelectText.text = "Item Selected: " + SelectedItem;
                }
            }
        }
    }
}
