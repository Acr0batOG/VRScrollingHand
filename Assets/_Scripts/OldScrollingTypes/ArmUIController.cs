using System.Collections;
using System.Collections.Generic;
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
        protected GameManager gameManager;
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
        
        // Metrics tracking variables
        protected float totalAmplitudeOfSwipe = 0f; // Logged
        protected float swipeAmplitude = 0f; // Not Logged
        protected float totalSwipeTime = 0f; // Not logged
        protected int numberOfFlicks = 0; //Logged
        protected float lastSwipeTime = 0f; //Not logged
        protected float averageSwipeSpeed = 0f;
        
        protected float timeBetweenSwipes = 0f;

        protected List<float> timeBetweenSwipesArray; //Logged
        protected int previousSelectedItem = 0;
        protected float trialStartTime; // Not logged
        protected float[] correctArray = new float [51];
        private float itemDistanceInit = (2454.621f/49f);

        protected void Start()
        {
            gameManager = GameManager.instance;
            AreaNum = gameManager.AreaNumber; // Get area being used
            SelectedItem = gameManager.SelectedItem;
            SelectionBar = GameObject.FindWithTag("SelectionBar").GetComponent<Slider>();
            InitializeArray();
            SelectionBar.value = 0f;
            ItemCount = gameManager.NumberOfItems;
            timeBetweenSwipesArray = new List<float>();
            
        }

        void Update()
        {
            // Calculate the middle point between startPoint and endPoint
            Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;
            Vector3 direction = (endPoint.position - startPoint.position).normalized;
            float distance = Vector3.Distance(startPoint.position, endPoint.position);
            UserHeight = gameManager.UserHeight; // Get height of character
            int previousArea = AreaNum;
            AreaNum = gameManager.AreaNumber;
            SelectedItem = gameManager.SelectedItem;
            ItemCount = gameManager.NumberOfItems; //Replace itemCount if NumberOfItems ever changes
            
            
            //Logged
            
            
            
            if (gameManager.SelectedItem != previousSelectedItem)
            {
                timeBetweenSwipesArray.Clear();
                numberOfFlicks = 0;
                totalAmplitudeOfSwipe = 0f;
            }
            
            previousSelectedItem = gameManager.SelectedItem;
            
            

           
        }

        

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
            
            gameManager.SelectedItem = SelectedItem;
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
