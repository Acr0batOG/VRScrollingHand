using System;
using _Scripts.Calculators;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class PinchScroll : ArmUIController
    {
        // Start is called before the first frame update
       private float scrollSpeed = 1000f; // Speed multiplier for scrolling
        private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
        private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
        private float contentHeight;
        private float viewportHeight;

        // Inertia-related variables
        private float currentScrollSpeed;
        private float deceleration = 60f; // Rate at which scrolling slows down
        private bool isScrolling;

        protected new void Start()
        {
            base.Start();
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Enter";
                //Debug.Log(other.gameObject.name);
                // Initialize last contact point but don't scroll yet
                lastContactPoint = other.ClosestPoint(startPoint.position);

                Scroll(other);
               
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                isScrolling = true;
                Scroll(other);

                // Restart dwell selection coroutine if list position changes significantly
                
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Exit";
                isScrolling = false;
              
                
            }
        }

        protected override void Scroll(Collider colliderInfo)
        {
            Vector3 currentContactPoint = colliderInfo.ClosestPoint(startPoint.position);

            // Calculate the vertical difference
            float positionDifference = currentContactPoint.y - lastContactPoint.y;

            if (Vector3.Distance(lastContactPoint, currentContactPoint) < slowMovementThreshold)
            {
                lastContactPoint = currentContactPoint;
                return;
            }

            // Update scroll speed based on the vertical difference
            currentScrollSpeed = positionDifference * scrollSpeed;

            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += currentScrollSpeed; // Addition because moving the hand up should scroll down

            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text = $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position  {currentScrollSpeed}";

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
                newScrollPosition.y += currentScrollSpeed/1.36f;
                newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
                scrollableList.content.anchoredPosition = newScrollPosition;
            }
        }
    }
}
