using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class DynamicScrollArmUIController : ArmUIController
{
    [SerializeField] private float scrollSpeed = 2f; // Speed multiplier for scrolling
    private float multiplier = 1550;
    private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
    private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
    private bool isPaused = false; // Flag to track if scrolling is paused
    

    protected new void Start()
    {
        base.Start();
        AdjustSpeed();
        
    }

    protected void OnTriggerEnter(Collider other)
    {
        menuText.text = "Enter";
        // Initialize last contact point but don't scroll yet
        lastContactPoint = other.ClosestPoint(startPoint.position);
        
        Scroll(other);
         // Start dwell selection coroutine
        if (dwellCoroutine == null)
        {
            dwellCoroutine = StartCoroutine(DwellSelection());
        }
    }

    protected void OnTriggerStay(Collider other)
    {   
        if (!isPaused) // Only call Scroll if not paused
        {
            Scroll(other);
        }
        // Restart dwell selection coroutine if list position changes significantly
        if (dwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = StartCoroutine(DwellSelection());
        }
        
    }

    protected void OnTriggerExit(Collider other)
    {

        menuText.text = "Exit";
        // Stop dwell selection coroutine on exit
        if (dwellCoroutine != null)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = null;
        }
    }

    protected override void Scroll(Collider collisionInfo)
    {

        // Determine the current contact point
        Vector3 currentContactPoint = collisionInfo.ClosestPoint(startPoint.position);

        // If the last contact point is not initialized, skip the first scroll to avoid jump
        if ((lastContactPoint == Vector3.zero)||Vector3.Distance(lastContactPoint, currentContactPoint) < (slowMovementThreshold*.36f)) //If no movement or very small movement
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
        
        float deltaPosition = 0;
        // Calculate the difference in contact point position
        switch(areaNum){
            case 1:
                deltaPosition = currentContactPoint.z - lastContactPoint.z;
                break;
            case 2:
                deltaPosition = currentContactPoint.z - lastContactPoint.z;
                break;
            case 3:
                deltaPosition = currentContactPoint.x - lastContactPoint.x;
                break;
            case 4:
                deltaPosition = currentContactPoint.x - lastContactPoint.x;
                break;
        }
        

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
    void AdjustSpeed(){
        switch(areaNum){
            case 3:
                scrollSpeed *= 2.1f; //Increase scroll speed for finger
                slowMovementThreshold = .0005f; //Decrease slow threshold
                break;
            case 4:
                scrollSpeed *= 3.0f;
                slowMovementThreshold = .00025f;
                break;
        }
    }

    
}
