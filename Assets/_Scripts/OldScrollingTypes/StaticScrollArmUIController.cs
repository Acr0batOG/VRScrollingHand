using System;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class StaticScrollArmUIController : ArmUIController{
        [SerializeField] private float staticScrollSpeed = 75f; //Speed multiplier for static scrolling 
        protected float handSpeed = 1.06f;
        protected float fingerSpeed = 5.0f;
        protected float fingertipSpeed = 10.0f;
        protected float armThreshold = 200f;
        protected float handThreshold = 100f;
        protected float fingerThreshold = 65f;
        protected float fingertipThreshold = 35f;
        [SerializeField] protected float endpointOffset = .05f;
        [SerializeField] protected float startpointOffset = .05f;

        protected new void Start()
        {
            base.Start();

        }
        protected void OnTriggerEnter(Collider other)
        {
            
            if (other.gameObject.name == "Other Fingertip" || other.gameObject.name == "Thumb")
            {
                //Debug.Log(other.gameObject.name);
                menuText.text = "Enter";
                Scroll(other);
                
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip" || other.gameObject.name == "Thumb")
            {
                Scroll(other);
                // Restart dwell selection coroutine if list position changes significantly
               
            }
        }
        protected void OnTriggerExit(Collider other){
            if (other.gameObject.name == "Other Fingertip" || other.gameObject.name == "Thumb")
            {
                menuText.text = "Exit";
                
            }
        }

        protected override void Scroll(Collider fingerCollider){
            // Calculate the offset for the endpoint
            Vector3 directionToEnd = (endPoint.position - startPoint.position).normalized;
            Vector3 endOffset = directionToEnd * endpointOffset;
            Vector3 directionToStart = (startPoint.position - endPoint.position).normalized;
            Vector3 startOffset = directionToStart * startpointOffset;

            // Apply the offset to bring the endpoint closer
            Vector3 adjustedEndpointPosition = endPoint.position - endOffset;
            Vector3 adjustedStartpointPosition = startPoint.position - startOffset;
            Vector3 contactPoint = fingerCollider.ClosestPoint(startPoint.position);

            // Calculate the middle point between startPoint and the adjusted endpoint
            Vector3 middlePoint = (adjustedStartpointPosition + adjustedEndpointPosition) / 2f;

            float threshold = GetThreshold(); // Determine threshold size based on collision object

            // Calculate the distance from the contact point to the start and adjusted end points
            float distanceFromStart = (contactPoint - adjustedStartpointPosition).magnitude;
            float distanceFromAdjustedEnd = (contactPoint - adjustedEndpointPosition).magnitude;

            // Determine the polarity based on which end the contact point is closer to
            int polarity = distanceFromStart > distanceFromAdjustedEnd ? -1 : 1;

            // Get the content height and the viewport height
            float contentHeight = scrollableList.content.sizeDelta.y;
            float viewportHeight = scrollableList.viewport.rect.height;

            // Calculate the new scroll position based on the distance from the middle point
            float deltaY = (contactPoint - middlePoint).magnitude * polarity * staticScrollSpeed;
            //if(AreaNum==2||AreaNum==1){
                //if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
              //      return; //Middle dead zone for no scrolling
            //}else if(AreaNum==3||AreaNum==4){
            //    if (Math.Abs(contactPoint.x - middlePoint.x) <= threshold)
              //      return; //Middle dead zone for no scrolling
            //}
            // Update the new scroll position
            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += deltaY;

            // Clamp the new scroll position within the scrollable area
            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

            // Set the new anchored position for the scroll content
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text = "Static Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString();
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
                case 4:
                    return capsuleCollider.height / fingerThreshold; //Finger Threshold - 100f
                case 5:
                    return capsuleCollider.height / fingertipThreshold; //Fingertip Threshold - 50f
                default:
                    return capsuleCollider.height / 165f;
            }
        }

    }
}
