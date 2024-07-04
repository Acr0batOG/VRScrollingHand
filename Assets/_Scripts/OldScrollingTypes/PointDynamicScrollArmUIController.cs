using System.Collections;
using System.Globalization;
using _Scripts.Calculators;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class PointDynamicScrollArmUIController : PointScrollArmUIController //Inherit from PointScrollAnyways
    {
        private float scrollSpeed = 625f;
        private const int TriggerTimeMax = 8;
        private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
        private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
        private readonly float fingerScrollMultiplier = 2.1f;
        private readonly float fingertipScrollMultiplier = 3.0f;
        private int triggerTimer; //To speed up dynamic scrolling
        private Coroutine pauseCoroutine; // Coroutine for the pause
        float contentHeight;
        float viewportHeight;

        protected new void Start()
        {
            base.Start();
            LengthCheck(); // Check arm length
            AdjustSpeed(); // Update speed based on point used to scroll
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
        }

        private void OnTriggerEnter(Collider other)
        {
            LengthCheck(); // Check arm length
            menuText.text = "Enter"; // Update menu text
            lastContactPoint = other.ClosestPoint(startPoint.position); //Set new contact position
            if (triggerTimer < TriggerTimeMax) //Give user 8 frames on collision enter to use Point scroll type
            {
                Scroll(other);
            }
            else 
            {
                // After collision, give approx 8 or 160ms to make selection then switch to dynamic scroll
                DynamicScroll(other);
            }

            // Cancel the pause coroutine if a new collision starts
            if (pauseCoroutine != null)
            {
                StopCoroutine(pauseCoroutine);
                pauseCoroutine = null;
            }
            DwellCoroutine ??= StartCoroutine(DwellSelection());
        }

        private void OnTriggerStay(Collider other)
        {
            if (triggerTimer < TriggerTimeMax) //Give user 8 frames after enter to use Point scroll type then switch
            { 
                Scroll(other);
            }
            else
            {
                // After collision, give approx 160ms then dynamic scrolling
                DynamicScroll(other);
            }
        
            if (DwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - PreviousScrollPosition) > DwellThreshold)
            {
                StopCoroutine(DwellCoroutine); //If too much movement, reset dwell selection as scrolling is happening
                DwellCoroutine = StartCoroutine(DwellSelection());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            menuText.text = "Exit"; // Update menu text

            // Start the pause coroutine
            if (pauseCoroutine != null)
            {
                StopCoroutine(pauseCoroutine); //On exit: Keep dynamic scrolling for 1.8 seconds, reset to point if exceeded 
            }
            pauseCoroutine = StartCoroutine(PauseBeforeResetCoroutine());
            if (DwellCoroutine != null)
            {
                StopCoroutine(DwellCoroutine); //Also reset dwell selection on exit
                DwellCoroutine = null;
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

            triggerTimer++; // 160ms or 8 frames of this scroll type 

            // Update distance text
            distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString(CultureInfo.InvariantCulture) + " " + EndOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
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
            float deltaY = normalisedPositionDifference * scrollSpeed;
            

            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += deltaY; // Addition because moving the hand up should scroll down

            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text = $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position  {deltaY}";

            // Update the last contact point
            lastContactPoint = currentContactPoint;
        
        }
        

        private IEnumerator PauseBeforeResetCoroutine()
        {
            yield return new WaitForSeconds(1.8f); // Pause for 1.8 seconds before resetting
            //Used to continue dynamic scrolling for 1.8s after exit, reset if exceeded
            triggerTimer = 0; // Reset trigger timer after 1.8 seconds for back to static scrolling
        }

        // Check arm length and adjust offsets accordingly
        

        void AdjustSpeed()
        {
            switch (AreaNum)
            {
                case 3:
                    scrollSpeed *= fingerScrollMultiplier; // Increase scroll speed for finger
                    slowMovementThreshold /= 2; // Decrease slow threshold by 1/2
                    break;
                case 4:
                    scrollSpeed *= fingertipScrollMultiplier; //Increase for fingertip
                    slowMovementThreshold /= 4; //Decrease threshold by 1/4
                    break;
            }
        }
    }
}
