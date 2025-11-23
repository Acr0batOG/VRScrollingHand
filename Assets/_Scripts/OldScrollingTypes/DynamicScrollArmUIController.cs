using System.Collections;
using System.Collections.Generic;
using _Scripts.Calculators;
using System;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class DynamicScrollArmUIController : ArmUIController
    {
        private float scrollSpeed = 550f; // Speed multiplier for scrolling
        private Vector3 lastContactPoint = Vector3.zero;
        private float slowMovementThreshold = .001f;
        private float contentHeight;
        private float viewportHeight;

        private Queue<(float time, float speed)> speedHistory = new Queue<(float, float)>();

        // Added: physical arm length for m/s speed calculations
        private float armLengthMeters;

        // Inertia-related variables
        private float currentScrollSpeed;
        private float deceleration = 75f;
        private bool isScrolling;

        protected new void Start()
        {
            base.Start();

            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;

            // Compute arm length in meters 
            armLengthMeters = Vector3.Distance(startPoint.position, endPoint.position);

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

                float normalisedLandingPoint =
                    ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, currentContactPoint);

                gameManager.NormalisedLandingPoint = normalisedLandingPoint;

                lastContactPoint = other.ClosestPoint(startPoint.position);

                timeBetweenSwipes = Time.time - lastSwipeTime;
                Debug.Log("Time between " + timeBetweenSwipes);

                if (timeBetweenSwipes < 2.0f)
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

                numberOfFlicks++;
                totalSwipeTime += Time.time - lastSwipeTime;

                gameManager.TotalAmplitudeOfSwipes = totalAmplitudeOfSwipe;
                gameManager.NumberOfFlicks = numberOfFlicks;
                gameManager.TimeBetweenSwipesArray = timeBetweenSwipesArray;

                Debug.Log("Max Speed Instant (m/s): " + absoluteMaxSpeed);
                Debug.Log("Max Speed 1 Second (m/s): " + maxSpeedSecond);
                Debug.Log("Max Speed 10 Second (m/s): " + maxSpeedTenSecond);
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

            float normalisedPosition =
                ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, currentContactPoint);

            float previousNormalizedPosition =
                ArmPositionCalculator.GetNormalisedPositionOnArm(endPoint.position, startPoint.position, lastContactPoint);

            float normalisedPositionDifference =
                normalisedPosition - previousNormalizedPosition;

            // --------------------------------------------------
            // ✔ NEW: Calculate REAL PHYSICAL SPEED IN METERS/SECOND
            // --------------------------------------------------
            float distanceMeters = normalisedPositionDifference * armLengthMeters;
            float speedMetersPerSecond = Mathf.Abs(distanceMeters / Time.deltaTime);

            // Track max instant speed (m/s)
            if (speedMetersPerSecond > absoluteMaxSpeed)
                absoluteMaxSpeed = speedMetersPerSecond;

            // Also record into speedHistory
            speedHistory.Enqueue((Time.time, speedMetersPerSecond));

            // --------------------------------------------------
            // KEEP YOUR ORIGINAL SCROLLING SYSTEM COMPLETELY INTACT
            // --------------------------------------------------

            currentScrollSpeed = normalisedPositionDifference * scrollSpeed;

            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += currentScrollSpeed;
            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
            scrollableList.content.anchoredPosition = newScrollPosition;

            distText.text =
                $"Dynamic Standard Scroll: Position {currentContactPoint} Scroll Position {newScrollPosition.y} Δ {currentScrollSpeed}";

            float handMovement = Vector3.Distance(lastContactPoint, currentContactPoint);
            totalAmplitudeOfSwipe += handMovement;

            swipeAmplitude = Mathf.Abs(normalisedPositionDifference);
            lastContactPoint = currentContactPoint;
        }

        private void UpdateAverageSpeeds(float now)
        {
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

            float sum10 = 0f;
            int count10 = 0;

            foreach (var (t, s) in speedHistory)
            {
                sum10 += s;
                count10++;
            }

            float avg10 = count10 > 0 ? sum10 / count10 : 0f;

            maxSpeedSecond = Mathf.Max(maxSpeedSecond, avg1);
            maxSpeedTenSecond = Mathf.Max(maxSpeedTenSecond, avg10);
        }

        private void Update()
        {
            if (!isScrolling && currentScrollSpeed != 0)
            {
                currentScrollSpeed = Mathf.MoveTowards(currentScrollSpeed, 0, deceleration * Time.deltaTime);

                Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
                newScrollPosition.y += currentScrollSpeed / 1.36f;
                newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
                scrollableList.content.anchoredPosition = newScrollPosition;
            }

            if (gameManager.SelectedItem != previousSelectedItem)
            {
                StartCoroutine(WaitBeforeReset());
            }

            // Remove old entries > 10 seconds
            while (speedHistory.Count > 0 && Time.time - speedHistory.Peek().time > 10f)
                speedHistory.Dequeue();

            // Update averages each frame
            UpdateAverageSpeeds(Time.time);

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
