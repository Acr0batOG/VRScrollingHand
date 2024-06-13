using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StaticScrollArmUIController : ArmUIController{
    [SerializeField] private float staticScrollSpeed = 75f; //Speed multiplier for static scrolling 
    protected float handSpeed = 1.06f;
    protected float fingerSpeed = 5.0f;
    protected float fingertipSpeed = 10.0f;
    protected float armThreshold = 150f;
    protected float handThreshold = 125f;
    protected float fingerThreshold = 75f;
    protected float fingertipThreshold = 40f;
    
    protected new void Start()
    {
        base.Start();
        AdjustSpeed();
        
    }
    protected void OnTriggerEnter(Collider other)
    {
        menuText.text = "Enter";
        Scroll(other);
         // Start dwell selection coroutine
        if (dwellCoroutine == null)
        {
            dwellCoroutine = StartCoroutine(DwellSelection()); //Start dwell selection
        }
    }

    protected void OnTriggerStay(Collider other)
    {
        Scroll(other);
        // Restart dwell selection coroutine if list position changes significantly
        if (dwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = StartCoroutine(DwellSelection()); //Reset selection if too much movement
        }
    }
    protected void OnTriggerExit(Collider other){
        menuText.text = "Exit";
        // Stop dwell selection coroutine on exit
        if (dwellCoroutine != null)
        {
            StopCoroutine(dwellCoroutine); //Stop dwell on exit
            dwellCoroutine = null;
        }
    }

    protected override void Scroll(Collider fingerCollider){
        Vector3 contactPoint = fingerCollider.ClosestPoint(startPoint.position);

        // Calculate the middle point between startPoint and endPoint
        Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;

        float threshold = GetThreshold(); //Determine threshold size base on collision object
        

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
        CheckThreshold(contactPoint, middlePoint, threshold);
        if(areaNum==2||areaNum==1){
            if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
                return; //Middle dead zone for no scrolling
        }else if(areaNum==3||areaNum==4){
            if (Math.Abs(contactPoint.x - middlePoint.x) <= threshold)
                return; //Middle dead zone for no scrolling
        }
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
    float GetThreshold(){
        switch(areaNum){//Update speed for area postion for scroll
                case 1: 
                    return capsuleCollider.height/armThreshold; //Arm Threshold - 165f
                case 2:
                    return capsuleCollider.height/handThreshold; //Hand Threshold - 150f
                case 3:
                    return capsuleCollider.height/fingerThreshold; //Finger Threshold - 100f
                case 4:
                    return capsuleCollider.height/fingertipThreshold; //Fingertip Threshold - 50f
                default:
                    return capsuleCollider.height/165f;
            }
    }
    void CheckThreshold(Vector3 contactPoint, Vector3 middlePoint, float threshold){
        
        if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
            return; //Middle dead zone for no scrolling
    }
    void AdjustSpeed(){
        switch(areaNum){//Update speed for area postion for scroll
            case 1: 
                break;
            case 2:
                staticScrollSpeed*=handSpeed; //*1.06
                break;
            case 3:
                staticScrollSpeed*=fingerSpeed; //*5
                break;
            case 4:
                staticScrollSpeed*=fingertipSpeed;//*10
                break;
        }
    }
}
