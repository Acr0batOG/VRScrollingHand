using System.Collections;
using Scrolling_Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scrolling_Types
{
    public class PointDynamicScrolling : ScrollBase, IScrollable
    {

        private const int TriggerTimeMax = 8;
        private int triggerTimer = 0;
        private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
        private Coroutine pauseCoroutine; // Coroutine for the pause
        private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
        
        
        protected void Start()
        {
            // LengthCheck(); // Check arm length
            // AdjustSpeed(); // Update speed based on point used to scroll
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
        }
        
        public void Setup()
        {
            
        }

        private void OnTriggerEnter(Collider other)
        {
            // LengthCheck(); // Check arm length
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

            if (dwellCoroutine == null) //Ised for selection.
            {
                dwellCoroutine = StartCoroutine(DwellSelection());
            }
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

            if (dwellCoroutine != null &&
                Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)
            {
                StopCoroutine(dwellCoroutine); //If too much movement, reset dwell selection as scrolling is happening
                dwellCoroutine = StartCoroutine(DwellSelection());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            menuText.text = "Exit"; // Update menu text

            // Start the pause coroutine
            if (pauseCoroutine != null)
            {
                StopCoroutine(
                    pauseCoroutine); //On exit: Keep dynamic scrolling for 1.8 seconds, reset to point if exceeded 
            }

            pauseCoroutine = StartCoroutine(PauseBeforeResetCoroutine());
            if (dwellCoroutine != null)
            {
                StopCoroutine(dwellCoroutine); //Also reset dwell selection on exit
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
            float armLength = (endPoint.position - startPoint.position).magnitude;
            float startOffset = startOffsetPercentage * armLength;
            float endOffset = endOffsetPercentage * armLength;

            // Calculate contact and adjusted contact positions
            float contactPosition = (contactPoint - startPoint.position).magnitude;
            float adjustedContactPosition = Mathf.Clamp(contactPosition - startOffset, 0, endOffset - startOffset);

            // Calculate bin index based on adjusted contact position
            int binIndex =
                Mathf.Clamp(
                    Mathf.RoundToInt((1 - (adjustedContactPosition / (endOffset - startOffset))) * (totalBins - 1)), 0,
                    totalBins - 1) + 1;

            // Calculate bin height and new scroll position
            float binHeight = (contentHeight - viewportHeight) / (totalBins - 1);
            float newScrollPositionY = (binIndex - 1) * binHeight;

            // Set the new scroll position
            Vector2 newScrollPosition = new Vector2(scrollableList.content.anchoredPosition.x, newScrollPositionY);
            scrollableList.content.anchoredPosition = newScrollPosition;

            triggerTimer++; // 160ms or 8 frames of this scroll type 

            // Update distance text
            distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString() +
                            " " + endOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
        }

        private void DynamicScroll(Collider colliderInfo)
        {
            Vector3 currentContactPoint = colliderInfo.ClosestPoint(base.startPoint.position);
            if (Vector3.Distance(lastContactPoint, currentContactPoint) < slowMovementThreshold)
            {
                lastContactPoint = currentContactPoint;
                return;
            }

            float normalisedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(
                endPoint.position, startPoint.position, colliderInfo.transform.position);
            Debug.Log("Current normalized position: " + normalisedPosition);
            float previousNormalizedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(
                endPoint.position, startPoint.position, lastContactPoint);
            float normalisedPositionDifference = normalisedPosition - previousNormalizedPosition;
            float deltaY = normalisedPositionDifference * UIScrollSpeed;

            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += deltaY; // Addition because moving the hand up should scroll down
            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text =
                $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position  {currentContactPoint.z}";

            // Update the last contact point
            lastContactPoint = currentContactPoint;
        }

        private IEnumerator PauseBeforeResetCoroutine()
        {
            yield return new WaitForSeconds(1.8f); // Pause for 1.8 seconds before resetting
            //Used to continue dynamic scrolling for 1.8s after exit, reset if exceeded
            triggerTimer = 0; // Reset trigger timer after 1.8 seconds for back to static scrolling
        }

        
    }
}