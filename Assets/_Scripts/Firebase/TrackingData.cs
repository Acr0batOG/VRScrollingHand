using System;
using System.Diagnostics;
using _Scripts.GameState;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace _Scripts.Firebase
{
    public class TrackingData : MonoBehaviour
    {
        [SerializeField] private CapsuleCollider starter;
        [SerializeField] private Transform dataObject;
        [SerializeField] private int insertionFrequency = 1; // Number of inserts per second
        private GameStart gameStart;
        private FirebaseUpdateGame firebaseGame;
        private DatabaseReference databaseReference;
        private GameManager gameManager;
        private float timeSinceLastInsert = 0f;
        private Stopwatch stopwatch;

        private int a;
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
            a++;
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

        private void InsertTrackingData()
        {
            try
            {
                if (firebaseGame.BlockId > 0 && firebaseGame.BlockId < 15)
                {
                    Vector3 position = dataObject.position;
                    string positionString = $"{position.x}, {position.y}, {position.z}";
                    // Insert the position string into Firebase
                    string key = a.ToString();
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
                }
            }
            catch (Exception e)
            {
                Debug.Log(e + " (Arnold accent) I'll be back");
            }
        }
    }
}
