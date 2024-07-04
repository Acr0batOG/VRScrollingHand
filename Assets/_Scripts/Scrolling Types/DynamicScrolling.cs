using Calculators;
using Scrolling_Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scrolling_Types
{
    public class DynamicScrolling : ScrollBase, IScrollable
    { 
        private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
        private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
        //private float fingertipScrollMultiplier = 3.0f;
        protected new void Start()
        {
            // contentHeight = scrollableList.content.sizeDelta.y;
            // viewportHeight = scrollableList.viewport.rect.height;
        }
        public void Setup()
        {
            
        }
        
        protected void OnTriggerEnter(Collider other)
        {
            menuText.text = "Enter";
            // Initialize last contact point but don't scroll yet
            lastContactPoint = other.ClosestPoint(base.startPoint.position);
            
            Scroll(other);
             // Start dwell selection coroutine
            if (dwellCoroutine == null)
            {
                dwellCoroutine = StartCoroutine(DwellSelection());
            }
        }
        
        protected void OnTriggerStay(Collider other)
        {   
            Scroll(other);
            // Restart dwell selection coroutine if list position changes significantly
            if (dwellCoroutine == null || 
                !(Mathf.Abs(scrollableList.content.anchoredPosition.y - previousScrollPosition) > dwellThreshold)) return;
            StopCoroutine(dwellCoroutine);
            dwellCoroutine = StartCoroutine(DwellSelection());
        
        }
        
        protected void OnTriggerExit(Collider other)
        {
        
            menuText.text = "Exit";
            // Stop dwell selection coroutine on exit
            if (dwellCoroutine != null)
            {
                StopCoroutine(dwellCoroutine);
                dwellCoroutine = null;
            }
        }
        

        public void Scroll(Collider colliderInfo)
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
            distText.text = $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position  {currentContactPoint.z}";
        
            // Update the last contact point
            lastContactPoint = currentContactPoint;
        } 
    }
}