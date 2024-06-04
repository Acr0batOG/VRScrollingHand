using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;


public class GameStart : FirebaseUpdateGame
{
    //Used to load a random number the user will select in the list
    [SerializeField] TextMeshPro selectNumber; // Number to be selected by user.
    int[] numberArray = {}; // Array to hold numbers the user will select, will be shuffled each time
    int numberArrayIndex = 0;
    int currentBlockId;
    int currentUserId;
    int previousBlockId;
    int previousUserId;
    // Only 10 values will be read for each trial. But different every time and non-repeating
  void Start()
    {
        InitArray(numberArray);
        Shuffle(numberArray);

        currentBlockId = blockId; //UserId and blockId from superclass
        currentUserId = userId;
        previousBlockId = currentBlockId; //Set previous value to have an on change in the update
        previousUserId = currentUserId;
        SetNumber();
        

    }
    void Update(){
        if(previousBlockId!=currentBlockId){
            currentBlockId = blockId;
            previousBlockId = currentBlockId;
        }
        if(previousUserId!=currentUserId){
            currentUserId = userId;
            previousUserId = currentUserId;
        }
    }
    void InitArray(int[] array){
        int count = gameManager.NumberOfItems;
        for(int i = 0; i < count; i++){
            array[i] = i+1; //Load array with each number in the list for selection
        }
    }

    void Shuffle(int[] array)
    {
        System.Random random = new System.Random();
        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1); // Random index from 0 to i
            // Swap array[i] with the element at random index
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
    void SetNumber(){
        selectNumber.text = numberArray[numberArrayIndex].ToString(); //Set the number the user will be retrieving
        numberArrayIndex++;
    }

}
