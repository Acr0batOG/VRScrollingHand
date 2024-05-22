using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DynamicScrollArmUIController : ArmUIController
{
    [SerializeField] private float scrollSpeed = 50f; // Speed multiplier for scrolling
    private float multiplier = 1650;
    private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
    private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold

    protected void Start()
    {
        base.Start();
    }

    protected void OnTriggerEnter(Collider other)
    {
        menuText.text = "Enter";
        // Initialize last contact point but don't scroll yet
        lastContactPoint = other.ClosestPoint(startPoint.position);
    }

    protected void OnTriggerStay(Collider other)
    {
        Scroll(other);
    }

    protected void OnTriggerExit(Collider other)
    {
        menuText.text = "Exit";
    }

    protected override void Scroll(Collider collisionInfo)
    {
        // Determine the current contact point
        Vector3 currentContactPoint = collisionInfo.ClosestPoint(startPoint.position);

        // If the last contact point is not initialized, skip the first scroll to avoid jump
        if (lastContactPoint == Vector3.zero)
        {
            lastContactPoint = currentContactPoint;
            return;
        }

        // Check if the movement is below the threshold to avoid small jitters
        if (Vector3.Distance(lastContactPoint, currentContactPoint) < slowMovementThreshold)
        {
            lastContactPoint = currentContactPoint;
            return;
        }

        // Calculate the difference in contact point position
        float deltaPosition = currentContactPoint.z - lastContactPoint.z;

        // Get the content height and the viewport height
        float contentHeight = scrollableList.content.sizeDelta.y;
        float viewportHeight = scrollableList.viewport.rect.height;

        // Calculate the new scroll position based on the difference in contact point position
        float deltaY = deltaPosition * scrollSpeed * multiplier;

        // Update the new scroll position
        Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
        newScrollPosition.y += deltaY; // Addition because moving the hand up should scroll down

        // Clamp the new scroll position within the scrollable area
        newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

        // Set the new anchored position for the scroll content
        scrollableList.content.anchoredPosition = newScrollPosition;

        // Update the distance text
        distText.text = $"Dynamic Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Distance for speed: {Vector3.Distance(lastContactPoint, currentContactPoint)}";

        // Update the last contact point
        lastContactPoint = currentContactPoint;
    }
}
