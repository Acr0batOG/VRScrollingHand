using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointScrollArmUIController : ArmUIController
{
    protected float userPointHeight; // Variable to hold user's height

    // Constants for offset percentages and divisors
    protected float startOffsetPercentage = 0.22f; //Default offset position
    protected float startOffsetChange = 0.04f; //Default offset position
    protected float endOffsetPercentage = 1.22f; // End of arm, used for 11 inch forearms. Will be replaced in GameManager
    protected float armDivisor = 1.85f; // Used to convert user's arm length to the ending point on their arm
    protected float handDivisor = 2.0f; // Used to convert user's hand length from their arm length to the ending point on their hand
    protected float fingerDivisor = 2.30f; // Used to convert user's finger length
    protected float fingertipDivisor = 2.6f; // Used to convert user's fingertip length
    protected float handDivisorAdjustment = .08f;
    protected float armDivisorAdjustment =.05f;
    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
        LengthCheck(); // Check arm length
    }

    private void OnTriggerEnter(Collider other)
    {
        LengthCheck(); // Check arm length
        menuText.text = "Enter"; // Update menu text
        Scroll(other); // Scroll through the content
        if (dwellCoroutine == null)
        {
            dwellCoroutine = StartCoroutine(DwellSelection()); //Used for selection (in superclass)
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Scroll(other); // Scroll through the content
        // Restart dwell selection coroutine if list position changes significantly
        if (dwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)
        {
            StopCoroutine(dwellCoroutine); //If too much movement, reset dwell Selection
            dwellCoroutine = StartCoroutine(DwellSelection());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        menuText.text = "Exit"; // Update menu text
        // Stop dwell selection coroutine on exit
        if (dwellCoroutine != null)
        {
            StopCoroutine(dwellCoroutine); //Reset dwell selection on exit
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
        float length = (endPoint.position - startPoint.position).magnitude;
        float startOffset = startOffsetPercentage * length;
        float endOffset = endOffsetPercentage * length;

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

        // Update distance text
        distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString() + " " + endOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
    }

    // Check arm length and adjust offsets accordingly
    void LengthCheck()
    {
        userPointHeight = gameManager.UserHeight; // Get height from GameManager
        int areaNum = gameManager.AreaNumber; // Check the area number

        switch(areaNum){
            case 1: 
                endOffsetPercentage = userPointHeight / armDivisor + armDivisorAdjustment; //Arm being used for scrolling, different size
                startOffsetPercentage += startOffsetChange; //.26
                break;
            case 2:
                endOffsetPercentage = userHeight / handDivisor - handDivisorAdjustment; //Different divisor to set hand size for users
                //.22
                break;
            case 3:
                endOffsetPercentage = userHeight / fingerDivisor;  //Test this
                startOffsetPercentage += startOffsetChange*2; //.32
                break;
            case 4:
                endOffsetPercentage = userHeight /fingertipDivisor ; //appx.85 with 2.2 arm length
                startOffsetPercentage -= startOffsetChange;
                break;
        }
    }
}
