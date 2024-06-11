using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System;
using System.Threading.Tasks;

public class GameStart : FirebaseUpdateGame
{
    [SerializeField] TextMeshProUGUI selectNumber; // Number to be selected by user.
    [SerializeField] TextMeshProUGUI correctText;
    List<int> numberArray = new(); // Array to hold numbers the user will select, will be shuffled each time
    int numberArrayIndex = 0;
    int previousSelectedItem = 0;
    int selectedItem;
    int currentBlockId;
    int currentUserId;
    int previousBlockId;
    int previousUserId;
    public bool startGame = false;
    // Only 10 values will be read for each trial. But different every time and non-repeating
    void Start()
    {
        gameManager = GameManager.instance;
        FillArray(numberArray); // Fill the selection array
        Shuffle(numberArray); // Shuffle the array for selection

        currentBlockId = blockId; // UserId and blockId from superclass
        currentUserId = userId;
        previousBlockId = currentBlockId; // Set previous value to have an on change in the update
        previousUserId = currentUserId;
        SetGameStart();
    }

    void Update()
    {
        if (previousBlockId != currentBlockId)
        {
            currentBlockId = blockId;
            previousBlockId = currentBlockId;
        }
        if (previousUserId != currentUserId)
        {
            currentUserId = userId;
            previousUserId = currentUserId;
        }
        SelectionChange();
    }

    void FillArray(List<int> array)
    {
        int count = gameManager.NumberOfItems;
        for (int i = 0; i < count; i++)
        {
            array.Add(i + 1); // Use Add method to fill array with numbers in the list, for selection
        }
    }

    void Shuffle(List<int> array)
    {
        System.Random random = new System.Random();
        int n = array.Count; // Use Count instead of Capacity
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1); // Random index from 0 to i
            // Swap array[i] with the element at random index
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    void SetGameStart()
    {
        selectNumber.text = "Select Any Number to Begin";
    }

    void SelectionChange()
    {
        selectedItem = gameManager.SelectedItem;
        Debug.Log("Selected item: " + selectedItem);
        if (selectedItem != previousSelectedItem)
        {
            previousSelectedItem = selectedItem;
            if (numberArrayIndex > 0) // Only check correctness after the first selection
            {
                CheckCorrect();
            }
            SetNumber();
        }
    }

    void SetNumber()
    {
        if (numberArrayIndex < numberArray.Count)
        {
            selectNumber.text = "Item #" + (numberArrayIndex + 1) + ", Please Select: " + numberArray[numberArrayIndex].ToString(); // Set the number the user will be retrieving
            numberArrayIndex++;
        }
        else
        {
            selectNumber.text = "No more items to select.";
        }
    }

    void CheckCorrect()
    {
        // Check if the selected item matches the current item in the array
        //Debug.Log(selectedItem + " == " + numberArray[numberArrayIndex - 1]);
        if (selectedItem == numberArray[numberArrayIndex - 1])
        {
            correctText.text = "Correct";
        }
        else
        {
            correctText.text = "Incorrect";
        }
    }
}
