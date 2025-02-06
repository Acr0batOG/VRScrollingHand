using _Scripts.Calculators;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class DynamicScrollArmUIController : ArmUIController
    {
        private float scrollSpeed = 550f; // Speed multiplier for scrolling
        private Vector3 lastContactPoint = Vector3.zero; // Used for dynamic scrolling to detect where the last hand position was
        private float slowMovementThreshold = .001f; // To detect and ignore movement within the collision below this threshold
        private float contentHeight;
        private float viewportHeight;

        // Inertia-related variables
        private float currentScrollSpeed;
        private float deceleration = 75f; // Rate at which scrolling slows down
        private bool isScrolling;

        // Flick detection variables
        private float flickSpeedThreshold = 5f; // Speed threshold for flick detection
        private int numberOfFlicks = 0;
        private float lastFlickTime = 0f;

        protected new void Start()
        {
            base.Start();
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
            trialStartTime = Time.time;
        }

        protected void OnTriggerEnter(Collider other)
        {
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
            Vector3 currentContactPoint = other.ClosestPoint(startPoint.position);

            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Enter";

                // Log the initial touch position of the object
                float normalisedLandingPoint = ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, currentContactPoint);
                gameManager.NormalisedLandingPoint = normalisedLandingPoint;

                // Initialize last contact point but don't scroll yet
                lastContactPoint = currentContactPoint;
                
                Scroll(other);
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                isScrolling = true;
                Scroll(other);
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

            if (Vector3.Distance(lastContactPoint, currentContactPoint) < slowMovementThreshold)
            {
                lastContactPoint = currentContactPoint;
                return;
            }

            float normalisedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, currentContactPoint);
            float previousNormalizedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, lastContactPoint);
            float normalisedPositionDifference = normalisedPosition - previousNormalizedPosition;
            currentScrollSpeed = normalisedPositionDifference * scrollSpeed;

            // Flick detection
            if (Mathf.Abs(currentScrollSpeed) > flickSpeedThreshold)
            {
                numberOfFlicks++;
                float timeSinceLastFlick = Time.time - lastFlickTime;
                timeBetweenSwipesArray.Add(timeSinceLastFlick);
                lastFlickTime = Time.time;

                Debug.Log($"Flick Detected! Number of Flicks: {numberOfFlicks}, Time Between Flicks: {timeSinceLastFlick}");
            }

            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += currentScrollSpeed; // Addition because moving the hand up should scroll down

            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update the distance text
            distText.text = $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position {currentScrollSpeed}";

            float handMovement = Vector3.Distance(lastContactPoint, currentContactPoint);
            totalAmplitudeOfSwipe += handMovement;
            Debug.Log(totalAmplitudeOfSwipe + " Amplitude Of Swipe");

            swipeAmplitude = Mathf.Abs(normalisedPosition - previousNormalizedPosition);

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
                newScrollPosition.y += currentScrollSpeed / 1.36f;
                newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
                scrollableList.content.anchoredPosition = newScrollPosition;
            }
        }
    }
}
