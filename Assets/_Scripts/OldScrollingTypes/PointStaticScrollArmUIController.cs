using System.Collections;
using System.Globalization;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace _Scripts.OldScrollingTypes
{
    public class PointStaticScrollArmUIController : PointScrollArmUIController
    {
        [SerializeField] private float staticScrollSpeed = 75f; //Speed multiplier for static scrolling
        private const int TriggerTimeMax = 8;
        private readonly float handSpeed = 1.06f;
        private readonly float fingerSpeed = 5.0f;
        private readonly float fingertipSpeed = 10.0f;
        private int triggerTimer;
        private Coroutine pauseCoroutine; // Coroutine for the pause
        Vector3 collisionPoint;
        protected new void Start()
        {

            base.Start();
            LengthCheck(); // Check arm length
            SpeedControl();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                Debug.Log(other.gameObject.name);
                LengthCheck(); // Check arm length
                menuText.text = "Enter"; // Update menu text
                if (triggerTimer < TriggerTimeMax)
                    Scroll(other); // Scroll through the content
                else
                {
                    StaticScroll(other, collisionPoint);
                }

                // Cancel the pause coroutine if a new collision starts
                if (pauseCoroutine != null)
                {
                    StopCoroutine(pauseCoroutine);
                    pauseCoroutine = null;
                }

                // Start dwell selection coroutine
                DwellCoroutine ??= StartCoroutine(DwellSelection());
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
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
                if (DwellCoroutine != null &&
                    Mathf.Abs(scrollableList.content.anchoredPosition.y - PreviousScrollPosition) > DwellThreshold)
                {
                    StopCoroutine(DwellCoroutine);
                    DwellCoroutine = StartCoroutine(DwellSelection()); //Reset the selection if too much movement 
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Exit"; // Update menu text
                // Stop dwell selection coroutine on exit
                // Start the pause coroutine
                if (pauseCoroutine != null)
                {
                    StopCoroutine(
                        pauseCoroutine); //On exit: Keep dynamic scrolling for 1.8 seconds, reset to point if exceeded 
                }

                pauseCoroutine = StartCoroutine(PauseBeforeResetCoroutine());
                if (DwellCoroutine != null)
                {
                    StopCoroutine(DwellCoroutine); //Reset selection on exit
                    DwellCoroutine = null;
                }
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
            triggerTimer++;  
        
            collisionPoint = fingerCollider.ClosestPoint(startPoint.position); //Set middle point to location where last point selection was made
            distText.text = "Initial Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString(CultureInfo.InvariantCulture) + " " + EndOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
        }

        private void StaticScroll(Collider collisionInfo, Vector3 pointStaticCollisionPoint){
            Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);
            //Set middle of static scroll to point of initial collision
            Vector3 middlePoint = pointStaticCollisionPoint;
            //Set the width of the dead zone for selection
            float threshold = capsuleCollider.height/145f;

            // Determine the polarity based on which end the contact point is closer to
            int polarity = contactPoint.magnitude > middlePoint.magnitude ? -1 : 1;
      

            // Get the content height and the viewport height
            // Get the content height and the viewport height
            float contentHeight = scrollableList.content.sizeDelta.y;
            float viewportHeight = scrollableList.viewport.rect.height;

            // Calculate the new scroll position based on the distance from the middle point
            float deltaY = (contactPoint - middlePoint).magnitude * polarity * staticScrollSpeed;
            if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
                return; //Middle dead zone for no scrolling
            // Update the new scroll position
            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += deltaY;

            // Clamp the new scroll position within the scrollable area
            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

            // Set the new anchored position for the scroll content
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text = "Point Static Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString(CultureInfo.InvariantCulture);
        }

        private IEnumerator PauseBeforeResetCoroutine()
        {
            yield return new WaitForSeconds(1.2f); // Pause for 1.2 seconds before resetting
            //Used to continue dynamic scrolling for 1.2s after exit, reset if exceeded
            triggerTimer = 0; // Reset trigger timer after 1.2 seconds for back to static scrolling
        }
        void SpeedControl(){
            int speedControlAreaNum = GameManager.AreaNumber; // Check the area number

            switch(speedControlAreaNum){
                case 1: 
                    break;
                case 2:
                    staticScrollSpeed *= handSpeed; //Increase speed for each type
                    break;
                case 3:
                    staticScrollSpeed *= fingerSpeed;
                    break;
                case 4:
                    staticScrollSpeed *= fingertipSpeed;
                    break;
            }
        
        }
    }
}
