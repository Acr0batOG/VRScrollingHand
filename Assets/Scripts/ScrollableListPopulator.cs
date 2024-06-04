using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Oculus.Interaction.Body.Input;
using Unity.VisualScripting;

public class ScrollableListPopulator : MonoBehaviour
{   
    //Just Ignore most things in this file as it has been fixed in a different branch
    [SerializeField] private GameObject listItemPrefab; // Reference to the prefab for list items
     private int numberOfItems; // Number of items to populate
     int previousNumberOfItems;
    [SerializeField] private Transform content; // Reference to the Content object in the ScrollView
    [SerializeField] private ScrollRect scrollRect; // Reference to the ScrollRect component
    GameManager gameManager;
    private float additionValue = 1.05f;
    private float startX = 337f; // Starting X position
    private float startY = 0f; // Starting Y position
    private float yOffset = 55f; // Y position offset between items

    void Start()
    {   
        gameManager = GameManager.instance;
        numberOfItems = gameManager.NumberOfItems;
        previousNumberOfItems = numberOfItems;
        startY = (numberOfItems + 1) * yOffset + 35;
        SetAdditionValue();
        PopulateList();
        SetScrollPositionToMidpoint();
        
    }
    
    void Update(){
        numberOfItems = gameManager.NumberOfItems;
        if(previousNumberOfItems!=numberOfItems){
        startY = (numberOfItems + 1) * yOffset + 35;
        RemoveListItems();
        SetAdditionValue();
        PopulateList();
        SetScrollPositionToMidpoint();
        previousNumberOfItems = numberOfItems;}
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
            float offsetMultiply = (float)(Math.Exp(-(numberOfItems / 8.0f)) + additionValue); // Formula to calculate the position of the list
            Debug.Log(offsetMultiply);
            // Update the total height needed for the content
            totalHeight += yOffset * offsetMultiply; // Add the yOffset to totalHeight for each item
        }

        // Adjust the size of the content to fit all items
        RectTransform contentRect = content.GetComponent<RectTransform>();
        totalHeight += yOffset; // Add extra yOffset for the bottom padding
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
    }

    private void SetScrollPositionToMidpoint()
    {
        // Calculate the midpoint normalized position
        float midpointNormalizedPosition = 0.5f; // Midpoint in normalized scroll position
        scrollRect.verticalNormalizedPosition = midpointNormalizedPosition;
    }
    private void RemoveListItems(){
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
    }
    private void SetAdditionValue(){ //Setting the length of each item so the list of numberOfItems ends at 1.
        //Just a mess really, not what I hoped for but it's been fixed in the next branch
        switch(numberOfItems/5){
            case 1:
                additionValue = 1.10475f; //5
                break;
            case 2:
                additionValue = 1.0335f; //10
                break;
            case 4:
                additionValue = 1.07792f; //20
                break;
            case 5:
                additionValue = 1.0885f; //25
                break;
            case 10:
                additionValue = 1.0637f; //50
                break;
            case 15:
                additionValue = 1.0423f; //75
                break;
            case 20:
                additionValue = 1.0315f; //100
                break;
            default:
                additionValue = 1.0625f; //default
                break;

        }
    }
}
