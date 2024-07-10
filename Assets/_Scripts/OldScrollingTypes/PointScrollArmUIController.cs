using System.Globalization;
using UnityEngine;

namespace _Scripts.OldScrollingTypes
{
    public class PointScrollArmUIController : ArmUIController
    {
        protected float UserPointHeight; // Variable to hold user's height

        // Constants for offset percentages and divisors
        protected float StartOffsetPercentage = 0.22f; //Default offset position
        protected readonly float StartOffsetChange = 0.04f; //Default offset position
        protected float EndOffsetPercentage = 1.22f; // End of arm, used for 11 inch forearms. Will be replaced in GameManager
        protected readonly float ARMDivisor = 1.85f; // Used to convert user's arm length to the ending point on their arm
        protected readonly float HandDivisor = 2.0f; // Used to convert user's hand length from their arm length to the ending point on their hand
        protected readonly float FingerDivisor = 2.30f; // Used to convert user's finger length
        protected const float FingertipDivisor = 2.6f; // Used to convert user's fingertip length
        protected readonly float HandDivisorAdjustment = .08f;
        // protected readonly float ARMDivisorAdjustment =.05f;
        // protected readonly float imagesizeMultiplier = 1.36f;
        // protected readonly float fontsizeMultiplier = 1.1f;
        // private Coroutine hoverCoroutine = null;
        // private int hoveredBinIndex = -1;
        // private bool isExpanded = false;
        // Start is called before the first frame update
        protected new void Start()
        {
            base.Start();
            LengthCheck(); // Check arm length  
        }

        private void OnTriggerEnter(Collider other)
        {
            LengthCheck(); // Check arm length
            menuText.text = "Enter"; // Update menu text
            Scroll(other); // Scroll through the content
            DwellCoroutine ??= StartCoroutine(DwellSelection());
        }

        private void OnTriggerStay(Collider other)
        {
            Scroll(other); // Scroll through the content
            // Restart dwell selection coroutine if list position changes significantly
            if (DwellCoroutine != null && Mathf.Abs(scrollableList.content.anchoredPosition.y - PreviousScrollPosition) > DwellThreshold)
            {
                StopCoroutine(DwellCoroutine); //If too much movement, reset dwell Selection
                DwellCoroutine = StartCoroutine(DwellSelection());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            menuText.text = "Exit"; // Update menu text
            // Stop dwell selection coroutine on exit
            if (DwellCoroutine != null)
            {
                StopCoroutine(DwellCoroutine); //Reset dwell selection on exit
                DwellCoroutine = null;
            }
        }

        protected override void Scroll(Collider fingerCollider)
        {
            Vector3 contactPoint = fingerCollider.ClosestPoint(startPoint.position); // Get the closest contact point

            // Calculate content and viewport height
            float contentHeight = scrollableList.content.sizeDelta.y;
            float viewportHeight = scrollableList.viewport.rect.height;

            int totalBins = GameManager.NumberOfItems; // Total number of bins for scrolling

            // Calculate arm length and offsets
            float length = (endPoint.position - startPoint.position).magnitude;
            float startOffset = StartOffsetPercentage * length;
            float endOffset = EndOffsetPercentage * length;

            // Calculate contact and adjusted contact positions
            float contactPosition = (contactPoint - startPoint.position).magnitude;
            float adjustedContactPosition = Mathf.Clamp(contactPosition - startOffset, 0, endOffset - startOffset);

            // Calculate bin index based on adjusted contact position
            int binIndex = Mathf.Clamp(Mathf.RoundToInt((1 - (adjustedContactPosition / (endOffset - startOffset))) * (totalBins - 1)), 0, totalBins - 1) + 1;
            // if (hoveredBinIndex != binIndex)
            // {
            //     hoveredBinIndex = binIndex;
            //
            //     // Stop any existing hover coroutine
            //     if (hoverCoroutine != null)
            //     {
            //         StopCoroutine(hoverCoroutine);
            //         hoverCoroutine = null;
            //         ResetBinHeight(binIndex);
            //     }
            //
            //     // Start a new hover coroutine
            //     hoverCoroutine = StartCoroutine(HoverOverBin(binIndex));
            // }
            // Calculate bin height and new scroll position
            float binHeight = (contentHeight - viewportHeight) / (totalBins - 1);
            float newScrollPositionY = (binIndex - 1) * binHeight;

            // Set the new scroll position
            Vector2 newScrollPosition = new Vector2(scrollableList.content.anchoredPosition.x, newScrollPositionY);
            scrollableList.content.anchoredPosition = newScrollPosition;

            // Update distance text
            distText.text = "Point Scroll: Position " + contactPoint.ToString() + " " + newScrollPosition.y.ToString(CultureInfo.InvariantCulture) + " " + EndOffsetPercentage + " " + capsuleCollider.GetComponent<CapsuleCollider>().height;
        }

        // Check arm length and adjust offsets accordingly
        protected void LengthCheck()
        {
            UserPointHeight = GameManager.UserHeight; // Get height from GameManager
            int lengthCheckAreaNum = GameManager.AreaNumber; // Check the area number

            switch(lengthCheckAreaNum){
                case 1: 
                    EndOffsetPercentage = UserPointHeight / ARMDivisor -.07f; //Arm being used for scrolling, different size
                    StartOffsetPercentage = 0.25f;
                    break;
                case 2:
                    EndOffsetPercentage = UserHeight / HandDivisor - HandDivisorAdjustment; //Different divisor to set hand size for users
                    //.22
                    break;
                case 3:
                    EndOffsetPercentage = UserHeight / FingerDivisor;  //Test this
                    StartOffsetPercentage = .32f;
                    break;
                case 4:
                    EndOffsetPercentage = UserHeight /FingertipDivisor ; //appx.85 with 2.2 arm length
                    StartOffsetPercentage = .14f;
                    break;
            }
        }
        // private IEnumerator HoverOverBin(int binIndex)
        // {
        //     // Wait for 500ms
        //     yield return new WaitForSeconds(0.5f);
        //
        //     // Expand the bin height
        //     ExpandBinHeight(binIndex);
        // }
        
        // private void ExpandBinHeight(int binIndex)
        // {
        //     // Logic to expand the bin height
        //     if (!isExpanded)
        //     {
        //         binIndex = Mathf.Clamp(binIndex, 0, 49);
        //         
        //         // Find the bin using binIndex and modify its height
        //         var image = scrollableList.content.GetChild(binIndex).GetComponentsInChildren<RectTransform>();
        //         foreach (RectTransform img in image)
        //         {
        //             img.sizeDelta = new Vector2(img.sizeDelta.x, img.sizeDelta.y * imagesizeMultiplier); // Increase height by 50% 
        //         }
        //         var text = scrollableList.content.GetChild(binIndex).GetComponentInChildren<TextMeshProUGUI>(); 
        //         text.fontSize = Mathf.RoundToInt(text.fontSize * fontsizeMultiplier); // Increase font size by 50%
        //
        //         isExpanded = true;
        //         // Expand the array selector values as well
        //         contentHeight = ItemHeight * (ItemCount - 1) + ItemHeight * imagesizeMultiplier;
        //         //55*49 + 55 * 1.36 = new total content height
        //         scrollableList.content.sizeDelta = new Vector2(scrollableList.content.sizeDelta.x,
        //             scrollableList.content.sizeDelta.y + 18f);
        //     }
        // }
        //
        // private void ResetBinHeight(int binIndex)
        // {
        //     if (isExpanded)
        //     {
        //         // Find the bin using binIndex and modify its height
        //         var image = scrollableList.content.GetChild(binIndex).GetComponentsInChildren<RectTransform>();
        //         foreach (RectTransform img in image)
        //         {
        //             img.sizeDelta = new Vector2(img.sizeDelta.x, img.sizeDelta.y / imagesizeMultiplier); // Increase height by 50% 
        //         }
        //         var text = scrollableList.content.GetChild(binIndex).GetComponentInChildren<TextMeshProUGUI>(); 
        //         
        //         text.fontSize = Mathf.RoundToInt(text.fontSize / fontsizeMultiplier); // Increase font size by 50%
        //
        //         isExpanded = false;
        //         //Set content height back to normal
        //         contentHeight = ItemHeight * ItemCount;
        //         scrollableList.content.sizeDelta = new Vector2(scrollableList.content.sizeDelta.x,
        //             scrollableList.content.sizeDelta.y - 18f);
        //     }
        // }
    }
}
