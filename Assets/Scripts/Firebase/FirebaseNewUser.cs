using System.Collections;
using System.Collections.Generic;
using Database_Objects;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions; // For ContinueWithOnMainThread

public class FirebaseNewUser : MonoBehaviour
{
    DatabaseReference reference;
    FirebaseAuth auth;
    [SerializeField] string userName;
    [SerializeField] float userHeight;
    int userId;
    int techniqueNumber;
    int areaNumber;
    bool bodyVisibility;
    // Previous values of userName and userHeight
    string previousUserName;
    float previousUserHeight;
    // Delay in seconds after typing stops for inserting new user
    float insertionDelay = 3.6f;
    Coroutine insertCoroutine;
    void Start()
    {
        previousUserName = userName;
        previousUserHeight = userHeight;

        // Initialize Firebase and authenticate user
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Set the root reference
                reference = FirebaseDatabase.DefaultInstance.RootReference;

                // Initialize Firebase Auth
                auth = FirebaseAuth.DefaultInstance;

                // Sign in the user anonymously
                SignInUser();
            }
            else
            {
                Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", task.Result));
            }
        });
    }

    void SignInUser()
    {
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

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            // Now that the user is authenticated, check and insert user
            CheckAndInsertUser();
        });
    }

    void Update()
    {
        // Check if userName or userHeight has changed
        if (userName != previousUserName || userHeight != previousUserHeight)
        {
            // Update previous values
            previousUserName = userName;
            previousUserHeight = userHeight;

            // Cancel the previous coroutine if it exists
            if (insertCoroutine != null)
            {
                StopCoroutine(insertCoroutine);
            }

            // Start a new coroutine for data insertion after a delay
            insertCoroutine = StartCoroutine(InsertAfterDelay());
        }
    }

    IEnumerator InsertAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(insertionDelay);

        // Retrieve all users and then check if the new user should be inserted
        CheckAndInsertUser();
    }

    void CheckAndInsertUser()
    {
        reference.Child("Game").Child("Users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                bool userExists = false;

                // Iterate through all users to check if a user with the same name and height exists
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    string name = childSnapshot.Child("name").Value.ToString();
                    float height = float.Parse(childSnapshot.Child("userHeight").Value.ToString());

                    if (name == userName && height == userHeight)
                    {
                        userExists = true; //If user exists, don't insert. Output message
                        break;
                    }
                }

                // If no such user exists, insert the new user
                if (!userExists)
                {
                    GetLastUserIdAndInsertUser();
                }
                else
                {
                    Debug.Log("A user with the same name and height already exists. No new user inserted.");
                }
            }
            else
            {
                Debug.LogError("Failed to retrieve users: " + task.Exception);
            }
        });
    }

    void GetLastUserIdAndInsertUser()
    {
        reference.Child("Game").Child("Users").OrderByKey().LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                int newUserId = 1; // Default userId if there are no users

                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    string lastUserIdStr = childSnapshot.Key;
                    if (int.TryParse(lastUserIdStr, out int lastUserId))
                    {
                        newUserId = lastUserId + 1;
                        userId = newUserId;
                    }
                }

                // Create a new user with the incremented userId
                User newUser = new User(newUserId, userName, userHeight);
                InsertUser(newUser);
            }
            else
            {
                Debug.LogError("Failed to retrieve last userId: " + task.Exception);
            }
        });
    }

    void InsertUser(User user)
    {
        // Convert userId to string to use it as a key
        string userIdStr = user.UserId.ToString();
        // Insert the user data into the "games" node in the database
        reference.Child("Game").Child("Users").Child(userIdStr).SetRawJsonValueAsync(JsonUtility.ToJson(user)).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("User data inserted successfully.");

                // Insert blocks for the new user
                // Commented out as no study is happening now and this is bound to change
                InsertBlocksForUser(user.UserId); 
            }
            else
            {
                Debug.LogError("Failed to insert user data: " + task.Exception);
            }
        });
    }

    void InsertBlocksForUser(int userId)
    {
        int k = 0;
        //Loop through each block combination the user will need to be tested on, and insert each user possiblity
        for (int area = 1; area <= 4; area++)
        {
            for (int technique = 1; technique <= 5; technique++)
            {
                
                k++;
                int blockId = k;

                // Set the values for the current combination
                techniqueNumber = technique;
                areaNumber = area;
                bodyVisibility = true;
                
                // Retrieve the last blockId and then insert a new block
                InsertBlock(new Block(blockId, userId, areaNumber, techniqueNumber, bodyVisibility));
                
            }
        }
    }


    void InsertBlock(Block block)
    {
        // Convert blockId to string to use it as a key
        string blockIdStr = block.BlockId.ToString();
        string userIdStr = block.UserId.ToString();
        // Check if the user exists
        reference.Child("Game").Child("Users").Child(userIdStr).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userSnapshot = task.Result;
                if (userSnapshot.Exists)
                {
                    // User exists, proceed with inserting the block
                    reference.Child("Game").Child("Users").Child(userIdStr).Child("Blocks").Child(blockIdStr).SetRawJsonValueAsync(JsonUtility.ToJson(block)).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            //Insert block data
                            Debug.Log("Block data inserted successfully.");
                        }
                        else
                        {
                            Debug.LogError("Failed to insert block data: " + task.Exception);
                        }
                    });
                }
                else
                {
                    Debug.LogError("User with ID " + block.UserId + " does not exist.");
                }
            }
            else
            {
                Debug.LogError("Failed to check user existence: " + task.Exception);
            }
        });
    }
}