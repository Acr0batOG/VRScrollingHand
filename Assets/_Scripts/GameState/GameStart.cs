using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using _Scripts.Database_Objects;
using _Scripts.Firebase;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace _Scripts.GameState
{
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
        private GameManager gameManager;
        private FirebaseUpdateGame firebaseGame;
        private DatabaseReference reference;
        private FirebaseAuth auth;
        private Stopwatch stopwatch;
        List<int> numberArray = new(); // Array to hold numbers the user will select, will be shuffled each time
        private float[] distanceArray = new float [50];
        private float[] colorArray = new float [51];
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
        private float distanceTravelled;
        private float previousScrollPosition;
        private float currentScrollPosition;
        float pauseTime = 2.2f;
        private float itemDistanceInit = (2454.621f/49f);
        private float itemLocation;
        bool isCoroutineRunning; // Flag to indicate if the coroutine is running
        public bool isCorrect;
        public float completionTime;
        private Color originalColor1;
        private Color originalColor2;
        private Graphic graphic1;
        private Graphic graphic2;
        private Color highlightColor1 = new Color(0.85f, 0f, 0f); // Highlighted color
        private Color highlightColor2 = new Color(0.9f, 0.9f, 0.9f); // Highlighted color (light gray)

        private int currentColorIndex = -1; // Index of the current highlighted color
        private int previousColorIndex = -1; // Index of the previous highlighted color

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
                    Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
                }
            });
            stopwatch = new Stopwatch(); //Create a stopwatch object for timing
            FillArray(numberArray); // Fill the selection array
            Shuffle(numberArray); // Shuffle the array for selection
            previousNumberOfItems = gameManager.NumberOfItems;
            currentBlockId = firebaseGame.BlockId; // UserId and blockId from superclass
            currentUserId = firebaseGame.UserId;
            previousBlockId = currentBlockId; // Set previous value to have an on change in the update
            previousUserId = currentUserId;
            SetGameStart(); //Start up the game
            StartCoroutine(WaitBeforeGetColor());
            for (int i = 0; i < 50; i++)
            {
                distanceArray[i] = i * itemDistanceInit;
                //Debug.Log("arr[" + i + "] = " + distanceArray[i]);
                colorArray[i] = i * itemDistanceInit - 25f;
            }
            colorArray[50] = 50 * itemDistanceInit - 25f;
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
                   
                }
            });
        }

        IEnumerator WaitBeforeGetColor()
        {
            yield return new WaitForSeconds(1.5f);
            RectTransform[] rect = scrollableList.content.GetChild(0).GetComponentsInChildren<RectTransform>();
            graphic1 = rect[0].GetComponent<Graphic>(); // Border
            originalColor1 = graphic1.color;
            graphic2 = rect[1].GetComponent<Graphic>(); // White interior
            originalColor2 = graphic2.color;
        }
        void Update()
        {   
            float currentPositionY = scrollableList.content.anchoredPosition.y;

            // Find the index of the color range that currentPositionY is within
            for (int i = 0; i < colorArray.Length - 1; i++)
            {
                if (currentPositionY >= colorArray[i] && currentPositionY <= colorArray[i + 1])
                {
                    previousColorIndex = currentColorIndex; // Update previousColorIndex
                    currentColorIndex = i; // Update currentColorIndex
                    break;
                }
            }

            // If currentPositionY is within a different color range, change colors
            if (currentColorIndex != previousColorIndex)
            {
                // Reset previous highlighted item to original colors
                if (previousColorIndex >= 0 && previousColorIndex < scrollableList.content.childCount)
                {
                    ResetColors(previousColorIndex);
                }

                // Highlight current item with new colors
                if (currentColorIndex >= 0 && currentColorIndex < scrollableList.content.childCount)
                {
                    HighlightColors(currentColorIndex);
                }
            }
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
            if(firebaseGame.LoadData||gameManager.NumberOfItems!=previousNumberOfItems||
               gameManager.TechniqueNumber!=gameManager.PreviousTechnique||gameManager.AreaNumber!=gameManager.PreviousArea){//Reset game settings, check for update from firebase class
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
        void HighlightColors(int index)
        {
            RectTransform[] rect = scrollableList.content.GetChild(index).GetComponentsInChildren<RectTransform>();
            graphic1 = rect[0].GetComponent<Graphic>(); // Border
            graphic1.color = highlightColor1;
            graphic2 = rect[1].GetComponent<Graphic>(); // White interior
            graphic2.color = highlightColor2;
        }

        void ResetColors(int index)
        {
            RectTransform[] rect = scrollableList.content.GetChild(index).GetComponentsInChildren<RectTransform>();
            graphic1 = rect[0].GetComponent<Graphic>(); // Border
            graphic1.color = originalColor1;
            graphic2 = rect[1].GetComponent<Graphic>(); // White interior
            graphic2.color = originalColor2;
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
                int j = random.Next(0, i + 1); // Random index from 0 to I
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
            if (firebaseGame.StartGame)
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
                    itemLocation = distanceArray[numberArray[numberArrayIndex - 1] - 1];
                    distanceToItem =  Math.Abs(scrollableList.content.anchoredPosition.y - itemLocation);
                    // Debug.Log(distanceToItem);
                }

                //When items are left to select
                stopwatch.Start(); //Start time
                previousScrollPosition = scrollableList.content.anchoredPosition.y;
                //Display item to be selected
                if (selectedItem == numberArray[numberArrayIndex])
                {
                    (numberArray[numberArrayIndex], numberArray[numberArrayIndex + 1]) = (numberArray[numberArrayIndex + 1], numberArray[numberArrayIndex]);
                }

                selectNumber.text = "Item #" + (numberArrayIndex + 1) + ", Please Select: " + numberArray[numberArrayIndex].ToString(); // Set the number the user will be retrieving
                numberArrayIndex++; //Update the array index to select next item
            }
            else
            {   //Once all 10 items have been selected
                selectNumber.text = "No more items to select.";
            }
        }
        //Check if answer is correct
        void CheckCorrect(int checkedSelectedItem)
        {
            gameManager.SelectedItem = checkedSelectedItem;
            stopwatch.Stop(); //Stop the timer
            TimeSpan elapsedTime = stopwatch.Elapsed; //Save elapsed time
            stopwatch.Reset(); //Reset stopwatch to 0
            //Get completion time as a float
            completionTime = (float)elapsedTime.TotalSeconds;
             RectTransform[] rect = scrollableList.content.GetChild(checkedSelectedItem-1).GetComponentsInChildren<RectTransform>();
             Graphic selectGraphic1 = rect[0].GetComponent<Graphic>(); //border
             selectGraphic1.color = new Color(1f, .1f, 1f); // Red color 
             Graphic selectGraphic2 = rect[1].GetComponent<Graphic>(); //White interior
             selectGraphic2.color = new Color(1f, .95f, 1f); 
            StartCoroutine(ResetColorsAfterDelay(selectGraphic1, selectGraphic2, 1.2f, checkedSelectedItem));

            if (checkedSelectedItem == numberArray[numberArrayIndex - 1]) //Used to check if answer is correct
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

            currentScrollPosition = scrollableList.content.anchoredPosition.y;
            distanceTravelled = Math.Abs(currentScrollPosition - previousScrollPosition);
            SetTrialData();
        }
        IEnumerator ResetColorsAfterDelay(Graphic resetGraphic1, Graphic resetGraphic2, float delay, int checkedSelectedItem)
        {
            yield return new WaitForSeconds(delay);
        
            // Reset colors to original
            resetGraphic1.color = originalColor1;
            resetGraphic2.color = originalColor2;
            previousColorIndex = checkedSelectedItem-1;
            HighlightColors(previousColorIndex);
        }
        private void SetTrialData()
        {
            // Query trial data based on the user ID
            string userIdStr = firebaseGame.UserId.ToString();
            reference.Child("Game").Child("Study1").Child("Trials").Child("User" + userIdStr).OrderByKey().LimitToLast(1).GetValueAsync().
                ContinueWithOnMainThread(task =>
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
                        InsertTrial(new Trial(firebaseGame.UserId, firebaseGame.BlockId, lastTrialId + 1, timeToComplete, 
                            correctSelection, gameManager.AreaNumber, gameManager.TechniqueNumber, gameManager.SelectedItem, 
                            numberArray[numberArrayIndex-2], itemLocation, distanceToItem, distanceTravelled));
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
            DatabaseReference trialReference = reference.Child("Game").Child("Study1").Child("Trials").Child("User" + userIdStr).Child(trialIdStr);

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
}
