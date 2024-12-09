using System;
using System.Collections;
using _Scripts.OptiTrack;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

namespace _Scripts.GameState
{
    public class MakeSelection : MonoBehaviour
    {
        [SerializeField] protected ScrollRect scrollableList;
        protected GameManager gameManager;
        protected TextMeshPro SelectText;
        protected StarterAlignment starterAlignment;
        protected int SelectedItem;
        protected readonly float ItemHeight = 55f; // Block item height
        protected float ItemCountMultiplier = 1.3f; // Multiplier for items
        protected float ContentSize;
        protected int ItemCount;
        private bool isProcessing; // Flag to prevent multiple executions
        protected Slider SelectionBar;
        private float[] correctArray;
        private float itemDistanceInit = (2454.621f / 49f);
        private DatabaseReference databaseReference;
        private bool isInitialized = false; // Flag to check if setup is complete
        private bool isCooldownActive = false; // Flag to check if cooldown is active

        void Start()
        {
            gameManager = GameManager.instance;
            starterAlignment = StarterAlignment.instance;
            correctArray = new float[gameManager.NumberOfItems+1];
            SelectedItem = gameManager.SelectedItem;
            SelectionBar = GameObject.FindWithTag("SelectionBar").GetComponent<Slider>();
            SelectionBar.value = 0f;
            InitializeArray();
            ItemCount = gameManager.NumberOfItems;
            gameManager.ArduinoSelect = false;
            

            // Initialize Firebase and set up the listener
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogError($"Failed to initialize Firebase: {task.Exception}");
                    return;
                }

                FirebaseDatabase database = FirebaseDatabase.DefaultInstance;
                databaseReference = database.RootReference;

                // Set up a listener for changes in the "button_presses" path
                databaseReference.Child("button_presses").ChildAdded += OnChildAdded;
                StartCoroutine(WaitBeforeSetFlag());
            });
        }

        private void Update()
        {
            // Check if Keypad8 is pressed and processing is not already active
            if (Input.GetKeyDown(KeyCode.Keypad8) && !isProcessing)
            {
                isProcessing = true; // Prevent further calls during processing

                if (isInitialized && !isCooldownActive && !gameManager.ArduinoSelect)
                {
                    Debug.Log("Removing Collider");
                    StartCoroutine(ProcessRemoveCollider());
                }
                else if (isInitialized && !isCooldownActive && gameManager.ArduinoSelect)
                {
                    Debug.Log("Making Selection");
                    StartCoroutine(ProcessSelection());
                }
                else
                {
                    // Reset the flag if no conditions are met
                    isProcessing = false;
                }
            }
        }

        private IEnumerator ProcessRemoveCollider()
        {
            yield return new WaitForSeconds(.4f);
            starterAlignment.ButtonSelectedRemoveCollider();
            yield return new WaitForSeconds(0.4f); // Optional delay if necessary
            isProcessing = false; // Reset the flag
        }

        private IEnumerator ProcessSelection()
        {
            SelectItem();
            yield return new WaitForSeconds(0.4f); // Optional delay if necessary
            isProcessing = false; // Reset the flag
        }

        IEnumerator WaitBeforeSetFlag()
        {
            yield return new WaitForSeconds(2.0f);
            isInitialized = true;
        }
        
        

        void InitializeArray()
        {
            for (int i = 0; i <= gameManager.NumberOfItems; i++)
            {
                correctArray[i] = i * itemDistanceInit - 25f;
            }
        }

        void OnChildAdded(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError("Firebase database error: " + args.DatabaseError.Message);
                return;
            }
            // Use Arduino Button to Remove Collider
            //Works to remove collider
            if (isInitialized && !isCooldownActive && !gameManager.ArduinoSelect)
            {
                Debug.Log("Removing Collider");
                starterAlignment.ButtonSelectedRemoveCollider(); // Works
            } 

            // Only process the event if the initial setup is complete and cooldown is not active
            // Use the Arduino Button to make selection
           else if (isInitialized && !isCooldownActive && gameManager.ArduinoSelect)
            {
                Debug.Log("Making Selection");
                SelectItem();
            }
        }

        protected void SelectItem()
        {
            // Activate the cooldown
            isCooldownActive = true;

            GameObject selectTextObject = GameObject.FindWithTag("ItemSelect"); // Get item to show selection
            float currentPositionY = scrollableList.content.anchoredPosition.y;
            SelectionBar.value = 1.4f;

            // Find the index of the color range that currentPositionY is within
            for (int i = 0; i < correctArray.Length - 1; i++)
            {
                if (currentPositionY >= correctArray[i] && currentPositionY <= correctArray[i + 1])
                {
                    SelectedItem = i + 1;
                }
            }

            gameManager.SelectedItem = SelectedItem;
            StartCoroutine(ResetSelection());
            

            // Check if the GameObject was found
            if (selectTextObject)
            {
                // Get the TextMeshProUGUI component from the GameObject
                SelectText = selectTextObject.GetComponent<TextMeshPro>();

                // Check if the component was found
                if (SelectText)
                {
                    // Set text to the item selected
                    SelectText.text = "Item Selected: " + SelectedItem;
                }
            }
        }

        IEnumerator ResetSelection()
        {
            yield return new WaitForSeconds(.65f);
            gameManager.ArduinoSelect = false;
            SelectionBar.value = 0f;

            // Deactivate the cooldown
            isCooldownActive = false;
        }

       
    }
}
