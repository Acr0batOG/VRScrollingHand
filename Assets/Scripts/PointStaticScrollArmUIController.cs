using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PointStaticScrollArmUIController : PointScrollArmUIController
{
    [SerializeField] private float staticScrollSpeed = 75f; //Speed multiplier for static scrolling
    private int triggerTimer = 0;
    Vector3 collisionPoint;
    protected new void Start()
    {

        base.Start();
        LengthCheck(); // Check arm length
        SpeedControl();
    }

    private void OnTriggerEnter(Collider other)
    {
        LengthCheck(); // Check arm length
        menuText.text = "Enter"; // Update menu text
        Scroll(other); // Scroll through the content
         // Start dwell selection coroutine
        if (dwellCoroutine == null)
        {
            dwellCoroutine = StartCoroutine(DwellSelection());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (triggerTimer<8){
             Scroll(other);
        }else { //After collision give appx 800ms to make selection then switch to Static scroll
            
            StaticScroll(other, collisionPoint);
        }
        // Restart dwell selection coroutine if list position changes significantly
        if (dwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = StartCoroutine(DwellSelection());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        triggerTimer = 0; //Only reset to other method if collision done
        menuText.text = "Exit"; // Update menu text
        // Stop dwell selection coroutine on exit
        if (dwellCoroutine != null)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = null;
        }
    }

    protected override void Scroll(Collider collisionInfo)
    {
        Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position); // Get the closest contact point

        // Calculate content and viewport height
        float contentHeight = scrollableList.content.sizeDelta.y;
        float viewportHeight = scrollableList.viewport.rect.height;

        int totalBins = gameManager.NumberOfItems; // Total number of bins for scrolling

        // Calculate arm length and offsets
        float armLength = (endPoint.position - startPoint.position).magnitude;
        float startOffset = startOffsetPercentage * armLength;
        float endOffset = endOffsetPercentage * armLength;

        // Calculate contact and adjusted contact positions
        float contactPosition = (contactPoint - startPoint.position).magnitude;
        float adjustedContactPosition = Mathf.Clamp(contactPosition - startOffset, 0, endOffset - startOffset);

        // Calculate bin index based on adjusted contact position
        int binIndex = Mathf.Clamp(Mathf.RoundToInt((1 - (adjustedContactPosition / (endOffset - startOffset))) * (totalBins - 1)), 0, totalBins - 1) + 1;

        // Calculate bin height and new scroll position
        float binHeight = (contentHeight - viewportHeight) / (totalBins - 1);
        float newScrollPositionY = (binIndex - 1) * binHeight;

        // Set the new scroll position
        Vector2 newScrollPosition = new Vector2(scrollableList.content.anchoredPosition.x, newScrollPositionY);
        scrollableList.content.anchoredPosition = newScrollPosition;
        triggerTimer++; //800 ms given to select point or 42 frames, then switch methods. 
        // Update distance text
        collisionPoint = collisionInfo.ClosestPoint(startPoint.position); //Set middle point to location where last point selection was made
        distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString() + " " + endOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
    }
    protected void StaticScroll(Collider collisionInfo, Vector3 collisionPoint){
        Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);

        Vector3 middlePoint = collisionPoint;
        float threshold = capsuleCollider.height/215.5f;

        // Determine the polarity based on which end the contact point is closer to
        int polarity = contactPoint.magnitude > middlePoint.magnitude ? -1 : 1;
      

        // Get the content height and the viewport height
        float contentHeight = scrollableList.content.sizeDelta.y;
        float viewportHeight = scrollableList.viewport.rect.height;

        // Calculate the new scroll position based on the distance from the middle point
        float deltaY = (contactPoint - middlePoint).magnitude * polarity * staticScrollSpeed;
        if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
            return; //Middle dead zone for no scrolling
        // Update the new scroll position
        Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
        newScrollPosition.y += deltaY;

        // Clamp the new scroll position within the scrollable area
        newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

        // Set the new anchored position for the scroll content
        scrollableList.content.anchoredPosition = newScrollPosition;

        // Update the distance text
        distText.text = "Point Static Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString();
    }

    // Check arm length and adjust offsets accordingly
    void LengthCheck()
    {
        userPointHeight = gameManager.UserHeight; // Get arm length from GameManager
        int areaNum = gameManager.AreaNumber; // Check the area number

        switch(areaNum){
            case 1: 
                endOffsetPercentage = userPointHeight / armDivisor + armDivisorAdjustment; //Arm being used for scrolling, different size
                break;
            case 2:
                endOffsetPercentage = userHeight / handDivisor - handDivisorAdjustment; //Different divisor to set hand size for users
                break;
            case 3:
                endOffsetPercentage = userHeight / fingerDivisor - armDivisorAdjustment;  //Needs to be changed
                break;
            case 4:
                endOffsetPercentage = userHeight / fingertipDivisor - armDivisorAdjustment; 
                break;
        }
    }
    void SpeedControl(){
        int areaNum = gameManager.AreaNumber; // Check the area number

        switch(areaNum){
            case 1: 
                 staticScrollSpeed *= 1.1f;
                break;
            case 2:
                staticScrollSpeed *= 1.06f;
                break;
            case 3:
                 staticScrollSpeed *= 1.55f;
                break;
            case 4:
                staticScrollSpeed *= 1.75f;
                break;
        }
        
    }
}
