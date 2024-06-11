using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase.Database;

public class FirebaseManager : MonoBehaviour
{
    private FirebaseAuth auth;
    private DatabaseReference dbReference;

    // Start is called before the first frame update
    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;

            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp
                var app = Firebase.FirebaseApp.DefaultInstance;

                // Initialize Firebase Auth
                auth = FirebaseAuth.DefaultInstance;

            }
            else
            {
                Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }

    

    // Update is called once per frame
    void Update()
    {
        
    }
}
