using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointDynamicScrollArmUIController : PointScrollArmUIController //Inherit from PointScrollAnyways
{
    [SerializeField] private float scrollSpeed = 2f; // Speed multiplier for scrolling
    private const int TriggerTimeMax = 8;
    private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
    private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
    private int triggerTimer = 0;
    private float multiplier = 1550;
    private Coroutine pauseCoroutine; // Coroutine for the pause

    protected new void Start()
    {
        base.Start();
        LengthCheck(); // Check arm length
        AdjustSpeed();
    }

    private void OnTriggerEnter(Collider other)
    {
        LengthCheck(); // Check arm length
        menuText.text = "Enter"; // Update menu text
        lastContactPoint = other.ClosestPoint(startPoint.position);
        if (triggerTimer < TriggerTimeMax) 
        {
            Scroll(other);
        }
        else 
        {
            // After collision, give approx 500ms or 26 frames to make selection then switch to dynamic scroll
            DynamicScroll(other);
        }

        // Cancel the pause coroutine if a new collision starts
        if (pauseCoroutine != null)
        {
            StopCoroutine(pauseCoroutine);
            pauseCoroutine = null;
        }
        if (dwellCoroutine == null)
        {
            dwellCoroutine = StartCoroutine(DwellSelection());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (triggerTimer < TriggerTimeMax) 
        {
            Scroll(other);
        }
        else
        {
            // After collision, give approx 500ms or 26 frames to make selection then switch to dynamic scroll
            DynamicScroll(other);
        }
        
        if (dwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)
        {
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = StartCoroutine(DwellSelection());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        menuText.text = "Exit"; // Update menu text

        // Start the pause coroutine
        if (pauseCoroutine != null)
        {
            StopCoroutine(pauseCoroutine);
        }
        pauseCoroutine = StartCoroutine(PauseBeforeResetCoroutine());
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

        triggerTimer++; // 800 ms given to select point or 42 frames

        // Update distance text
        distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString() + " " + endOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
    }

    protected void DynamicScroll(Collider collisionInfo)
    {
        //float currentTime = Time.time;

        // 26 Frames in a similar or stopped location and greater than 1 second since last pause
        // if (stoppedDetector > 48)
        // {
        //     if (currentTime - lastPauseTime > 3.6f)
        //     {
        //         StartCoroutine(PauseForSelectionCoroutine());
        //         lastPauseTime = currentTime;
        //     }
        //     stoppedDetector = 0;
        //     return;
        // }

        // Determine the current contact point
        Vector3 currentContactPoint = collisionInfo.ClosestPoint(startPoint.position);

        // If the last contact point is not initialized, skip the first scroll to avoid jump
        if ((lastContactPoint == Vector3.zero) || Vector3.Distance(lastContactPoint, currentContactPoint) < (slowMovementThreshold * .36f)) // If no movement or very small movement
        {
            //stoppedDetector++;
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
        distText.text = $"Dynamic Scroll Point: Position {currentContactPoint} Scroll Position {newScrollPosition.y}";

        // Update the last contact point
        lastContactPoint = currentContactPoint;
    }

    // private IEnumerator PauseForSelectionCoroutine()
    // {
    //     isPaused = true; // Set the pause flag to true
    //     yield return new WaitForSeconds(1); // Don't allow scrolling for 1 second
    //     isPaused = false; // Reset the pause flag to false
    // }

    private IEnumerator PauseBeforeResetCoroutine()
    {
        //Debug.Log("Timer Started");
        yield return new WaitForSeconds(1.8f); // Pause for 1.8 seconds before resetting
        //Debug.Log("Timer Finished: Reseting Scrolling");
        triggerTimer = 0; // Reset trigger timer after 1.8 seconds
    }

    // Check arm length and adjust offsets accordingly
    void LengthCheck()
    {
        userPointHeight = gameManager.UserHeight; // Get arm length from GameManager
        int areaNum = gameManager.AreaNumber; // Check the area number

        // Adjust offsets based on hand or arm being used for scrolling. 
        switch (areaNum)
        {
            case 1:
                endOffsetPercentage = userPointHeight / armDivisor + armDivisorAdjustment; // Arm being used for scrolling, different size
                break;
            case 2:
                endOffsetPercentage = userHeight / handDivisor - handDivisorAdjustment; // Different divisor to set hand size for users
                break;
            case 3:
                endOffsetPercentage = userHeight / fingerDivisor - armDivisorAdjustment;  // Needs to be changed
                break;
            case 4:
                endOffsetPercentage = userHeight / fingertipDivisor - armDivisorAdjustment;
                break;
        }
    }

    void AdjustSpeed()
    {
        switch (areaNum)
        {
            case 3:
                scrollSpeed *= 2.1f; // Increase scroll speed for finger
                slowMovementThreshold /= 2; // Decrease slow threshold
                break;
            case 4:
                scrollSpeed *= 3.0f;
                slowMovementThreshold /= 4;
                break;
        }
    }
}
