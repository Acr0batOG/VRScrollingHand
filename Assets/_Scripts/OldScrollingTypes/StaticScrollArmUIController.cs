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
    
        protected new void Start()
        {
            base.Start();

        }
        protected void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                //Debug.Log(other.gameObject.name);
                menuText.text = "Enter";
                Scroll(other);
                // Start dwell selection coroutine
                if (DwellCoroutine == null)
                {
                    DwellCoroutine = StartCoroutine(DwellSelection()); //Start dwell selection
                }
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                Scroll(other);
                // Restart dwell selection coroutine if list position changes significantly
                if (DwellCoroutine != null &&
                    Mathf.Abs(scrollableList.content.anchoredPosition.y - PreviousScrollPosition) > DwellThreshold)
                {
                    StopCoroutine(DwellCoroutine);
                    DwellCoroutine = StartCoroutine(DwellSelection()); //Reset selection if too much movement
                }
            }
        }
        protected void OnTriggerExit(Collider other){
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Exit";
                // Stop dwell selection coroutine on exit
                if (DwellCoroutine != null)
                {
                    StopCoroutine(DwellCoroutine); //Stop dwell on exit
                    DwellCoroutine = null;
                }
            }
        }

        protected override void Scroll(Collider fingerCollider){
            Vector3 contactPoint = fingerCollider.ClosestPoint(startPoint.position);

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
