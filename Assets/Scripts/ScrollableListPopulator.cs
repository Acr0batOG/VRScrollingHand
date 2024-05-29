using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ScrollableListPopulator : MonoBehaviour
{
    [SerializeField] private GameObject listItemPrefab; // Reference to the prefab for list items
    private int numberOfItems = 50; // Number of items to populate
    [SerializeField] private Transform content; // Reference to the Content object in the ScrollView
    private float startX = 337f; // Starting X position
     private float startY = 0f; // Starting Y position
    private float yOffset = 55f; // Y position offset between items
    float[] floatArray = new float[101];
    

    void Start()
    {
        startY = (numberOfItems + 1) * yOffset + 30;
        PopulateList();
        //common list size multiplier Fix tomorrow
        floatArray[5] = 1.64f;
        floatArray[10] = 1.32f;
        floatArray[20] = 1.16f;
        floatArray[25] = 1.135f;
        floatArray[50] = 1.067f;
        floatArray[100] = 1.033f;
    }

    private void PopulateList()
{
    float totalHeight = 0f; // To calculate total height of the content

    for (int i = 0; i < numberOfItems; i++)
    {
        // Instantiate a new list item and set its parent to the content transform
        GameObject listItem = Instantiate(listItemPrefab, content);

        // Set the name of the instantiated item to a unique name
        listItem.name = "ListItem_" + (i + 1).ToString();

        // Find the Image component within the instantiated prefab
        Image listItemImage = listItem.GetComponent<Image>();
        if (listItemImage != null)
        {
            // Find the TextMeshProUGUI component within the children of the Image component
            TextMeshProUGUI listItemText = listItemImage.GetComponentInChildren<TextMeshProUGUI>();
            if (listItemText != null)
            {
                listItemText.text = (i + 1).ToString();
            }
            else
            { 
                Debug.LogError("TextMeshProUGUI component not found in list item prefab.");
            }
        }
        else
        {
            Debug.LogError("Image component not found in list item prefab.");
        }
        
        // Manually set the position of each item
        RectTransform listItemRect = listItem.GetComponent<RectTransform>();
        float newY = startY + (-i * yOffset);
        listItemRect.anchoredPosition = new Vector2(startX, newY);
        
        // Update the total height needed for the content
        totalHeight += yOffset*1.067f;//floatArray[numberOfItems-1]; // Add the yOffset to totalHeight for each item
        
    }

    // Adjust the size of the content to fit all items
    RectTransform contentRect = content.GetComponent<RectTransform>();
    totalHeight += yOffset; // Add extra yOffset for the bottom padding
    contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }
    // Function to calculate the value of the polynomial equation
    

}
