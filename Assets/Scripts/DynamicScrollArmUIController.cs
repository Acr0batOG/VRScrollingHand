using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DynamicScrollArmUIController : ArmUIController
{
    [SerializeField] private float scrollSpeed = 2f; // Speed multiplier for scrolling
    private float multiplier = 1550;
    private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
    private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
    int stoppedDetector = 0;
    private bool isPaused = false; // Flag to track if scrolling is paused
    private float lastPauseTime = 0f; // Timestamp of the last pause

    protected new void Start()
    {
        base.Start();
    }

    protected void OnTriggerEnter(Collider other)
    {
        menuText.text = "Enter";
        // Initialize last contact point but don't scroll yet
        lastContactPoint = other.ClosestPoint(startPoint.position);
        Scroll(other);
    }

    protected void OnTriggerStay(Collider other)
    {   
        if (!isPaused) // Only call Scroll if not paused
        {
            Scroll(other);
        }
        
    }

    protected void OnTriggerExit(Collider other)
    {
        stoppedDetector = 0; //Reset on exit
        menuText.text = "Exit";
    }

    protected override void Scroll(Collider collisionInfo)
    {
        float currentTime = Time.time;
        
         //42 Frames in a similar or stopped location and greater than 1 second since last pause
        
        if (stoppedDetector > 42)
        {
            if (currentTime - lastPauseTime > 1.0f)
            {
                StartCoroutine(PauseForSelectionCoroutine());
                lastPauseTime = currentTime;
            }
            stoppedDetector = 0;
            return;
        }

        // Determine the current contact point
        Vector3 currentContactPoint = collisionInfo.ClosestPoint(startPoint.position);

        // If the last contact point is not initialized, skip the first scroll to avoid jump
        if ((lastContactPoint == Vector3.zero)||Vector3.Distance(lastContactPoint, currentContactPoint) < (slowMovementThreshold*.36f)) //If no movement or very small movement
        {
            stoppedDetector++;
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
        distText.text = $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} ";

        // Update the last contact point
        lastContactPoint = currentContactPoint;
    }

    private IEnumerator PauseForSelectionCoroutine()
    {
        isPaused = true; // Set the pause flag to true
        yield return new WaitForSeconds(1); // Don't allow scrolling for 1 second
        isPaused = false; // Reset the pause flag to false
    }
}
