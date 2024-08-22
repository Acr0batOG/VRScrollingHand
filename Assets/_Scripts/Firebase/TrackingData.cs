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
        [SerializeField] private int insertionFrequency = 1; // Number of inserts per second
        [SerializeField] private XRController xrController;
        private GameStart gameStart;
        private FirebaseUpdateGame firebaseGame;
        private DatabaseReference databaseReference;
        private GameManager gameManager;
        private float timeSinceLastInsert;
        private Stopwatch stopwatch;
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

                if (timeSinceLastInsert >= 1f / insertionFrequency)
                {
                    InsertTrackingData();
                    timeSinceLastInsert = 0f;
                }
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void InsertTrackingData()
        {
            try
            {
                if (firebaseGame.BlockId is > 0 and < 13 && dataObject.name is not ("PinchScroll" or "Right Controller"))
                {
                    Vector3 position = dataObject.position;
                    string positionString = $"{position.x}, {position.y}, {position.z}";
                    // Insert the position string into Firebase
                    string key = GetTimestamp(DateTime.Now);
                    databaseReference.Child("Game").Child("Study1").Child("tracking_data")
                        .Child("User" + firebaseGame.UserId).Child("Block" + firebaseGame.BlockId)
                        .Child(dataObject.name).Child(key)
                        .SetValueAsync(positionString).ContinueWithOnMainThread(task =>
                        {
                            if (task.Exception != null)
                            {
                                Debug.LogError($"Failed to insert data: {task.Exception}");
                            }
                        });
                }else if (firebaseGame.BlockId == 13 &&
                          (dataObject.name == "PinchScroll" || dataObject.name == "Other Fingertip"))
                {
                    Vector3 position = dataObject.position;
                    string positionString = $"{position.x}, {position.y}, {position.z}";
                    // Insert the position string into Firebase
                    string key = GetTimestamp(DateTime.Now);
                    databaseReference.Child("Game").Child("Study1").Child("tracking_data")
                        .Child("User" + firebaseGame.UserId).Child("Block" + firebaseGame.BlockId)
                        .Child(dataObject.name).Child(key)
                        .SetValueAsync(positionString).ContinueWithOnMainThread(task =>
                        {
                            if (task.Exception != null)
                            {
                                Debug.LogError($"Failed to insert data: {task.Exception}");
                            }
                        });
                }else if (firebaseGame.BlockId == 14)
                {
                    if (xrController != null && xrController.inputDevice.isValid)
                    {
                        // Try to get the primary 2D axis (thumbstick) value
                        if (xrController.inputDevice.TryGetFeatureValue(CommonUsages.primary2DAxis,
                                out Vector2 thumbstickInput))
                        {
                            // Get the vertical input from the thumbstick
                            float verticalInput = thumbstickInput.y;
                            
                            // Insert the position string into Firebase
                            string key = GetTimestamp(DateTime.Now);
                            databaseReference.Child("Game").Child("Study1").Child("tracking_data")
                                .Child("User" + firebaseGame.UserId).Child("Block" + firebaseGame.BlockId)
                                .Child("ControllerJoystick").Child(key)
                                .SetValueAsync(verticalInput.ToString()).ContinueWithOnMainThread(task =>
                                {
                                    if (task.Exception != null)
                                    {
                                        Debug.LogError($"Failed to insert data: {task.Exception}");
                                    }
                                });
                            
                        }
                    }
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
