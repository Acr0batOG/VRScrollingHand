using System.Collections;
using System.Globalization;
using _Scripts.Calculators;
using _Scripts.GameState;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class PointDynamicScrollArmUIController : PointScrollArmUIController //Inherit from PointScrollAnyways
    {
        private float scrollSpeed = 550f;
        private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
        private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
        private bool touchFinished;
        private int scrollCounter;
        private readonly float fingerScrollMultiplier = 1.5f;
        private readonly float fingertipScrollMultiplier = 3.0f;
        private Coroutine pauseCoroutine; // Coroutine for the pause
        private GameManager gameManager;
        float contentHeight;
        float viewportHeight;

        // Inertia-related variables
        private float currentScrollSpeed;
        private float deceleration = 75f; // Rate at which scrolling slows down
        private bool isScrolling;

        protected new void Start()
        {
            
            base.Start();
            gameManager = GameManager.instance;
            LengthCheck(); // Check arm length
            AdjustSpeed(); // Update speed based on point used to scroll
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            contentHeight = scrollableList.content.sizeDelta.y;
            touchFinished = gameManager.TouchFinished;
            if (other.gameObject.name == "Other Fingertip" || other.gameObject.name == "Thumb")
            {
                LengthCheck(); // Check arm length
                menuText.text = "Enter"; // Update menu text
                lastContactPoint = other.ClosestPoint(startPoint.position); // Set new contact position
                if (!touchFinished) // Give user 8 frames on collision enter to use Point scroll type
                {
                    Scroll(other);
                }
                else
                {
                    // After collision, give approx 8 or 160ms to make selection then switch to dynamic scroll
                    DynamicScroll(other);
                }
                

            
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip" || other.gameObject.name == "Thumb")
            {
                isScrolling = true;
                if (!touchFinished) // Give user 8 frames after enter to use Point scroll type then switch
                {
                    Scroll(other);
                }
                else
                {
                    // After collision, give approx 160ms then dynamic scrolling
                    DynamicScroll(other);
                }
                
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip" || other.gameObject.name == "Thumb")
            {
                menuText.text = "Exit"; // Update menu text
                isScrolling = false;
               
            }
        }

        protected override void Scroll(Collider fingerCollider)
        {
            Vector3 contactPoint = fingerCollider.ClosestPoint(startPoint.position); // Get the closest contact point

            int totalBins = GameManager.NumberOfItems; // Total number of bins for scrolling

            // Calculate arm length and offsets
            float armLength = (endPoint.position - startPoint.position).magnitude;
            float startOffset = StartOffsetPercentage * armLength;
            float endOffset = EndOffsetPercentage * armLength;

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

            scrollCounter++;
            if (scrollCounter >= 8)
            {
                touchFinished = true;
                gameManager.TouchFinished = true;
            }

            // Update distance text
            distText.text = "Initial Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString(CultureInfo.InvariantCulture) + " " + EndOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
        }

        private void DynamicScroll(Collider colliderInfo)
        {
            Vector3 currentContactPoint = colliderInfo.ClosestPoint(startPoint.position);
            if (Vector3.Distance(lastContactPoint, currentContactPoint) < slowMovementThreshold)
            {
                lastContactPoint = currentContactPoint;
                return;
            }
            float normalisedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, currentContactPoint);
            float previousNormalizedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, lastContactPoint);
            float normalisedPositionDifference = normalisedPosition - previousNormalizedPosition;
            currentScrollSpeed = normalisedPositionDifference * scrollSpeed;

            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += currentScrollSpeed; // Addition because moving the hand up should scroll down

            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text = $"Point Dynamic Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position {currentScrollSpeed}";

            // Update the last contact point
            lastContactPoint = currentContactPoint;
        }

        private void Update()
        {
            // Apply inertia
            if (!isScrolling && currentScrollSpeed != 0)
            {
                currentScrollSpeed = Mathf.MoveTowards(currentScrollSpeed, 0, deceleration * Time.deltaTime);

                Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
                newScrollPosition.y += currentScrollSpeed / 1.36f; // Adjusted for inertia
                newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
                scrollableList.content.anchoredPosition = newScrollPosition;
            }
        }

        void AdjustSpeed()
        {
            switch (AreaNum)
            {
                case 4:
                    scrollSpeed *= fingerScrollMultiplier; // Increase scroll speed for finger
                    slowMovementThreshold /= 2; // Decrease slow threshold by 1/2
                    break;
                case 5:
                    scrollSpeed *= fingertipScrollMultiplier; // Increase for fingertip
                    slowMovementThreshold /= 4; // Decrease threshold by 1/4
                    break;
            }
        }
    }
}
