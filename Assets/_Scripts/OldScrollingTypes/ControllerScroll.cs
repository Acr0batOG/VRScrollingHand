using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.OldScrollingTypes
{
    public class ControllerScroll : ArmUIController
    {
        // Start is called before the first frame update
        [SerializeField] private float scrollSpeed = 100f;  // Adjust the speed of scrolling

        private float contentHeight;
        private float viewportHeight;

        protected new void Start()
        {
            contentHeight = scrollableList.content.rect.height;
            viewportHeight = scrollableList.viewport.rect.height;
        }

        void Update()
        {
            // Get the vertical input from the right joystick
            float verticalInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y;
            // Check if the A button is pressed
            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                SelectItem();
            }
            // Calculate the new scroll position based on the joystick input
            Vector2 newScrollPosition = scrollableList.content.anchoredPosition;
            newScrollPosition.y += verticalInput * scrollSpeed * Time.deltaTime; // Multiply by Time.deltaTime for frame-rate independence

            // Clamp the new scroll position to ensure it stays within the scrollable area
            newScrollPosition.y = Mathf.Clamp(newScrollPosition.y, 0, contentHeight - viewportHeight);

            // Apply the new scroll position
            scrollableList.content.anchoredPosition = newScrollPosition;
        }
    }
}
