using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;

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
    protected TextMeshPro dataText;
    protected float userHeight; 
    protected int areaNum;
    protected float armModify = 4.75f; //Works
    protected float handModify = 4.5f; //Works
    protected float fingerModify = 5.0f; //Works
    protected float fingertipModify = 7.45f; //Works, except tracking sucks
    protected Coroutine dwellCoroutine; // Coroutine for the dwell selection
    protected float previousScrollPosition; // Previous scroll position for dwell check
    protected float dwellThreshold = 10f; // Threshold for movement to cancel dwell
    protected float dwellTime = 2.5f; // Time required to dwell on an item
    protected int itemCount = 0;
    protected void Start()
    {
        gameManager = GameManager.instance;
        areaNum = gameManager.AreaNumber; // Get area being used
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
        if(itemCount!=gameManager.NumberOfItems){
            itemCount = gameManager.NumberOfItems; //Replace itemCount if NumberOfItems ever changes
        }
        AreaCheck(previousArea, areaNum); //Check if area has changed
        switch (areaNum) // Switch to set the start and end points
        {
            case 1: // Arm scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, armModify);
                break;
            case 2: // Hand scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, handModify);
                break;
            case 3: // Finger scrolling
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
        // Add scrolling logic here if needed
    }
    protected IEnumerator DwellSelection()
{
    float initialPosition = scrollableList.content.anchoredPosition.y;
    previousScrollPosition = initialPosition;
    float startTime = Time.time;
    Debug.Log("Selection Starting");

    while (Time.time - startTime < dwellTime)
    {
        if(Time.time-startTime < .066){
            selectionBar.value = 0;
        }else{
            selectionBar.value = Time.time-startTime; //Fill the selection bar
        }
        
        float currentPosition = scrollableList.content.anchoredPosition.y;
        Debug.Log("Checking threshold");

        if (Mathf.Abs(currentPosition - initialPosition) > dwellThreshold)
        {
            Debug.Log("Selection Cancelled");
            startTime = Time.time; // Reset the dwell timer
            initialPosition = currentPosition; // Update the initial position
        }

        yield return null; // Wait for the next frame
    }

    Debug.Log("Selection Made");
    // Dwell time completed, select the item
    SelectItem();
    }


    protected void SelectItem()
    {
       GameObject dataTextObject = GameObject.FindWithTag("DataText");
        float viewportHeight = scrollableList.viewport.rect.height;
        // Calculate the height of each item in the list
        float itemHeight = 55f;

        // Calculate the total height of the list content
        float contentHeight = itemHeight * itemCount;
        float relativeScrollPosition = scrollableList.content.anchoredPosition.y / (contentHeight - viewportHeight);
        int x = Mathf.Clamp(Mathf.FloorToInt(relativeScrollPosition * itemCount), 1, itemCount) + 1;
        // Calculate the relative scroll position within the content
        float halfwayHeight = (contentHeight-viewportHeight)/2f;
        int selectedItem;
        int targetValue = 1;
        float distanceFromTarget = Mathf.Abs(relativeScrollPosition * itemCount - targetValue);

        // Factor to increase the adjustment based on distance from target
        float adjustmentFactor = distanceFromTarget / itemCount*1.3f;
        Debug.Log("ADJ" + adjustmentFactor);
        Debug.Log(relativeScrollPosition * itemCount + 1);
        if (halfwayHeight < relativeScrollPosition)
        {
            selectedItem = Mathf.Clamp(Mathf.RoundToInt(relativeScrollPosition * itemCount + 1), 1, itemCount);
        }
        else
        {
            selectedItem = Mathf.Clamp(Mathf.RoundToInt(relativeScrollPosition * itemCount + 1 - adjustmentFactor), 1, itemCount);
        }
        // Calculate the selected item index based on the scroll position and item height
        // Check if the GameObject was found
        if (dataTextObject != null)
        {
            // Get the TextMeshProUGUI component from the GameObject
            dataText = dataTextObject.GetComponent<TextMeshPro>();

            // Check if the component was found
            if (dataText != null)
            {
                // Implement item selection logic here
                dataText.text = "Item Selected: " + selectedItem;
            }
        }
    }

}
