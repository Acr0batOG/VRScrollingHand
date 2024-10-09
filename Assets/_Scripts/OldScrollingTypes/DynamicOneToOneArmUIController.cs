using _Scripts.Calculators;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class DynamicOneToOneArmUIController : ArmUIController
    {
        private float scrollSpeed = 500f; // Speed multiplier for scrolling
    
        private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
        private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
        private readonly float fingerScrollMultiplier = 2.1f;
        private readonly float fingertipScrollMultiplier = 3.0f;
        float contentHeight;
        float viewportHeight;
    
        protected new void Start()
        {
            base.Start();
            AdjustSpeed();
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
        }

        protected void OnTriggerEnter(Collider other)
        {
            contentHeight = scrollableList.content.sizeDelta.y;
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Enter";
                Debug.Log(other.gameObject.name);
                // Initialize last contact point but don't scroll yet
                lastContactPoint = other.ClosestPoint(startPoint.position);

                Scroll(other);
            
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                Scroll(other);
                
            }

        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Exit";
               
            }
        }

        protected override void Scroll(Collider colliderInfo)
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
            distText.text = $"Dynamic One to One Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position  {deltaY}";

            // Update the last contact point
            lastContactPoint = currentContactPoint;
        }

        
        void AdjustSpeed(){
            switch(AreaNum){
                case 3:
                    scrollSpeed *= fingerScrollMultiplier; //Increase scroll speed for finger
                    slowMovementThreshold /= 2; //Decrease slow threshold
                    break;
                case 4:
                    scrollSpeed *= fingertipScrollMultiplier; //Increase speed for fingertip scroll
                    slowMovementThreshold /= 4;
                    break;
            }
        }
    }
}
