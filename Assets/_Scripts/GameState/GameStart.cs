using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using _Scripts.Database_Objects;
using _Scripts.Firebase;
using _Scripts.OptiTrack;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace _Scripts.GameState
{
    public class GameStart : Singleton<GameStart>
    {
        [SerializeField] public TextMeshProUGUI selectNumber; // Number to be selected by user.
        [SerializeField] TextMeshProUGUI correctText;
        [SerializeField] bool testMode;
        [SerializeField] bool saveData;
        [SerializeField] bool timeLimit;
        [SerializeField] int selectedNumber;
        [SerializeField] private int itemMultiplier = 2;
        [SerializeField] protected ScrollRect scrollableList;
        [SerializeField] AudioSource correctAudioSource;
        [SerializeField] AudioSource incorrectAudioSource;
        public String trialIdStr;
        public String userIdStr;
        private GameManager gameManager;
        private StarterAlignment starterAlignment;
        private StarterHandAlignment starterHandAlignment;
        private FirebaseUpdateGame firebaseGame;
        private DatabaseReference reference;
        private FirebaseAuth auth;
        private Stopwatch stopwatch;
        public List<int> numberArray = new(); // Array to hold numbers the user will select, will be shuffled each time
        private float[] distanceArray = new float [50];
        private float[] colorArray = new float [51];
        public int numberArrayIndex;
        int previousSelectedItem;
        private int previousSelectedNumber;
        public bool stopGame; //Used to stop game once all items selected
        int selectedItem;
        public int currentBlockId;
        public int currentUserId;
        int previousBlockId;
        int previousUserId;
        int previousNumberOfItems;
        private int startingItem = 25;
        private int timeExceededValue = -1;
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
        private ColorBlock colors;
        private Graphic graphic1;
        private Graphic graphic2;
        private readonly Color highlightColor1 = new Color(.9f, .85f, 0f); // Highlighted color
        private readonly Color highlightColor2 = new Color(.94f, .94f, 94f); // Highlighted color (light gray)
        private int currentColorIndex = -1; // Index of the current highlighted color
        private int previousColorIndex = -1; // Index of the previous highlighted color

        public bool StopGame
        {
            get { return stopGame; }
            set { stopGame = value; }
        }
    
        // Only 10 values will be read for each trial. But different every time and non-repeating
        void Start()
        {
            saveData = true;
            gameManager = GameManager.instance; //Game manager instance 
            firebaseGame = FirebaseUpdateGame.instance; //Firebase manager instance
            starterAlignment = StarterAlignment.instance;
            starterHandAlignment = StarterHandAlignment.instance;
            FirebaseSetup();
            InitialSetup();
            SetGameStart(); //Start up the game
            InitializeArray();

        }

        void FirebaseSetup()
        {
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
        }

        void InitializeArray()
        {
            for (int i = 0; i <= 50; i++)
            {
                if(i < 50)
                    distanceArray[i] = i * itemDistanceInit;
                //Debug.Log("arr[" + i + "] = " + distanceArray[i]);
                colorArray[i] = i * itemDistanceInit - 25f;
            }
        }
        void InitialSetup()
        {
            stopwatch = new Stopwatch(); //Create a stopwatch object for timing
            FillArray(numberArray); // Fill the selection array
            Shuffle(numberArray); // Shuffle the array for selection
            previousNumberOfItems = gameManager.NumberOfItems;
            currentBlockId = firebaseGame.BlockId; // UserId and blockId from superclass
            currentUserId = firebaseGame.UserId;
            previousBlockId = currentBlockId; // Set previous value to have an on change in the update
            previousUserId = currentUserId;
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

        public void GetColor()
        {
            RectTransform outerRect = scrollableList.content.GetChild(0).GetComponent<RectTransform>();
            Button button = outerRect.GetComponent<Button>();
            // Get the current ColorBlock
            colors = button.colors;
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
                    if (currentColorIndex != i)
                    {
                        previousColorIndex = currentColorIndex;
                        currentColorIndex = i;
                        UpdateHighlightedColors();
                    }
                    break;
                }
            }

            
            UpdateBlockAndUserId();

            if (NeedsGameReset())
            {
                ResetGameSettings();
            }

            if (!testMode)
            {
                SelectionChange();
            }
            else if (previousSelectedNumber != selectedNumber && !isCoroutineRunning)
            {
                StartCoroutine(TestSelectionChange());
            }

            if (timeLimit)
            {
                if (stopwatch.Elapsed.Seconds >= 15)
                {
                    Debug.Log("Time exceeded");
                    CheckCorrect(timeExceededValue);
                    timeExceededValue--;
                }
            }

            if (gameManager.InitalizeList)
            {
                GetColor();
                gameManager.InitalizeList = false;
            }
        }

        private void UpdateHighlightedColors()
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

        private void UpdateBlockAndUserId()
        {
            if (previousBlockId != firebaseGame.BlockId)
            {
                currentBlockId = firebaseGame.BlockId;
                previousBlockId = currentBlockId;
            }

            if (previousUserId != firebaseGame.UserId)
            {
                currentUserId = firebaseGame.UserId;
                previousUserId = currentUserId;
            }
        }

        private bool NeedsGameReset()
        {
            return firebaseGame.LoadData || 
                   gameManager.NumberOfItems != previousNumberOfItems || 
                   gameManager.TechniqueNumber != gameManager.PreviousTechnique || 
                   gameManager.AreaNumber != gameManager.PreviousArea;
        }

        private void ResetGameSettings()
        {
            stopwatch = new Stopwatch();
            ResetGame();
            SetGameStart();
        }

        void HighlightColors(int index)
        {
           RectTransform outerRect = scrollableList.content.GetChild(index).GetComponent<RectTransform>();
           Button button = outerRect.GetComponent<Button>();
           // Get the current ColorBlock
           ColorBlock newColors = button.colors;

           // Set all states to white
           newColors.normalColor = Color.white;
           newColors.highlightedColor = Color.white;
           newColors.pressedColor = Color.white;
           newColors.selectedColor = Color.white;
           newColors.disabledColor = Color.white;

           // Apply the modified ColorBlock back to the button
           button.colors = newColors;
            RectTransform[] rect = scrollableList.content.GetChild(index).GetComponentsInChildren<RectTransform>();
            graphic1 = rect[0].GetComponent<Graphic>(); // Border
            graphic1.color = highlightColor1;
            graphic2 = rect[1].GetComponent<Graphic>(); // White interior
            graphic2.color = highlightColor2;
        }

        void ResetColors(int index)
        {
            RectTransform outerRect = scrollableList.content.GetChild(index).GetComponent<RectTransform>();
            Button button = outerRect.GetComponent<Button>();
            button.colors = colors;
            RectTransform[] rect = scrollableList.content.GetChild(index).GetComponentsInChildren<RectTransform>();
            graphic1 = rect[0].GetComponent<Graphic>(); // Border
            graphic1.color = originalColor1;
            graphic2 = rect[1].GetComponent<Graphic>(); // White interior
            graphic2.color = originalColor2;
        }
        void ResetGame(){
            previousNumberOfItems = gameManager.NumberOfItems;
            Shuffle(numberArray);
            numberArrayIndex = 0; //Set array index back to 0
            currentBlockId = firebaseGame.BlockId; // UserId and blockId from superclass
            currentUserId = firebaseGame.UserId;
            previousBlockId = currentBlockId; // Set previous value to have an on change in the update
            previousUserId = currentUserId;
            correctText.text = ""; //Reset correct text
            stopGame = false; //Reset stop game to allow selections again.
        }

        void FillArray(List<int> array)
        {
            float[] rangeArray = {.25f,.35f,.55f,.65f,.85f,.95f}; //Holds the difference between items
            float[] valueArray = new float[3];
            int[] itemsToBeUsed = new int[6];
            int k = 0;
            for (int i = 0; i < rangeArray.Length; i+=2)
            {
                valueArray[k] = Random.Range(rangeArray[i], rangeArray[i + 1]); //Random Value between the items
                k++;
            }

            k = 0;
            //Get the value to + or -
            //25*valueArray[i]
            for (int i = 0; i < itemsToBeUsed.Length; i+=2)
            {
                itemsToBeUsed[i] = startingItem + (int)(startingItem * valueArray[k]); //Positive - Cast to int
                itemsToBeUsed[i+1] =  startingItem + (int)(startingItem * valueArray[k] * -1); //Negative - Cast to Int
                k++;
                //Correct - gets the percentile averages where it should be
            }
            
            for (int i = 0; i < itemMultiplier; i++)
            {
                array.AddRange(itemsToBeUsed);
            }
            
            
        }
        void RemoveArray(List<int> array)
        {
            array.Clear(); // Remove all array elements before adding them again
        }

        void Shuffle(List<int> array)
        {
            // Used to randomize array elements from 1-50
            System.Random random = new System.Random();
            int n = array.Count;

            // Perform the Fisher-Yates shuffle
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1); // Random index from 0 to I
                // Swap array[i] with the element at random index
                (array[i], array[j]) = (array[j], array[i]);
            }

            // Check for consecutive duplicates and rearrange if needed
            for (int i = 1; i < n; i++)
            {
                if (array[i] == array[i - 1])
                {
                    // Find a new index to swap with that is not the same as the previous element
                    int j = i;
                    while (j == i || array[j] == array[i - 1])
                    {
                        j = random.Next(0, n);
                    }
                    // Swap to avoid consecutive duplicates
                    (array[i], array[j]) = (array[j], array[i]);
                }
            }

            string arrayString = string.Join(", ", array);
            Debug.Log(arrayString);
            
        }
        

        void SetGameStart()
        {
            if (gameManager.AreaNumber == 3 && gameManager.TechniqueNumber == 2)
                selectNumber.text = "Select A Button to Begin";
            else
                selectNumber.text = "Select Arm Object To Begin";

        }

        private void SelectionChange()
        {
           
                selectedItem = gameManager.SelectedItem;
                if (selectedItem != previousSelectedItem) //Massive fucking bug right here!
                //Doesn't work if user tries to select the same number twice, but I don't know how to fix it
                {
                    previousSelectedItem = selectedItem;
                    if (numberArrayIndex > 0) // Only check correctness after the first selection
                    {
                        CheckCorrect(selectedItem); //Check if selection is correct 
                    }

                    StartCoroutine(WaitBeforeNew());
                    
                    //SetNumber(); //Set next item
                }
            }
        

        IEnumerator WaitBeforeNew()
        {
            yield return new WaitForSeconds(.4f);
            ResetAfterSelection();
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

        public void SetNumber()
        {
            if (numberArrayIndex < numberArray.Count)
            {
                //if (numberArrayIndex > 0)
                //{
                    // Debug.Log(numberArray[numberArrayIndex - 1]);
                    // Debug.Log("Actual Position " + scrollableList.content.anchoredPosition.y);
                    // Debug.Log("Array Number " + arr[numberArray[numberArrayIndex-1]-1]);
                    itemLocation = distanceArray[numberArray[numberArrayIndex] - 1];
                    distanceToItem =  Math.Abs(scrollableList.content.anchoredPosition.y - itemLocation);
                    // Debug.Log(distanceToItem);
                //}
                
                //When items are left to select
                stopwatch.Start(); //Start time
                previousScrollPosition = scrollableList.content.anchoredPosition.y;
                //Display item to be selected
                if (selectedItem == numberArray[numberArrayIndex]) //If the previous item selected is supposed to be the
                //Next item, swap them.
                {
                    (numberArray[numberArrayIndex], numberArray[numberArrayIndex + 1]) = (numberArray[numberArrayIndex + 1], numberArray[numberArrayIndex]);
                }
                    selectNumber.text = "Item #" + (numberArrayIndex + 1) + ", Please Select: " +
                                        numberArray[numberArrayIndex]
                                            .ToString(); // Set the number the user will be retrieving
                    numberArrayIndex++; //Update the array index to select next item
                
            }
            else
            {   //Once all items have been selected
                selectNumber.text = "No more items to select.";
                numberArrayIndex++;
                stopGame = true;
            }
        }
        //Check if answer is correct
        // ReSharper disable Unity.PerformanceAnalysis
        void CheckCorrect(int checkedSelectedItem)
        {
            gameManager.SelectedItem = checkedSelectedItem;
            stopwatch.Stop(); //Stop the timer
            TimeSpan elapsedTime = stopwatch.Elapsed; //Save elapsed time
            stopwatch.Reset(); //Reset stopwatch to 0
            //Get completion time as a float
            completionTime = (float)elapsedTime.TotalSeconds;
            try
            {
                RectTransform[] rect = scrollableList.content.GetChild(checkedSelectedItem - 1)
                    .GetComponentsInChildren<RectTransform>();
                Graphic selectGraphic1 = rect[0].GetComponent<Graphic>(); //border
                Graphic selectGraphic2 = rect[1].GetComponent<Graphic>(); //White interior
            

            StartCoroutine(ResetColorsAfterDelay(selectGraphic1, selectGraphic2, 1.2f, checkedSelectedItem));
            if (!stopGame) //Boolean to check answer
            {
                if (checkedSelectedItem == numberArray[numberArrayIndex - 1]) //Used to check if answer is correct
                {
                    //Answer is correct, set correct to true. Play sound and display green text
                    isCorrect = true;
                    correctText.text = "Correct";
                    correctText.color = Color.green; // Set color to green for correct
                    correctAudioSource.Play(); // Play the correct audio clip
                    selectGraphic1.color = new Color(1f, .3686f, .3686f);
                    selectGraphic2.color = Color.green;

                }
                else
                {
                    //Answer is incorrect. Play incorrect noise, set isCorrect to false and display red text
                    isCorrect = false;
                    correctText.text = "Incorrect";
                    correctText.color = Color.red; // Set color to red for incorrect
                    incorrectAudioSource.Play(); // Play the incorrect audio clip
                    selectGraphic1.color = new Color(1f, .3686f, .3686f);
                    selectGraphic2.color = new Color(1f, .2f, .2f);
                }

                currentScrollPosition = scrollableList.content.anchoredPosition.y;
                distanceTravelled = Math.Abs(currentScrollPosition - previousScrollPosition);
                
                SetTrialData();
               
            }

            }catch (Exception e)
            {
                Debug.Log(e + " Exception: With checking the answer and changing the color");
                //Answer is incorrect. Play incorrect noise, set isCorrect to false and display red text
                if (!stopGame) //Boolean to check answer
                {
                    isCorrect = false;
                    correctText.text = "Incorrect";
                    correctText.color = Color.red; // Set color to red for incorrect
                    incorrectAudioSource.Play(); // Play the incorrect audio clip
                    currentScrollPosition = scrollableList.content.anchoredPosition.y;
                    distanceTravelled = Math.Abs(currentScrollPosition - previousScrollPosition);
                    completionTime = 15.0f;
                    SetTrialData();
                }
            }

        
        }
        // ReSharper disable Unity.PerformanceAnalysis
        IEnumerator ResetColorsAfterDelay(Graphic resetGraphic1, Graphic resetGraphic2, float delay, int checkedSelectedItem)
        {
            yield return new WaitForSeconds(delay);
            try
            {
                if (resetGraphic1 != null && resetGraphic2 != null)
                {
                    // Reset colors to original
                    resetGraphic1.color = originalColor1;
                    resetGraphic2.color = originalColor2;
                    previousColorIndex = checkedSelectedItem - 1;
                    ResetColors(previousColorIndex);
                }
            }
            catch (Exception e)
            {
                Debug.Log("This error sucks my dudes "+ e + " I also hate eggshells in my sandwich");
            }
        }

        private void ResetAfterSelection()
        {
            
            gameManager.DisableAllArmUIControllers();
            starterAlignment.scrollList.RemoveListItems();
            
            StartCoroutine(Wait());
            if (gameManager.AreaNumber == 3 && gameManager.TechniqueNumber == 2)
                selectNumber.text = "Select A Button to Continue";
            else if (numberArrayIndex >= numberArray.Count)
                selectNumber.text = "No More Items to Select";
            else
                selectNumber.text = "Select the Object to Continue";

        }

        private IEnumerator Wait()
        { 
            yield return new WaitForSeconds(.36f);
            if (gameManager.AreaNumber == 1||gameManager.AreaNumber==3)
            {
                starterAlignment.startCollider.enabled = true;
                starterAlignment.startRenderer.enabled = true;
                starterHandAlignment.startCollider.enabled = false;
                starterHandAlignment.startRenderer.enabled = false;
            }
            else if(gameManager.AreaNumber == 2)
            {
                starterHandAlignment.startCollider.enabled = true;
                starterHandAlignment.startRenderer.enabled = true;
                starterAlignment.startCollider.enabled = false;
                starterAlignment.startRenderer.enabled = false;
            }
        }

        public void DisableColliders()
        {
            starterAlignment.startCollider.enabled = false;
            starterAlignment.startRenderer.enabled = false;
            starterHandAlignment.startCollider.enabled = false;
            starterHandAlignment.startRenderer.enabled = false;
        }
        private void SetTrialData()
        {
            // Query trial data based on the user ID
            string userIdStr = firebaseGame.UserId.ToString();
            reference.Child("Game").Child("Study2").Child("Trials").Child("User" + userIdStr).OrderByKey().LimitToLast(1).GetValueAsync().
                ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    // Get the last trialId
                    int lastTrialId = 0;
                    //Insert correct answer bool and time to select answer in Firebase
                    bool correctSelection = isCorrect;
                    float timeToComplete = completionTime;
                    if (gameManager.SelectedItem < 0)
                    {
                        timeToComplete = 15.0f;
                    }
                    if(saveData&&!firebaseGame.PracticeMode){
                        // Insert a new trial with the incremented trialId
                        InsertTrial(new Trial(firebaseGame.UserId, firebaseGame.BlockId, lastTrialId++, timeToComplete, 
                            correctSelection, gameManager.AreaNumber, gameManager.TechniqueNumber, gameManager.SelectedItem, 
                            numberArray[numberArrayIndex-1], itemLocation, distanceToItem, distanceTravelled));
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
            trialIdStr = trial.TrialId.ToString(); 
            userIdStr = trial.UserId.ToString();

            DateTime theTime = DateTime.Now;
            string datetime = theTime.ToString("yyyy-MM-dd\\THH:mm:ss\\Z");
            // Form the reference path for inserting the trial data
            DatabaseReference trialReference = reference.Child("Game").Child("Study2").Child("Trials").Child("User" + userIdStr).Child(datetime);
            
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
