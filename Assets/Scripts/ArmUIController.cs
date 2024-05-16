using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class ArmUIController : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private ScrollRect scrollableList;

    public AllVariables allVariables;
    private float listLength;
 
    //[SerializeField] private float scaleFactor = 2f;
    // private int itemsInList = 10;
     private Vector3 initialScrollPosition;
    [SerializeField] private float staticScrollSpeed = 50f;
    [SerializeField] private float dynamicScrollSpeed = 5f;
    private Vector3 lastContactPoint = Vector3.zero;
    private float slowMovementThreshold = .1f; // Add the declaration here
    int dynamicDirection = 1; //1 == z for dynamic scrolling 2 == y for scrolling
    public TextMeshPro distText;

    public TextMeshProUGUI selectText;
    void Start()
    {
        listLength = (startPoint.position - endPoint.position).magnitude;
        initialScrollPosition = scrollableList.content.anchoredPosition;
    }

    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        selectText.text = "Enter";
        
        if(allVariables.techniqueNumber==1){
            staticScrollList(other);
        }else if(allVariables.techniqueNumber==2){
            pointScrollList(other);
        }else if(allVariables.techniqueNumber==3){
            dynamicScrollList(other, dynamicDirection);
        }

    }
    

    private void OnTriggerStay(Collider other)
    {   if(allVariables.techniqueNumber==1){ //Technique number one is static scrolling. Place a finger and it will scroll
        staticScrollList(other);
        }else if(allVariables.techniqueNumber==2){ //Technique number two is selecting a point and scrolling from there
            pointScrollList(other);
        }else if(allVariables.techniqueNumber==3){ //Technique number three is dynamic scrolling where the finger moves to scroll
            dynamicScrollList(other, dynamicDirection);
        }
    }

    private void OnTriggerExit(Collider other){   
        selectText.text = "Exit";   
    }

    // private void SizeList(Collider collisionInfo)
    // {
    //     if (!collisionInfo.gameObject.CompareTag("Finger")) return;
        
    //     Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);
    //     Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;
    //     float distanceFromStart = (contactPoint - startPoint.position).magnitude;
    //     float distanceFromEnd = (contactPoint - endPoint.position).magnitude;
    //     int polarity = distanceFromStart < distanceFromEnd ? -1 : 1;
    //     scaleFactor += (contactPoint - middlePoint).magnitude/listLength*polarity;
    //     //scrollableList.localScale = scaleFactor * baseScale;
    //     distText.text = "Position:" + contactPoint.ToString();
    // }

    private void staticScrollList(Collider collisionInfo){
    // Determine the contact point
        Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);

        // Calculate the middle point between startPoint and endPoint
        Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;

        // Calculate the distance from the contact point to the start and end points
        float distanceFromStart = (contactPoint - startPoint.position).magnitude;
        float distanceFromEnd = (contactPoint - endPoint.position).magnitude;

        // Determine the polarity based on which end the contact point is closer to
        int polarity = distanceFromStart > distanceFromEnd ? -1 : 1;

        // Get the content height and the viewport height
        float contentHeight = scrollableList.content.sizeDelta.y;
        float viewportHeight = scrollableList.viewport.rect.height;

        // Calculate the new scroll position based on the distance from the middle point
        float deltaY = (contactPoint - middlePoint).magnitude * polarity * staticScrollSpeed;
        if(contactPoint.magnitude <= middlePoint.magnitude+.0108f&&contactPoint.magnitude >= middlePoint.magnitude-.0108f)
            return; //Middle dead zone for no scrolling
        // Update the new scroll position
        Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
        newScrollPosition.y += deltaY;

        // Clamp the new scroll position within the scrollable area
        newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

        // Set the new anchored position for the scroll content
        scrollableList.content.anchoredPosition = newScrollPosition;

        // Update the distance text
        distText.text = "Static Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString();
    }
    private void pointScrollList(Collider collisionInfo){
        Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);

        // Get the content height and the viewport height
        float contentHeight = scrollableList.content.sizeDelta.y;
        float viewportHeight = scrollableList.viewport.rect.height;

        // Determine the total number of bins (1-20)
        int totalBins = 20;

        // Define the new smaller range on the arm
        float startOffsetPercentage = 0.30f;  // 30%
        float endOffsetPercentage = 0.83f;   // 83%

        // Calculate the new range positions
        float armLength = (endPoint.position - startPoint.position).magnitude;
        float startOffset = startOffsetPercentage * armLength;
        float endOffset = endOffsetPercentage * armLength;

        // Calculate the position along the arm in terms of the new range
        float contactPosition = (contactPoint - startPoint.position).magnitude;
        float adjustedContactPosition = Mathf.Clamp(contactPosition - startOffset, 0, endOffset - startOffset);

        // Calculate the bin index within the new range with reversed mapping
        int binIndex = Mathf.Clamp(Mathf.RoundToInt((1 - (adjustedContactPosition / (endOffset - startOffset))) * (totalBins - 1)), 0, totalBins - 1) + 1;

        // Determine the new scroll position based on the bin index
        float binHeight = (contentHeight - viewportHeight) / (totalBins - 1);
        float newScrollPositionY = (binIndex - 1) * binHeight;

        // Set the new anchored position for the scroll content
        Vector2 newScrollPosition = new Vector2(scrollableList.content.anchoredPosition.x, newScrollPositionY);
        scrollableList.content.anchoredPosition = newScrollPosition;

        // Update the distance text
        distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString();

    
    }
  private void dynamicScrollList(Collider collisionInfo, int direction){
    // Determine the current contact point
    Vector3 currentContactPoint = collisionInfo.ClosestPoint(startPoint.position);

    // Check if this is the first frame or if the contact point is significantly different
    if (lastContactPoint == Vector3.zero || Vector3.Distance(lastContactPoint, currentContactPoint) > slowMovementThreshold)
    {
        lastContactPoint = currentContactPoint;
        return;
    }
    float deltaPosition;
    // Calculate the difference in contact point position
    if(direction == 1){
     deltaPosition = currentContactPoint.z - lastContactPoint.z;
    }else{
     deltaPosition = currentContactPoint.y - lastContactPoint.y;
    }

    // Get the content height and the viewport height
    float contentHeight = scrollableList.content.sizeDelta.y;
    float viewportHeight = scrollableList.viewport.rect.height;

    // Calculate the new scroll position based on the difference in contact point position
    float deltaY = deltaPosition*dynamicScrollSpeed;


    // Update the new scroll position
    Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
    newScrollPosition.y -= deltaY; // Subtracting because moving the hand up should scroll down

    // Clamp the new scroll position within the scrollable area
    newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

    // Set the new anchored position for the scroll content
    scrollableList.content.anchoredPosition = newScrollPosition;

    // Update the distance text
    distText.text = "Dynamic Scroll: Position " + currentContactPoint.ToString() + " " + newScrollPosition.y.ToString() + " DeltaY: " + deltaY.ToString();

    // Update the last contact point
    lastContactPoint = currentContactPoint;
}


}