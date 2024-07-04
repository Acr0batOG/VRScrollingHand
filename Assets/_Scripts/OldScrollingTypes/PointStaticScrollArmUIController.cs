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
        Vector3 collisionPoint;
        protected new void Start()
        {

            base.Start();
            LengthCheck(); // Check arm length
            SpeedControl();
        }

        private void OnTriggerEnter(Collider other)
        {
            LengthCheck(); // Check arm length
            menuText.text = "Enter"; // Update menu text
            Scroll(other); // Scroll through the content
            // Start dwell selection coroutine
            DwellCoroutine ??= StartCoroutine(DwellSelection());
        }

        private void OnTriggerStay(Collider other)
        {
            if (triggerTimer<TriggerTimeMax){
                Scroll(other);
            }else { //After collision give appx 160ms to make selection then switch to Static scroll
            
                StaticScroll(other, collisionPoint);
            }
            // Restart dwell selection coroutine if list position changes significantly
            if (DwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - PreviousScrollPosition) > DwellThreshold)
            {
                StopCoroutine(DwellCoroutine);
                DwellCoroutine = StartCoroutine(DwellSelection()); //Reset the selection if too much movement 
            }
        }

        private void OnTriggerExit(Collider other)
        {
            triggerTimer = 0; //Only reset to other method if collision done
            menuText.text = "Exit"; // Update menu text
            // Stop dwell selection coroutine on exit
            if (DwellCoroutine != null)
            {
                StopCoroutine(DwellCoroutine); //Reset selection on exit
                DwellCoroutine = null;
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
            distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString(CultureInfo.InvariantCulture) + " " + EndOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
        }

        private void StaticScroll(Collider collisionInfo, Vector3 pointStaticCollisionPoint){
            Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);
            //Set middle of static scroll to point of initial collision
            Vector3 middlePoint = pointStaticCollisionPoint;
            //Set the width of the dead zone for selection
            float threshold = capsuleCollider.height/165f;

            // Determine the polarity based on which end the contact point is closer to
            int polarity = contactPoint.magnitude > middlePoint.magnitude ? -1 : 1;
      

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
