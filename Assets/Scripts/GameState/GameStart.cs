using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using System;
using Firebase.Auth;
using System.Diagnostics;
using Database_Objects;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class GameStart : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI selectNumber; // Number to be selected by user.
    [SerializeField] TextMeshProUGUI correctText;
    [SerializeField] bool testMode;
    [SerializeField] bool saveData;
    [SerializeField] int selectedNumber;
    [SerializeField] protected ScrollRect scrollableList;
    [SerializeField] AudioSource correctAudioSource;
    [SerializeField] AudioSource incorrectAudioSource;
    protected GameManager gameManager;
    protected FirebaseUpdateGame firebaseGame;
    protected DatabaseReference reference;
    protected FirebaseAuth auth;
    protected Stopwatch stopwatch;
    List<int> numberArray = new(); // Array to hold numbers the user will select, will be shuffled each time
    private float[] arr = new float [50];
    int numberArrayIndex;
    int previousSelectedItem;
    private int previousSelectedNumber;
    int selectedItem;
    int currentBlockId;
    int currentUserId;
    int previousBlockId;
    int previousUserId;
    int previousNumberOfItems;
    private float distanceToItem;
    float pauseTime = 2.2f;
    private float itemHeight = 55f;
    private float itemDistanceInit = (2454.621f/49f);
    private float itemLocation;
    bool isCoroutineRunning; // Flag to indicate if the coroutine is running
    public bool isCorrect;
    public float completionTime;

    public int itemSelected;
    
    // Only 10 values will be read for each trial. But different every time and non-repeating
    void Start()
    {
        saveData = true;
        gameManager = GameManager.instance; //Game manager instance 
        firebaseGame = FirebaseUpdateGame.instance; //Firebase manager instance
         // Initialize Firebase and authenticate the user
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Set the root reference
                reference = FirebaseDatabase.DefaultInstance.RootReference;

                // Initialize Firebase Auth
                auth = FirebaseAuth.DefaultInstance;
                SignInUser();
            }
            else
            {
                Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", task.Result));
            }
        });
        for (int i = 0; i < 50; i++)
        {
            arr[i] = i * itemDistanceInit;
            Debug.Log("arr[" + i + "] = " + arr[i]);
        }
        stopwatch = new Stopwatch(); //Create a stopwatch object for timing
        FillArray(numberArray); // Fill the selection array
        Shuffle(numberArray); // Shuffle the array for selection
        previousNumberOfItems = gameManager.NumberOfItems;
        currentBlockId = firebaseGame.BlockId; // UserId and blockId from superclass
        currentUserId = firebaseGame.UserId;
        previousBlockId = currentBlockId; // Set previous value to have an on change in the update
        previousUserId = currentUserId;
        SetGameStart(); //Start up the game
    }
    void SignInUser()
    {
        //Sign user into the database
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
  
        });
    }
    void Update()
    {
        if (previousBlockId != currentBlockId)
        {
            currentBlockId = firebaseGame.BlockId; //Update block data if changed
            previousBlockId = currentBlockId;
        }
        if (previousUserId != currentUserId)
        {
            currentUserId = firebaseGame.UserId; //Update user data if changed
            previousUserId = currentUserId;
        } //Reset if new data, or list change or different technique selected
        if(firebaseGame.LoadData==true||gameManager.NumberOfItems!=previousNumberOfItems||gameManager.TechniqueNumber!=gameManager.PreviousTechnique||gameManager.AreaNumber!=gameManager.PreviousArea){//Reset game settings, check for update from firebase class
            stopwatch = new Stopwatch();
            ResetGame(); //Resets the game
            SetGameStart(); //Setup for starting the game
        }
        if(!testMode)
            SelectionChange(); //Not in test mode just start the array
        else if(previousSelectedNumber != selectedNumber && !isCoroutineRunning)
        {
            StartCoroutine(TestSelectionChange()); //In test mode, check we are not waiting for a delay and the number has changed
        }
    }
    void ResetGame(){
        previousNumberOfItems = gameManager.NumberOfItems;
        RemoveArray(numberArray); //Reset the array to no elements
        FillArray(numberArray); // Fill the selection array
        Shuffle(numberArray); // Shuffle the array for selection
        numberArrayIndex = 0; //Set array index back to 0
        currentBlockId = firebaseGame.BlockId; // UserId and blockId from superclass
        currentUserId = firebaseGame.UserId;
        previousBlockId = currentBlockId; // Set previous value to have an on change in the update
        previousUserId = currentUserId;
        correctText.text = ""; //Reset correct text
    }

    void FillArray(List<int> array)
    {
        int count = gameManager.NumberOfItems;
        for (int i = 0; i < count; i++)
        {
            array.Add(i + 1); // Use Add method to fill array with numbers in the list, for selection
        }
    }
   void RemoveArray(List<int> array)
    {
        array.Clear(); // Remove all array elements before adding them again
    }

    void Shuffle(List<int> array)
    {   
        //Used to randomize array elements from 1-50
        System.Random random = new System.Random();
        int n = array.Count; // Use Count instead of Capacity
        for (int i = n - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1); // Random index from 0 to i
            // Swap array[i] with the element at random index
            (array[i], array[j]) = (array[j], array[i]);
        } 
        // Trim the list to contain only 10 items after shuffling. 10 items selected for each block
        if (array.Count > 10)
        {
            array.RemoveRange(10, array.Count - 10); //Trim 50 item array to 10
        }
    }

    void SetGameStart()
    {
        selectNumber.text = "Select Any Number to Begin"; //Start text displayed to begin
    }

    private void SelectionChange()
    {   
        selectedItem = gameManager.SelectedItem;
        if (selectedItem != previousSelectedItem)
        {
            previousSelectedItem = selectedItem;
            if (numberArrayIndex > 0) // Only check correctness after the first selection
            {
                CheckCorrect(selectedItem); //Check if selection is correct 
            }
            SetNumber(); //Set next item
        }
    }
    //This only used for test mode, when I'm too lazy to use the VR headset
    IEnumerator TestSelectionChange()
    {
        isCoroutineRunning = true; // Set the flag to indicate the coroutine is running
        yield return new WaitForSeconds(pauseTime); // Add a delay before checking correctness
        if (numberArrayIndex > 0) // Only check correctness after the first selection
        {   
            CheckCorrect(selectedNumber); // Using selectedNumber to check correctness
        }
        SetNumber(); //Set next number
        isCoroutineRunning = false; // Reset the flag when the coroutine completes
        previousSelectedNumber = selectedNumber; // Update previousSelectedNumber before starting the coroutine
    }

    void SetNumber()
    {
        if (numberArrayIndex < numberArray.Count)
        {
            if (numberArrayIndex > 0)
            {
                // Debug.Log(numberArray[numberArrayIndex - 1]);
                // Debug.Log("Actual Position " + scrollableList.content.anchoredPosition.y);
                // Debug.Log("Array Number " + arr[numberArray[numberArrayIndex-1]-1]);
                itemLocation = arr[numberArray[numberArrayIndex - 1] - 1];
                distanceToItem =  scrollableList.content.anchoredPosition.y - itemLocation;
                // Debug.Log(distanceToItem);
            }

            //When items are left to select
            stopwatch.Start(); //Start time
            //Display item to be selected
            selectNumber.text = "Item #" + (numberArrayIndex + 1) + ", Please Select: " + numberArray[numberArrayIndex].ToString(); // Set the number the user will be retrieving
            numberArrayIndex++; //Update the array index to select next item
        }
        else
        {   //Once all 10 items have been selected
            selectNumber.text = "No more items to select.";
        }
    }
    //Check if answer is correct
   void CheckCorrect(int selectedItem)
   {
       gameManager.SelectedItem = selectedItem;
        stopwatch.Stop(); //Stop the timer
        TimeSpan elapsedTime = stopwatch.Elapsed; //Save elapsed time
        stopwatch.Reset(); //Reset stopwatch to 0
        //Get completion time as a float
        completionTime = (float)elapsedTime.TotalSeconds;
        if (selectedItem == numberArray[numberArrayIndex - 1]) //Used to check if answer is correct
        {
            //Answer is correct, set correct to true. Play sound and display green text
            isCorrect = true;
            correctText.text = "Correct";
            correctText.color = Color.green; // Set color to green for correct
            correctAudioSource.Play(); // Play the correct audio clip
        }
        else
        {
            //Answer is incorrect. Play incorrect noise, set isCorrect to false and display red text
            isCorrect = false;
            correctText.text = "Incorrect";
            correctText.color = Color.red; // Set color to red for incorrect
            incorrectAudioSource.Play(); // Play the incorrect audio clip
        }
        SetTrialData();
    }
    // ReSharper disable Unity.PerformanceAnalysis
    protected void SetTrialData()
    {
        // Query trial data based on the user ID
        string userIdStr = firebaseGame.UserId.ToString();
        reference.Child("Game").Child("Trials").Child("User" + userIdStr).OrderByKey().LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // Get the last trialId
                int lastTrialId = GetLastTrialId(snapshot);
                //Insert correct answer bool and time to select answer in Firebase
                bool correctSelection = isCorrect;
                float timeToComplete = completionTime;
                if(saveData){
                // Insert a new trial with the incremented trialId
                //InsertTrial(new Trial(firebaseGame.UserId, firebaseGame.BlockId, lastTrialId + 1, timeToComplete, correctSelection, gameManager.AreaNumber, gameManager.TechniqueNumber, gameManager.SelectedItem, numberArray[numberArrayIndex-2], itemLocation, distanceToItem));
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve trials: " + task.Exception);
            }
        });
    }

    int GetLastTrialId(DataSnapshot snapshot)
    {
        int lastTrialId = 0; // Default value if no trials exist

        foreach (DataSnapshot childSnapshot in snapshot.Children)
        {
            // Parse trialId from the child key
            if (int.TryParse(childSnapshot.Key, out int trialId))
            {   
                //Set new trial ID based on last one found
                if (trialId > lastTrialId)
                {
                    lastTrialId = trialId;
                }
            }
        }

        return lastTrialId;
    }

    void InsertTrial(Trial trial)
    {
        // Convert blockId, trialId, and userId to strings to use them as key
        string trialIdStr = trial.TrialId.ToString();
        string userIdStr = trial.UserId.ToString();

        // Form the reference path for inserting the trial data
        DatabaseReference trialReference = reference.Child("Game").Child("Trials").Child("User" + userIdStr).Child(trialIdStr);

        // Insert the trial data into the firebase
        trialReference.SetRawJsonValueAsync(JsonUtility.ToJson(trial)).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Trial data inserted successfully.");
            }
            else
            {
                Debug.LogError("Failed to insert trial data: " + task.Exception);
            }
        });
    }
}
