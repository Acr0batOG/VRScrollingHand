using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.GameState
{
    public class MakeSelection : MonoBehaviour
    {
        [SerializeField] protected ScrollRect scrollableList;
        protected GameManager GameManager;
        protected TextMeshPro SelectText;
        protected int SelectedItem;
        protected readonly float ItemHeight = 55f; //Block item height
        protected float ItemCountMultiplier = 1.3f; //Multiplier for items
        protected float ContentSize;
        protected int ItemCount;
        protected Slider SelectionBar;
        private float[] correctArray = new float [51];
        private float itemDistanceInit = (2454.621f/49f);
        void Start()
        {
            GameManager = GameManager.instance;
            SelectedItem = GameManager.SelectedItem;
            SelectionBar = GameObject.FindWithTag("SelectionBar").GetComponent<Slider>();
            SelectionBar.value = 0f;
            InitializeArray();
            ItemCount = GameManager.NumberOfItems;
        }
        
        void Update()
        {
        
        }
        void InitializeArray()
        {
            for (int i = 0; i <= 50; i++)
            {
                correctArray[i] = i * itemDistanceInit - 25f;
            }
        }

        private void OnCollisionEnter(Collision other)
        {
           
                
                SelectItem();
        }

        private void OnCollisionExit(Collision other)
        {
            
        }

        private void OnCollisionStay(Collision other)
        {
          
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

            // float viewportHeight = scrollableList.viewport.rect.height;
            // SelectionBar.value = 1f;
            // // Calculate the total height of the list content
            // ContentSize = ItemHeight * ItemCount;
            // float relativeScrollPosition =
            //     scrollableList.content.anchoredPosition.y /
            //     (ContentSize - viewportHeight); //CUrrent scroll psoition of current list 
            // // Calculate the relative scroll position within the content
            // float halfwayHeight =
            //     (ContentSize - viewportHeight) / 2f; //Halfway point to see if we meed to add an adjustment factor
            //
            // int targetValue = 1; //Target for distance calculations
            // float distanceFromTarget =
            //     Mathf.Abs(relativeScrollPosition * ItemCount - targetValue); //Distance for adjustment calculation
            //
            // // Factor to increase the adjustment based on distance from target
            // float adjustmentFactor =
            //     distanceFromTarget /
            //     (ItemCount * ItemCountMultiplier); //Subtraction for items greater than 25 to align the selections
            // //Debug.Log("ADJ" + adjustmentFactor); //For testing
            // //Debug.Log(relativeScrollPosition * itemCount + 1);
            // if (halfwayHeight < relativeScrollPosition)
            // {
            //
            //     SelectedItem = Mathf.Clamp(Mathf.RoundToInt(relativeScrollPosition * ItemCount + 1), 1,
            //         ItemCount); //Get item selected
            // }
            // else
            // {
            //     SelectedItem =
            //         Mathf.Clamp(Mathf.RoundToInt(relativeScrollPosition * ItemCount + 1 - adjustmentFactor), 1,
            //             ItemCount); //Get item selected minus a factor
            // }

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
