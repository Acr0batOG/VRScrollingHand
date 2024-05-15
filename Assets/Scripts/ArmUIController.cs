using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ArmUIController : MonoBehaviour
{
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private ScrollRect scrollableList;

    private float listLength;
    private Vector3 baseScale;
    [SerializeField] private float scaleFactor = 2f;
    private int itemsInList = 10;
    private Vector3 initialScrollPosition;
    [SerializeField] private float scrollSpeed = 25f;

    public TextMeshPro distText;

    public TextMeshProUGUI selectText;
    void Start()
    {
        listLength = (startPoint.position - endPoint.position).magnitude;
        initialScrollPosition = scrollableList.content.anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        selectText.text = "Enter";
        baseScale = other.transform.localScale;
        ScrollList(other);
    }
    

    private void OnTriggerStay(Collider other)
    {   
        ScrollList(other);
    }

    private void OnTriggerExit(Collider other)
    {   
        selectText.text = "Exit";
        baseScale = other.transform.localScale;
    
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

    private void ScrollList(Collider collisionInfo)
{
    // Determine the contact point
    Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);

    // Calculate the middle point between startPoint and endPoint
    Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;

    // Calculate the distance from the contact point to the start and end points
    float distanceFromStart = (contactPoint - startPoint.position).magnitude;
    float distanceFromEnd = (contactPoint - endPoint.position).magnitude;

    // Determine the polarity based on which end the contact point is closer to
    int polarity = distanceFromStart < distanceFromEnd ? -1 : 1;

    // Get the content height and the viewport height
    float contentHeight = scrollableList.content.sizeDelta.y;
    float viewportHeight = scrollableList.viewport.rect.height;

    // Calculate the new scroll position based on the distance from the middle point
    float deltaY = (contactPoint - middlePoint).magnitude * polarity * scrollSpeed;

    // Update the new scroll position
    Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
    newScrollPosition.y += deltaY;

    // Clamp the new scroll position within the scrollable area
    newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

    // Set the new anchored position for the scroll content
    scrollableList.content.anchoredPosition = newScrollPosition;

    // Update the distance text
    distText.text = "Position: " + contactPoint.ToString() + " " + newScrollPosition.y.ToString();
}
}