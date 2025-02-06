using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace _Scripts.OldScrollingTypes
{
    public class ControllerScroll : ArmUIController
    {
        [SerializeField] private float scrollSpeed = 750f;  // Adjust the speed of scrolling

        private float contentHeight;
        private float viewportHeight;

        [SerializeField] private XRController xrController;

        private bool isJoystickPushed = false; // Track joystick movement state

        protected new void Start()
        {
            contentHeight = scrollableList.content.rect.height;
            viewportHeight = scrollableList.viewport.rect.height;
            trialStartTime = Time.time;
        }

        void Update()
        {
            contentHeight = scrollableList.content.rect.height;
            viewportHeight = scrollableList.viewport.rect.height;

            if (xrController != null && xrController.inputDevice.isValid)
            {
                if (xrController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickInput))
                {
                    float verticalInput = thumbstickInput.y;

                    // Check if joystick was previously pushed but now is back to zero (flick detected)
                    if (isJoystickPushed && Mathf.Approximately(verticalInput, 0f))
                    {
                        numberOfFlicks++;  // Count the flick
                        isJoystickPushed = false; // Reset state
                    }

                    // If joystick is pushed, accumulate swipe amplitude and mark state
                    if (!Mathf.Approximately(verticalInput, 0f))
                    {
                        totalAmplitudeOfSwipe += Mathf.Abs(verticalInput);
                        isJoystickPushed = true;
                    }

                    // Scroll content
                    Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
                    newScrollPosition.y -= verticalInput * scrollSpeed * Time.deltaTime;
                    newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);
                    scrollableList.content.anchoredPosition = newScrollPosition;
                }
            }
            else
            {
                Debug.LogWarning("XRController is not assigned or inputDevice is not valid.");
            }
        }
    }
}
