using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StaticScrollArmUIController : ArmUIController{
    [SerializeField] private float staticScrollSpeed = 75f; //Speed multiplier for static scrolling
    [SerializeField] private float threshold = .0105f; //Used to create a center "dead zone" for on arm scrolling when the user wants select or stop at a certain selection 
    
    protected new void Start()
    {
        base.Start();
        
    }
    protected void OnTriggerEnter(Collider other)
    {
        menuText.text = "Enter";
        Scroll(other);
    }

    protected void OnTriggerStay(Collider other)
    {
        Scroll(other);
    }
    protected void OnTriggerExit(Collider other){
        menuText.text = "Exit";
    }

    protected override void Scroll(Collider collisionInfo){
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
        distText.text = "Static Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString();
    }
}
