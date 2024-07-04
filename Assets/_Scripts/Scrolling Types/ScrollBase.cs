using System.Collections;
using _Scripts.GameState;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.Scrolling_Types
{
    public class ScrollBase : MonoBehaviour
    {
        [SerializeField] protected CapsuleCollider capsuleCollider;
        protected GameManager gameManager;
        protected int itemCount;
        
        [Header("Pivots")] 
        [SerializeField] protected Transform startPoint;
        [SerializeField] protected Transform endPoint;
        
        [Header("UI Components")]
        [SerializeField] protected ScrollRect scrollableList;
        [SerializeField] protected TextMeshProUGUI menuText;
        [SerializeField] protected Slider selectionBar;
        [SerializeField] protected TextMeshPro selectText;
        [SerializeField] protected TextMeshPro distText;
        [SerializeField] protected float UIScrollSpeed = 3100f; //Speed multiplier for static scrolling
        [SerializeField] protected float contentHeight;
        [SerializeField] protected float viewportHeight;
        
        [Header("Parameters")]
        [SerializeField] protected float userHeight; 
        [SerializeField] protected int areaNum;
        [SerializeField] protected int selectedItem;
        [SerializeField] protected float scrollMultiplier = 2.1f;
        [SerializeField] protected float dwellThreshold = 10f;
        [SerializeField] protected float dwellTime = 2.2f;
        [SerializeField] float itemCountMultiplier = 1.3f; //Multiplier for items
        [SerializeField] float itemHeight = 55f; //Block item height
        
        [Header("Offsets")]
        [SerializeField] protected float startOffsetPercentage = 0.22f; //Default offset position
        [SerializeField] protected float endOffsetPercentage = 1.22f;
        protected Coroutine dwellCoroutine; 
        protected float previousScrollPosition;

        public virtual void Start()
        {
            gameManager = GameManager.instance;
            itemCount = gameManager.NumberOfItems;
            itemHeight = 55f;
        }

         protected IEnumerator DwellSelection()
        {
            float initialPosition = scrollableList.content.anchoredPosition.y;
            //Very first selection get distance from selected object
    
            previousScrollPosition = initialPosition;
            float startTime = Time.time;
            //Debug.Log("Selection Starting");

            while (Time.time - startTime < dwellTime)
            {
                if(Time.time-startTime < .066){
                    selectionBar.value = 0; //Don't fill the bar for the first 66ms for smoother looking fill
                }else{
                    selectionBar.value = Time.time-startTime; //Fill the selection bar
                }
        
                float currentPosition = scrollableList.content.anchoredPosition.y; //Current list position
                // Debug.Log("Checking threshold");

                if (Mathf.Abs(currentPosition - initialPosition) > dwellThreshold) //If too much movement reset position
                {
                    //Debug.Log("Selection Cancelled");
                    startTime = Time.time; // Reset the dwell timer
                    initialPosition = currentPosition; // Update the initial position
                }

                yield return null; // Wait for the next frame
            }

            //Debug.Log("Selection Made");
            // Dwell time completed, select the item
            SelectItem();
        }


        protected void SelectItem()
        {
            GameObject selectTextObject = GameObject.FindWithTag("ItemSelect"); //Get item to show selection
            float viewportHeight = scrollableList.viewport.rect.height;

            // Calculate the total height of the list content
            float contentHeight = itemHeight * itemCount;
            float relativeScrollPosition = scrollableList.content.anchoredPosition.y / (contentHeight - viewportHeight); //CUrrent scroll psoition of current list 
            // Calculate the relative scroll position within the content
            float halfwayHeight = (contentHeight-viewportHeight)/2f; //Halfway point to see if we meed to add an adjustment factor
        
            int targetValue = 1; //Target for distance calculations
            float distanceFromTarget = Mathf.Abs(relativeScrollPosition * itemCount - targetValue); //Distance for adjustment calculation

            // Factor to increase the adjustment based on distance from target
            float adjustmentFactor = distanceFromTarget / (itemCount*itemCountMultiplier); //Subtraction for items greater than 25 to align the selections
            //Debug.Log("ADJ" + adjustmentFactor); //For testing
            //Debug.Log(relativeScrollPosition * itemCount + 1);
            if (halfwayHeight < relativeScrollPosition)
            {
            
                selectedItem = Mathf.Clamp(Mathf.RoundToInt(relativeScrollPosition * itemCount + 1), 1, itemCount); //Get item selected
            }
            else
            {
                selectedItem = Mathf.Clamp(Mathf.RoundToInt(relativeScrollPosition * itemCount + 1 - adjustmentFactor), 1, itemCount); //Get item selected minus a factor
            }
            gameManager.SelectedItem = selectedItem;
            // Calculate the selected item index based on the scroll position and item height
            // Check if the GameObject was found
            if (selectTextObject)
            {
                // Get the TextMeshProUGUI component from the GameObject
                selectText = selectTextObject.GetComponent<TextMeshPro>();

                // Check if the component was found
                if (selectText)
                {
                    // Set text to the item selected
                    selectText.text = "Item Selected: " + selectedItem;
                }
            }
        }
    }
}