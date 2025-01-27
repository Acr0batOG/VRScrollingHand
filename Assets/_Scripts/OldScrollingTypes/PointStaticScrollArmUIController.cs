
using System;
using System.Globalization;
using _Scripts.GameState;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace _Scripts.OldScrollingTypes
{
    public class PointStaticScrollArmUIController : PointScrollArmUIController
    {
        [SerializeField] private float staticScrollSpeed = 75f; //Speed multiplier for static scrolling
        private GameManager gameManager;
        private int scrollCounter;
        private bool touchFinished;
        private Coroutine pauseCoroutine; // Coroutine for the pause
        protected float armThreshold = 200f;
        protected float handThreshold = 100f;
        protected float fingerThreshold = 65f;
        protected float fingertipThreshold = 35f;
        
        protected new void Start()
        {
            base.Start();
            gameManager = GameManager.instance;
            LengthCheck(); // Check arm length
            
        }

        private void OnTriggerEnter(Collider other)
        {
            touchFinished = gameManager.TouchFinished;
            if (other.gameObject.name == "Other Fingertip")
            {
                Debug.Log(other.gameObject.name);
                LengthCheck(); // Check arm length
                menuText.text = "Enter"; // Update menu text
                if (!touchFinished)
                    Scroll(other); // Scroll through the content
                else
                {
                    StaticScroll(other);
                }
                

                // Start dwell selection coroutine
                //DwellCoroutine ??= StartCoroutine(DwellSelection());
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                if (!touchFinished)
                {
                    Scroll(other);
                }
                else
                {
                    //After collision give appx 160ms to make selection then switch to Static scroll

                    StaticScroll(other);
                }

              
                
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

            int totalBins = gameManager.NumberOfItems; // Total number of bins for scrolling

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
        
            distText.text = "Initial Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString(CultureInfo.InvariantCulture) + " " + EndOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
            if (scrollCounter >= 8)
            {
                touchFinished = true;
                gameManager.TouchFinished = true;
            }
        }

        private void StaticScroll(Collider collisionInfo){
            Vector3 contactPoint = collisionInfo.ClosestPoint(startPoint.position);

            // Calculate the middle point between startPoint and endPoint
            Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;

            float threshold = GetThreshold(); //Determine threshold size base on collision object
            
            // Calculate the distance from the contact point to the start and end points
            float distanceFromStart = (contactPoint - startPoint.position).magnitude;
            float distanceFromEnd = (contactPoint - endPoint.position).magnitude;

            // Determine the polarity based on which end the contact point is closer to
            int polarity = distanceFromStart > distanceFromEnd ? -1 : 1;

            // Get the content height and the viewport height
            float contentHeight = scrollableList.content.sizeDelta.y;
            float viewportHeight = scrollableList.viewport.rect.height;

            // Calculate the new scroll position based on the distance from the middle point
            float deltaY = (contactPoint - middlePoint).magnitude * polarity * staticScrollSpeed;
            if(AreaNum==2||AreaNum==1){
                if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
                    return; //Middle dead zone for no scrolling
            }else if(AreaNum==3||AreaNum==4){
                if (Math.Abs(contactPoint.x - middlePoint.x) <= threshold)
                    return; //Middle dead zone for no scrolling
            }
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
        float GetThreshold()
        {
            switch (AreaNum)
            {
                //Update speed for area postion for scroll
                case 1:
                    return capsuleCollider.height / armThreshold; //Arm Threshold - 165f
                case 2:
                    return capsuleCollider.height / handThreshold; //Hand Threshold - 150f
                case 3:
                    return capsuleCollider.height / fingerThreshold; //Finger Threshold - 100f
                case 4:
                    return capsuleCollider.height / fingertipThreshold; //Fingertip Threshold - 50f
                default:
                    return capsuleCollider.height / 165f;
            }
        }
    }
}
