using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class PointScrollArmUIController : ArmUIController
    {
        protected float UserPointHeight; // Variable to hold user's height

        // Constants for offset percentages and divisors
        protected float StartOffsetPercentage = 0.22f; //Default offset position
        protected float EndOffsetPercentage = 1.22f; // End of arm, used for 11 inch forearms. Will be replaced in GameManager
        private List<float> recentScrollPositions = new List<float>();
        private int movingAverageWindowSize = 10; // Adjust the window size as needed
       
        protected new void Start()
        {
            base.Start();
            LengthCheck(); // Check arm length  
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                LengthCheck(); // Check arm length
                menuText.text = "Enter"; // Update menu text
                Scroll(other); // Scroll through the content
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                Scroll(other); // Scroll through the content
                
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Exit"; // Update menu text
                
            }
        }

        protected override void Scroll(Collider fingerCollider)
        {
            Vector3 contactPoint = fingerCollider.ClosestPoint(startPoint.position); // Get the closest contact point

            // Calculate content and viewport height
            float contentHeight = scrollableList.content.sizeDelta.y;
            float viewportHeight = scrollableList.viewport.rect.height;

            int totalBins = GameManager.NumberOfItems; // Total number of bins for scrolling

            // Calculate arm length and offsets
            float length = (endPoint.position - startPoint.position).magnitude;
            float startOffset = StartOffsetPercentage * length;
            float endOffset = EndOffsetPercentage * length;

            // Calculate contact and adjusted contact positions
            float contactPosition = (contactPoint - startPoint.position).magnitude;
            float adjustedContactPosition = Mathf.Clamp(contactPosition - startOffset, 0, endOffset - startOffset);

            // Calculate bin index based on adjusted contact position
            int binIndex = Mathf.Clamp(Mathf.RoundToInt((1 - (adjustedContactPosition / (endOffset - startOffset))) * (totalBins - 1)), 0, totalBins - 1) + 1;

            // Calculate bin height and new scroll position
            float binHeight = (contentHeight - viewportHeight) / (totalBins - 1);
            float newScrollPositionY = (binIndex - 1) * binHeight;

            // Add the new scroll position to the list and keep the list within the window size
            recentScrollPositions.Add(newScrollPositionY);
            if (recentScrollPositions.Count > movingAverageWindowSize)
            {
                recentScrollPositions.RemoveAt(0);
            }

            // Calculate the smoothed scroll position using the moving average
            float smoothedScrollPositionY = recentScrollPositions.Average();

            // Set the new scroll position
            Vector2 newScrollPosition = new Vector2(scrollableList.content.anchoredPosition.x, smoothedScrollPositionY);
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update distance text
            distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + smoothedScrollPositionY.ToString(CultureInfo.InvariantCulture) + " " + EndOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
        }

        // Check arm length and adjust offsets accordingly
        protected void LengthCheck()
        {
            UserPointHeight = GameManager.UserHeight; // Get height from GameManager
            int lengthCheckAreaNum = GameManager.AreaNumber; // Check the area number

            switch(lengthCheckAreaNum){
                case 1: 
                    EndOffsetPercentage = .72f; //Arm being used for scrolling, different size
                    StartOffsetPercentage = 0.25f;
                    break;
                case 2:
                    EndOffsetPercentage = .68f; //Different divisor to set hand size for users
                    StartOffsetPercentage = 0.27f;
                    
                    break;
            }
        }
    }
}
