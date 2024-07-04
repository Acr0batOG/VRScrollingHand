using Scrolling_Interface;
using UnityEngine;

namespace Scrolling_Types
{
    public class PointStaticScrolling : ScrollBase, IScrollable
    {
        private const int TriggerTimeMax = 8;
        // protected float handSpeed = 1.06f;
        // protected float fingerSpeed = 5.0f;
        // protected float fingertipSpeed = 10.0f;
        private int triggerTimer;
        private Vector3 collisionPoint;

        public override void Start()
        {
            base.Start();
            UIScrollSpeed = 75f;
        }
        public void Setup()
        {
            
        }
        private void OnTriggerEnter(Collider other)
        {
            menuText.text = "Enter"; // Update menu text
            Scroll(other); // Scroll through the content
            // Start dwell selection coroutine
            if (dwellCoroutine == null)
            {
                dwellCoroutine = StartCoroutine(DwellSelection()); //Used for selection (in superclass)
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
                //After collision give appx 160ms to make selection then switch to Static scroll

                StaticScroll(other, collisionPoint);
            }

            // Restart dwell selection coroutine if list position changes significantly
            if (dwellCoroutine != null &&
                Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)
            {
                StopCoroutine(dwellCoroutine);
                dwellCoroutine = StartCoroutine(DwellSelection()); //Reset the selection if too much movement 
            }
        }

        private void OnTriggerExit(Collider other)
        {
            triggerTimer = 0; //Only reset to other method if collision done
            menuText.text = "Exit"; // Update menu text
            // Stop dwell selection coroutine on exit
            if (dwellCoroutine != null)
            {
                StopCoroutine(dwellCoroutine); //Reset selection on exit
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
            triggerTimer++;

            collisionPoint =
                colliderInfo.ClosestPoint(startPoint
                    .position); //Set middle point to location where last point selection was made
            distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString() +
                            " " + endOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
        }

        protected void StaticScroll(Collider collisionInfo, Vector3 staticCollisionPoint)
        {
            Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);
            //Set middle of static scroll to point of initial collision
            Vector3 middlePoint = staticCollisionPoint;
            //Set the width of the deadzone for selection
            float threshold = capsuleCollider.height / 165f;

            // Determine the polarity based on which end the contact point is closer to
            int polarity = contactPoint.magnitude > middlePoint.magnitude ? -1 : 1;


            // Get the content height and the viewport height
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;

            // Calculate the new scroll position based on the distance from the middle point
            float deltaY = (contactPoint - middlePoint).magnitude * polarity * UIScrollSpeed;
            if (contactPoint.magnitude <= middlePoint.magnitude + threshold &&
                contactPoint.magnitude >= middlePoint.magnitude - threshold)
                return; //Middle dead zone for no scrolling
            // Update the new scroll position
            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += deltaY;

            // Clamp the new scroll position within the scrollable area
            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

            // Set the new anchored position for the scroll content
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text = "Point Static Scroll: Position " + contactPoint.ToString() + " " +
                            newScrollPosition.y.ToString();
        }
    }
}
