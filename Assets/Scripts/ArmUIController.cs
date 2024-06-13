using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Oculus.Interaction.Input;
using static OVRProjectConfig;
public class ArmUIController : MonoBehaviour
{
    [SerializeField] protected Transform startPoint;
    [SerializeField] protected Transform endPoint;
    [SerializeField] protected ScrollRect scrollableList;
    [SerializeField] protected CapsuleCollider capsuleCollider;
    [SerializeField] protected TextMeshPro distText;
    [SerializeField] protected TextMeshProUGUI menuText;
    
    protected GameManager gameManager;
    protected Slider selectionBar;
    protected TextMeshPro selectText;
    protected float userHeight; 
    protected int areaNum;
    protected int selectedItem;
    protected Coroutine dwellCoroutine; // Coroutine for the dwell selection
    protected float armModify = 4.75f; //Works
    protected float handModify = 4.75f; //Works
    protected float fingerModify = 5.0f; //Works
    protected float fingertipModify = 7.5f; //Works, except tracking sucks
    protected float itemCountMultiplier = 1.3f; //Multiplier for items
    protected float itemHeight = 55f; //Block item height
    protected float previousScrollPosition; // Previous scroll position for dwell check
    protected float dwellThreshold = 10f; // Threshold for movement to cancel dwell
    protected float dwellTime = 2.2f; // Time required to dwell on an item
    protected int itemCount = 0;

    protected void Start()
    {
        gameManager = GameManager.instance;
        areaNum = gameManager.AreaNumber; // Get area being used
        selectedItem = gameManager.SelectedItem;
        selectionBar = GameObject.FindWithTag("SelectionBar").GetComponent<Slider>();
        selectionBar.value = 0f;
        itemCount = gameManager.NumberOfItems;
        switch (areaNum) // Switch to set the start and end points
        {
            case 1:
                startPoint = GameObject.FindWithTag("Elbow").transform; // Arm scrolling
                endPoint = GameObject.FindWithTag("Wrist").transform;
                break;
            case 2:
                startPoint = GameObject.FindWithTag("Wrist").transform; //Hand scrolling
                endPoint = GameObject.FindWithTag("LFingertip").transform;
                break;
            case 3:
                startPoint = GameObject.FindWithTag("Fingerbase").transform; //Finger scrolling
                endPoint = GameObject.FindWithTag("RFingertip").transform;
                break;
            case 4:
                startPoint = GameObject.FindWithTag("Fingermid").transform; //Fingertip scrolling
                endPoint = GameObject.FindWithTag("RFingertip").transform;
                break;
        }
    }
    
    void Update()
    {
        // Calculate the middle point between startPoint and endPoint
        Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        userHeight = gameManager.UserHeight; // Get height of character
        int previousArea = areaNum;
        areaNum = gameManager.AreaNumber;
        selectedItem = gameManager.SelectedItem;
        itemCount = gameManager.NumberOfItems; //Replace itemCount if NumberOfItems ever changes
        
        AreaCheck(previousArea, areaNum); //Check if area has changed
       
        switch (areaNum) // Switch to set the start and end points
        {
            case 1: // Arm scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, armModify);
                break;
            case 2: // Hand scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, handModify);
                break;
            case 3: // Finger scrolling. Shift the capsule slightly
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, fingerModify);
                break;
            case 4: // Fingertip scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, fingertipModify);
                break;
        }
    }

    void OrientCollider(CapsuleCollider collider, Vector3 middlePoint, Vector3 direction, float distance, float userHeight, float multiplier)
    {
        collider.center = Vector3.zero; // Reset center to origin
        collider.height = distance * userHeight * multiplier; // Set height based on distance and user height
        collider.transform.position = middlePoint; // Position the collider at the midpoint
        // Set the direction of the collider to Y-axis
        collider.direction = 1; // 0 for X, 1 for Y, 2 for Z

        // Calculate the rotation to align with the direction vector
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

        // Apply the rotation to the collider
        collider.transform.rotation = rotation;
    }
    void AreaCheck(int previousArea, int areaNum){
        if(areaNum!=previousArea){
            switch (areaNum) // Switch to set the start and end points
        {
            case 1:
                startPoint = GameObject.FindWithTag("Elbow").transform; // Arm scrolling
                endPoint = GameObject.FindWithTag("Wrist").transform;
                break;
            case 2:
                startPoint = GameObject.FindWithTag("Wrist").transform; //Hand scrolling
                endPoint = GameObject.FindWithTag("LFingertip").transform;
                break;
            case 3:
                startPoint = GameObject.FindWithTag("Fingerbase").transform; //Finger scrolling
                endPoint = GameObject.FindWithTag("RFingertip").transform;
                break;
            case 4:
                startPoint = GameObject.FindWithTag("Fingermid").transform; //Fingertip scrolling
                endPoint = GameObject.FindWithTag("RFingertip").transform;
                break;
        }
        }
    } 

    protected virtual void Scroll(Collider collisionInfo)
    {
        //Just to be inherited
    }
    protected IEnumerator DwellSelection()
{
    float initialPosition = scrollableList.content.anchoredPosition.y;
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
        if (selectTextObject != null)
        {
            // Get the TextMeshProUGUI component from the GameObject
            selectText = selectTextObject.GetComponent<TextMeshPro>();

            // Check if the component was found
            if (selectText != null)
            {
                // Set text to the item selected
                selectText.text = "Item Selected: " + selectedItem;
            }
        }
    }

}
