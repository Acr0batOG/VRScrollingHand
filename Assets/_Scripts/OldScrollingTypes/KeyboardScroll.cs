using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace _Scripts.OldScrollingTypes
{
    public class KeyboardScroll : ArmUIController
    {
        private float scrollSpeed = 650f;  // Adjust the speed of scrolling
        // 365 is ideal for comparable speed

        private float contentHeight;
        private float viewportHeight;

        protected new void Start()
        {
            contentHeight = scrollableList.content.rect.height;
            viewportHeight = scrollableList.viewport.rect.height;
        }

        void Update()
        {
            contentHeight = scrollableList.content.rect.height;
            viewportHeight = scrollableList.viewport.rect.height;
            float verticalInput = 0f;
                    
            if (Input.GetKey(KeyCode.UpArrow))
            {
                verticalInput = 1f; // Scroll up on arrow up press
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                verticalInput = -1f; // Scroll down on arrow down press
            }

            // Calculate the new scroll position based on the joystick input
            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y -= verticalInput * scrollSpeed * Time.deltaTime;

            // Clamp the new scroll position to ensure it stays within the scrollable area
            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

            // Apply the new scroll position
            scrollableList.content.anchoredPosition = newScrollPosition;
              
        }
    }
}