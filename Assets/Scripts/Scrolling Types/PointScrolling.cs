using Scrolling_Interface;
using UnityEngine;

namespace Scrolling_Types
{
    public class PointScrolling : ScrollBase, IScrollable
    {
    protected void Start()
    {
        
    }
     public void Setup()
     {
    
     }
    private void OnTriggerEnter(Collider other)
    {
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

    public void Scroll(Collider colliderInfo)
    {
        Vector3 contactPoint = colliderInfo.ClosestPoint(startPoint.position); // Get the closest contact point

        // Calculate content and viewport height
        contentHeight = scrollableList.content.sizeDelta.y;
        viewportHeight = scrollableList.viewport.rect.height;

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

    }
}