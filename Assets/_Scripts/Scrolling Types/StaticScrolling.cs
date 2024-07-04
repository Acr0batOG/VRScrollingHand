using System;
using _Scripts.Scrolling_Interface;
using UnityEngine;

namespace _Scripts.Scrolling_Types
{
    public class StaticScrolling : ScrollBase, IScrollable
    {
        protected float armThreshold = 150f;
        protected float handThreshold = 125f;
        protected float fingerThreshold = 75f;
        protected float fingertipThreshold = 40f; 
        public override void Start()
        {
            UIScrollSpeed = 75f;
        }
        public void Setup()
        {
            
        }
        protected void OnTriggerEnter(Collider other)
        {
            menuText.text = "Enter";
            Scroll(other);
             // Start dwell selection coroutine
            if (dwellCoroutine == null)
            {
                dwellCoroutine = StartCoroutine(DwellSelection()); //Start dwell selection
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            Scroll(other);
            // Restart dwell selection coroutine if list position changes significantly
            if (dwellCoroutine == null || !(Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) >
                                            dwellThreshold)) return;
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = StartCoroutine(DwellSelection()); //Reset selection if too much movement
        }
        protected void OnTriggerExit(Collider other){
            menuText.text = "Exit";
            // Stop dwell selection coroutine on exit
            if (dwellCoroutine == null) return;
            StopCoroutine(dwellCoroutine); //Stop dwell on exit
            dwellCoroutine = null;
        }
        

        public void Scroll(Collider fingerCollider){
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
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;

            // Calculate the new scroll position based on the distance from the middle point
            float deltaY = (contactPoint - middlePoint).magnitude * polarity * UIScrollSpeed;
            CheckThreshold(contactPoint, middlePoint, threshold);
            switch (areaNum)
            {
                case 2:
                case 1:
                {
                    if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
                        return; //Middle dead zone for no scrolling
                    break;
                }
                case 3:
                case 4:
                {
                    if (Math.Abs(contactPoint.x - middlePoint.x) <= threshold)
                        return; //Middle dead zone for no scrolling
                    break;
                }
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

        private float GetThreshold()
        {
            return areaNum switch
            {
                //Update speed for area postion for scroll
                1 => capsuleCollider.height / armThreshold //Arm Threshold - 165f
                ,
                2 => capsuleCollider.height / handThreshold //Hand Threshold - 150f
                ,
                3 => capsuleCollider.height / fingerThreshold //Finger Threshold - 100f
                ,
                4 => capsuleCollider.height / fingertipThreshold //Fingertip Threshold - 50f
                ,
                _ => capsuleCollider.height / 165f
            };
        }
        void CheckThreshold(Vector3 contactPoint, Vector3 middlePoint, float threshold){
            
            if(contactPoint.magnitude <= middlePoint.magnitude+threshold&&contactPoint.magnitude >= middlePoint.magnitude-threshold)
                return; //Middle dead zone for no scrolling
        }
    }
}