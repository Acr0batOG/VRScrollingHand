using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PointScrollArmUIController : ArmUIController{
    // Define the new smaller range on the arm
    private float startOffsetPercentage = 0.30f;  // 30%
    private float endOffsetPercentage = 0.83f;   // 83%
    protected new void Start()
    {
        base.Start();
    }
    protected  void OnTriggerEnter(Collider other)
    {
        menuText.text = "Enter";
        Scroll(other);
    }

    protected void OnTriggerStay(Collider other)
    {
        Scroll(other);
    }
    protected  void OnTriggerExit(Collider other){
        menuText.text = "Exit";
    }

    protected override void Scroll(Collider collisionInfo){
        Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);

        // Get the content height and the viewport height
        float contentHeight = scrollableList.content.sizeDelta.y;
        float viewportHeight = scrollableList.viewport.rect.height;

        // Determine the total number of bins (1-20)
        int totalBins = 20;

        // Calculate the new range positions
        float armLength = (endPoint.position - startPoint.position).magnitude;
        float startOffset = startOffsetPercentage * armLength;
        float endOffset = endOffsetPercentage * armLength;

        // Calculate the position along the arm in terms of the new range
        float contactPosition = (contactPoint - startPoint.position).magnitude;
        float adjustedContactPosition = Mathf.Clamp(contactPosition - startOffset, 0, endOffset - startOffset);

        // Calculate the bin index within the new range with reversed mapping
        int binIndex = Mathf.Clamp(Mathf.RoundToInt((1 - (adjustedContactPosition / (endOffset - startOffset))) * (totalBins - 1)), 0, totalBins - 1) + 1;

        // Determine the new scroll position based on the bin index
        float binHeight = (contentHeight - viewportHeight) / (totalBins - 1);
        float newScrollPositionY = (binIndex - 1) * binHeight;

        // Set the new anchored position for the scroll content
        Vector2 newScrollPosition = new Vector2(scrollableList.content.anchoredPosition.x, newScrollPositionY);
        scrollableList.content.anchoredPosition = newScrollPosition;

        // Update the distance text
        distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString();
    }
}
