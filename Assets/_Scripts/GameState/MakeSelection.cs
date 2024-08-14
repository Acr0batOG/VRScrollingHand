using System;
using System.Collections;
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
        protected GameManager GameManager;
        protected TextMeshPro SelectText;
        protected int SelectedItem;
        protected readonly float ItemHeight = 55f; // Block item height
        protected float ItemCountMultiplier = 1.3f; // Multiplier for items
        protected float ContentSize;
        protected int ItemCount;
        protected Slider SelectionBar;
        private float[] correctArray = new float[51];
        private float itemDistanceInit = (2454.621f / 49f);
        private DatabaseReference databaseReference;
        private bool isInitialized = false; // Flag to check if setup is complete
        private bool isCooldownActive = false; // Flag to check if cooldown is active

        void Start()
        {
            GameManager = GameManager.instance;
            SelectedItem = GameManager.SelectedItem;
            SelectionBar = GameObject.FindWithTag("SelectionBar").GetComponent<Slider>();
            SelectionBar.value = 0f;
            InitializeArray();
            ItemCount = GameManager.NumberOfItems;

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

        IEnumerator WaitBeforeSetFlag()
        {
            yield return new WaitForSeconds(2.0f);
            isInitialized = true;
        }

        void InitializeArray()
        {
            for (int i = 0; i <= 50; i++)
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

            // Only process the event if the initial setup is complete and cooldown is not active
            if (isInitialized && !isCooldownActive)
            {
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

            GameManager.SelectedItem = SelectedItem;
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
            yield return new WaitForSeconds(.5f);
            SelectionBar.value = 0f;

            // Deactivate the cooldown
            isCooldownActive = false;
        }
    }
}
