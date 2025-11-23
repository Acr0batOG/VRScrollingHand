using System.Collections;
using System.Collections.Generic;
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

        private Queue<(float time, float speed)> speedHistory = new Queue<(float, float)>();

        // Inertia-related variables
        private float currentScrollSpeed;
        private float deceleration = 75f; // Rate at which scrolling slows down
        private bool isScrolling;

        protected new void Start()
        {
            base.Start();
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
            trialStartTime = Time.time;
            previousSelectedItem = gameManager.SelectedItem;
        }

        protected void OnTriggerEnter(Collider other)
        {
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
            Vector3 currentContactPoint = other.ClosestPoint(startPoint.position);
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Enter";
                //Log the inital touch position of the object
                float normalisedLandingPoint = ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, currentContactPoint);
                gameManager.NormalisedLandingPoint = normalisedLandingPoint;
                //Debug.Log(other.gameObject.name);
                // Initialize last contact point but don't scroll yet
                lastContactPoint = other.ClosestPoint(startPoint.position);
                
                timeBetweenSwipes = Time.time - lastSwipeTime; // Time since the last swipe
                Debug.Log("Time between " + timeBetweenSwipes);
                if(timeBetweenSwipes < 2.0f)
                    timeBetweenSwipesArray.Add(timeBetweenSwipes);
                lastSwipeTime = Time.time;

                Scroll(other);
               
            }
        }

        protected void OnTriggerStay(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                isScrolling = true;
                Scroll(other);
                
                gameManager.TotalAmplitudeOfSwipes = totalAmplitudeOfSwipe;
                gameManager.NumberOfFlicks = numberOfFlicks;
                gameManager.TimeBetweenSwipesArray = timeBetweenSwipesArray;

            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name == "Other Fingertip")
            {
                menuText.text = "Exit";
                isScrolling = false;
                
                numberOfFlicks++; // Count this as a flick
                Debug.Log("Flicks " + numberOfFlicks);
                totalSwipeTime += Time.time - lastSwipeTime;
                Debug.Log("Total Time " + totalSwipeTime);
                
                gameManager.TotalAmplitudeOfSwipes = totalAmplitudeOfSwipe;
                gameManager.NumberOfFlicks = numberOfFlicks;
                gameManager.TimeBetweenSwipesArray = timeBetweenSwipesArray;

                Debug.Log("Max Speed 1 ms" + absoluteMaxSpeed);

                Debug.Log("Max Speed 1 second" + maxSpeedSecond);

                Debug.Log("Max Speed 10 Second" + maxSpeedTenSecond);

             
                
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

            if(Mathf.abs(currentScrollSpeed) > absoluteMaxSpeed){
                absoluteMaxSpeed = Mathf.abs(currentScrollSpeed);
            }

            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += currentScrollSpeed; // Addition because moving the hand up should scroll down

            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
            scrollableList.content.anchoredPosition = newScrollPosition;
            
            // Update the distance text
            distText.text = $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Delta Position  {currentScrollSpeed}";
            
            float handMovement = Vector3.Distance(lastContactPoint, currentContactPoint);
            totalAmplitudeOfSwipe += handMovement;
            Debug.Log(totalAmplitudeOfSwipe + " Amplitude Of Swipe");

            swipeAmplitude = Mathf.Abs(normalisedPosition - previousNormalizedPosition);
            
            // Update the last contact point
            lastContactPoint = currentContactPoint;
            
            
            
        }

        private void UpdateAverageSpeeds(float now)
        {
            // 1-second average
            float sum1 = 0f;
            int count1 = 0;

            foreach (var (t, s) in speedHistory)
            {
                if (Time.time - t <= 1f)
                {
                    sum1 += s;
                    count1++;
                }
            }

            float avg1 = count1 > 0 ? sum1 / count1 : 0f;

            // 10-second average
            float sum10 = 0f;
            int count10 = 0;

            foreach (var (t, s) in speedHistory)
            {
                sum10 += s;
                count10++;
            }

            float avg10 = count10 > 0 ? sum10 / count10 : 0f;

            // Update max averages
            maxSpeedSecond = Mathf.Max(maxSpeedSecond, avg1);
            maxSpeedTenSecond = Mathf.Max(maxSpeedTenSecond, avg10);

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
            if (gameManager.SelectedItem != previousSelectedItem)
            {
                StartCoroutine(WaitBeforeReset());

            }

            float speed = Mathf.Abs(currentScrollSpeed);

            // Record this frame’s speed
            speedHistory.Enqueue((Time.time, speed));

            // Remove old entries > 10 seconds
            while (speedHistory.Count > 0 && Time.time - speedHistory.Peek().time > 10f)
                speedHistory.Dequeue();

            previousSelectedItem = gameManager.SelectedItem;
        }
        IEnumerator WaitBeforeReset()
        {
            yield return new WaitForSeconds(.1f);
            timeBetweenSwipesArray.Clear();
            numberOfFlicks = 0;
            totalAmplitudeOfSwipe = 0f;
            absoluteMaxSpeed = 0f;
            maxSpeedSecond = 0f;
            maxSpeedTenSecond = 0f;
            
        
        }
    }
    
}
