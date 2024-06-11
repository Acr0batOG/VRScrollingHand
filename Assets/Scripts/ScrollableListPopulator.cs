using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Oculus.Interaction.Body.Input;
using Unity.VisualScripting;

public class ScrollableListPopulator : MonoBehaviour
{
    [SerializeField] private GameObject listItemPrefab; // Reference to the prefab for list items
     private int numberOfItems; // Number of items to populate
     int previousNumberOfItems;
     private float listStartOffset = .0002f;
    [SerializeField] private Transform content; // Reference to the Content object in the ScrollView
    [SerializeField] private ScrollRect scrollRect; // Reference to the ScrollRect component
    GameManager gameManager;
    private float totalHeight;

    void Start()
    {   
        gameManager = GameManager.instance;
        numberOfItems = gameManager.NumberOfItems; //Get number of items
        previousNumberOfItems = numberOfItems;
        PopulateList();  //Populate the list
        SetScrollPositionToMidpoint();
    }
    
    void Update(){
        numberOfItems = gameManager.NumberOfItems;
        if(previousNumberOfItems!=numberOfItems){ //If change in the number of items reset list
        RemoveListItems(); 
        PopulateList();
        SetScrollPositionToMidpoint();
        previousNumberOfItems = numberOfItems;}
    }

    private void PopulateList()
    {
        //Add the items  to the list
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
        
        }

        // Adjust the size of the content to fit all items
        RectTransform contentRect = content.GetComponent<RectTransform>();
        totalHeight = numberOfItems * 50 + 240; //Linear formula to align the list
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

   public void SetScrollPositionToMidpoint()
    {
        // Get the total height of the content
        float contentHeight = totalHeight;

        // Get the height of the viewport
        float viewportHeight = scrollRect.viewport.rect.height;

        // Calculate the midpoint position in the content
        float midpointPosition = contentHeight / 2.0f;

        // Adjust for the viewport height to find the correct position
        float targetPosition = midpointPosition - (viewportHeight / 2.0f);

        // Normalize the target position to a value between 0 and 1
        float normalizedPosition = 1.0f - (targetPosition / (contentHeight - viewportHeight)) + (numberOfItems*listStartOffset);

        // Clamp the normalized position between 0 and 1
        normalizedPosition = Mathf.Clamp(normalizedPosition, 0.0f, 1.0f);

        // Set the vertical normalized position
        scrollRect.verticalNormalizedPosition = normalizedPosition;
    }
    private void RemoveListItems(){
        foreach (Transform child in content)
        {
            //Remove all items from the list
            Destroy(child.gameObject);
        }
    }
}
