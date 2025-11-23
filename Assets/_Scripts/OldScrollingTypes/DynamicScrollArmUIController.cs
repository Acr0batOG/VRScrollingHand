using System.Collections;
using System.Collections.Generic;
using _Scripts.Calculators;
using System;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class DynamicScrollArmUIController : ArmUIController
    {
        private float scrollSpeed = 550f;
        private Vector3 lastContactPoint = Vector3.zero;
        private float slowMovementThreshold = .001f;

        private float contentHeight;
        private float viewportHeight;

        private Queue<(float time, float speedMS)> speedHistory = new Queue<(float, float)>();  // NEW: stores m/s values

        // NEW: real-world speed metrics in m/s
        protected float instantSpeedMS = 0f;
        protected float maxSpeedSecond = 0f;     // your request
        protected float maxSpeedTenSecond = 0f;  // your request
        protected float absoluteMaxSpeedMS = 0f; // max instantaneous m/s

        private float lastSpeedTime = 0f;        // NEW: track delta time between samples

        // Inertia
        private float currentScrollSpeed;
        private float deceleration = 75f; 
        private bool isScrolling;

        protected new void Start()
        {
            base.Start();
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;
            trialStartTime = Time.time;
            previousSelectedItem = gameManager.SelectedItem;

            lastSpeedTime = Time.time;
        }

        protected void OnTriggerEnter(Collider other)
        {
            contentHeight = scrollableList.content.sizeDelta.y;
            viewportHeight = scrollableList.viewport.rect.height;

            if (other.gameObject.name == "Other Fingertip")
            {
                Vector3 currentContactPoint = other.ClosestPoint(startPoint.position);
                menuText.text = "Enter";

                float normalisedLandingPoint = ArmPositionCalculator.GetNormalisedPositionOnArm(
                    endPoint.position, startPoint.position, currentContactPoint);

                gameManager.NormalisedLandingPoint = normalisedLandingPoint;

                lastContactPoint = currentContactPoint;

                timeBetweenSwipes = Time.time - lastSwipeTime;
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

                Debug.Log("Max Instant m/s: " + absoluteMaxSpeedMS);
                Debug.Log("Max 1s Avg m/s: " + maxSpeedSecond);
                Debug.Log("Max 10s Avg m/s: " + maxSpeedTenSecond);
            }
        }

        protected override void Scroll(Collider colliderInfo)
        {
            Vector3 currentContactPoint = colliderInfo.ClosestPoint(startPoint.position);
            float movement = Vector3.Distance(lastContactPoint, currentContactPoint);

            // Compute real instant speed in m/s
            float now = Time.time;
            float dt = now - lastSpeedTime;

            if (dt > 0.0001f)
            {
                instantSpeedMS = movement / dt;  // NEW: m/s

                if (instantSpeedMS > absoluteMaxSpeedMS)
                    absoluteMaxSpeedMS = instantSpeedMS;

                // Store speed for 1s + 10s windows
                speedHistory.Enqueue((now, instantSpeedMS));
            }

            lastSpeedTime = now;

            // Clean history older than 10 seconds
            while (speedHistory.Count > 0 && now - speedHistory.Peek().time > 10f)
                speedHistory.Dequeue();

            UpdateAverageSpeeds(now);

            // Existing scroll-based UI behaviour
            if (movement < slowMovementThreshold)
            {
                lastContactPoint = currentContactPoint;
                return;
            }

            float normalizedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(
                endPoint.position, startPoint.position, currentContactPoint);

            float previousNormalizedPosition = ArmPositionCalculator.GetNormalisedPositionOnArm(
                endPoint.position, startPoint.position, lastContactPoint);

            currentScrollSpeed = (normalizedPosition - previousNormalizedPosition) * scrollSpeed;

            Vector2 newPos = scrollableList.content.anchoredPosition;
            newPos.y += currentScrollSpeed;
            newPos.y = Mathf.Clamp(newPos.y, 0, contentHeight - viewportHeight);

            scrollableList.content.anchoredPosition = newPos;

            totalAmplitudeOfSwipe += movement;

            lastContactPoint = currentContactPoint;
        }

        private void UpdateAverageSpeeds(float now)
        {
            float sum1 = 0f;
            float sum10 = 0f;
            int count1 = 0;
            int count10 = speedHistory.Count;

            foreach (var (t, s) in speedHistory)
            {
                sum10 += s;
                if (now - t <= 1f)
                {
                    sum1 += s;
                    count1++;
                }
            }

            float avg1 = count1 == 0 ? 0f : sum1 / count1;
            float avg10 = count10 == 0 ? 0f : sum10 / count10;

            if (avg1 > maxSpeedSecond)
                maxSpeedSecond = avg1;

            if (avg10 > maxSpeedTenSecond)
                maxSpeedTenSecond = avg10;
        }

        private void Update()
        {
            // inertia code remains unchanged...

            if (gameManager.SelectedItem != previousSelectedItem)
                StartCoroutine(WaitBeforeReset());

            previousSelectedItem = gameManager.SelectedItem;
        }

        IEnumerator WaitBeforeReset()
        {
            yield return new WaitForSeconds(.1f);

            timeBetweenSwipesArray.Clear();
            numberOfFlicks = 0;
            totalAmplitudeOfSwipe = 0f;

            // Reset speeds
            instantSpeedMS = 0f;
            absoluteMaxSpeedMS = 0f;
            maxSpeedSecond = 0f;
            maxSpeedTenSecond = 0f;
            speedHistory.Clear();
        }
    }
}
