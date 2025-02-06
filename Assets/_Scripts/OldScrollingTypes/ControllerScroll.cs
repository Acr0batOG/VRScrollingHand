using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace _Scripts.OldScrollingTypes
{
    public class ControllerScroll : ArmUIController
    {
        [SerializeField] private float scrollSpeed = 750f;  // Adjust the speed of scrolling
        // 365 is ideal for comparable speed

        private float contentHeight;
        private float viewportHeight;

        // Reference to the XR Controller component
        [SerializeField] private XRController xrController;

        protected new void Start()
        {
            contentHeight = scrollableList.content.rect.height;
            viewportHeight = scrollableList.viewport.rect.height;
            previousSelectedItem = gameManager.SelectedItem;
        }

        void Update()
        {
            contentHeight = scrollableList.content.rect.height;
            viewportHeight = scrollableList.viewport.rect.height;
            // Ensure the XRController is assigned and the inputDevice is valid
            if (xrController != null && xrController.inputDevice.isValid)
            {
                // Try to get the primary 2D axis (thumbstick) value
                if (xrController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstickInput))
                {
                    // Get the vertical input from the thumbstick
                    float verticalInput = thumbstickInput.y; 
                    if (Mathf.Abs(verticalInput) > 0.1f)
                    {
                        numberOfFlicks++;
                        timeBetweenSwipes = Time.time - lastSwipeTime; // Time since the last swipe
                        if(timeBetweenSwipes < 2.0f && timeBetweenSwipes > 0.0f)
                            timeBetweenSwipesArray.Add(timeBetweenSwipes);
                        lastSwipeTime = Time.time;
                        
                        totalAmplitudeOfSwipe += Mathf.Abs(verticalInput);
                    }
               

                    // Calculate the new scroll position based on the joystick input
                    Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
                    newScrollPosition.y -= verticalInput * scrollSpeed * Time.deltaTime;

                    // Clamp the new scroll position to ensure it stays within the scrollable area
                    newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

                    // Apply the new scroll position
                    scrollableList.content.anchoredPosition = newScrollPosition;
                    
                    gameManager.NumberOfFlicks = numberOfFlicks;
                    gameManager.TimeBetweenSwipesArray = timeBetweenSwipesArray;
                }
            }
            else
            {
                Debug.LogWarning("XRController is not assigned or inputDevice is not valid.");
            }
            
            
            if (gameManager.SelectedItem != previousSelectedItem)
            {
                StartCoroutine(WaitBeforeReset());
                
            }

            previousSelectedItem = gameManager.SelectedItem;
        }
        IEnumerator WaitBeforeReset()
        {
            yield return new WaitForSeconds(.1f);
            timeBetweenSwipesArray.Clear();
            numberOfFlicks = 0;
            totalAmplitudeOfSwipe = 0f;
            
        
        }
    }
}
