using System;
using System.Diagnostics;
using _Scripts.GameState;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Debug = UnityEngine.Debug;

namespace _Scripts.Firebase
{
    public class TrackingData : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider starter;
        [SerializeField] private Transform dataObject;
        //[SerializeField] private int insertionFrequency = 1; // Number of inserts per second
        [SerializeField] private XRController xrController;
        private GameStart gameStart;
        private FirebaseUpdateGame firebaseGame;
        private DatabaseReference databaseReference;
        private GameManager gameManager;
        private Stopwatch stopwatch;
        private string accumulatedData = ""; // String to accumulate data
        private float timeSinceLastInsert = 0f;
        private float timeSinceLastDataCollection = 0f;
        private const float InsertionInterval = 10f; // Interval in seconds for Firebase insertion
        private const float DataCollectionInterval = 0.2f; // Interval in seconds for data collection (5 times per second)


        // Start is called before the first frame up
        void Start()
        {
            firebaseGame = FirebaseUpdateGame.instance;
            gameManager = GameManager.instance;
            gameStart = GameStart.instance;
            stopwatch = new Stopwatch();
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
            });
            stopwatch.Start();
        }

        // Update is called once per frame
        void Update()
        {
            if (gameManager.TrackData && starter.gameObject.activeSelf)
            {
                timeSinceLastInsert += Time.deltaTime;
                timeSinceLastDataCollection += Time.deltaTime;

                if (!firebaseGame.PracticeMode)
                {
                    // Collect data only if the collection interval has passed
                    if (timeSinceLastDataCollection >= DataCollectionInterval)
                    {
                        AppendTrackingData();
                        timeSinceLastDataCollection = 0f; // Reset data collection timer
                    }

                    // Insert data into Firebase every 10 seconds
                    if (timeSinceLastInsert >= InsertionInterval)
                    {
                        InsertAccumulatedData();
                        timeSinceLastInsert = 0f;
                        accumulatedData = ""; // Clear accumulated data after insertion
                    }
                }
            }
        }

        private void AppendTrackingData()
        {
            try
            {
                if (firebaseGame.BlockId is > 0 and < 13 &&
                    dataObject.name is not ("PinchScroll" or "Right Controller"))
                {
                    Vector3 position = dataObject.position;
                    string positionString = $"{position.x}, {position.y}, {position.z}";
                    accumulatedData += $"[{GetTimestamp(DateTime.Now)}] {dataObject.name}: {positionString}\n";
                }
                else if (firebaseGame.BlockId == 13 &&
                         (dataObject.name == "PinchScroll" || dataObject.name == "Other Fingertip"))
                {
                    Vector3 position = dataObject.position;
                    string positionString = $"{position.x}, {position.y}, {position.z}";
                    accumulatedData += $"[{GetTimestamp(DateTime.Now)}] {dataObject.name}: {positionString}\n";
                }
                else if (firebaseGame.BlockId == 14)
                {
                    if (xrController != null && xrController.inputDevice.isValid)
                    {
                        if (xrController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis,
                                out Vector2 thumbstickInput))
                        {
                            float verticalInput = thumbstickInput.y;
                            accumulatedData += $"[{GetTimestamp(DateTime.Now)}] ControllerJoystick: {verticalInput}\n";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e + " (Arnold accent) I'll be back");
            }
        }

        private void InsertAccumulatedData()
        {
            try
            {
                if (!string.IsNullOrEmpty(accumulatedData))
                {
                    string key = GetTimestamp(DateTime.Now);
                    databaseReference.Child("Game").Child("Study1").Child("tracking_data")
                        .Child("User" + firebaseGame.UserId).Child("Block" + firebaseGame.BlockId)
                        .Child("AccumulatedData").Child(key)
                        .SetValueAsync(accumulatedData).ContinueWithOnMainThread(task =>
                        {
                            if (task.Exception != null)
                            {
                                Debug.LogError($"Failed to insert accumulated data: {task.Exception}");
                            }
                        });
                }
            }
            catch (Exception e)
            {
                Debug.Log(e + " (Arnold accent) I'll be back");
            }
        }

        private static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyy,MM,dd,HH,mm,ss,ffff");
        }
    }
}