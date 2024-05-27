using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions; // For ContinueWithOnMainThread

public class FirebaseNewUser : MonoBehaviour
{
    DatabaseReference reference;
    [SerializeField] string userName;
    [SerializeField] float userHeight;
    int userId;
    int techniqueNumber;
    int areaNumber;
    bool bodyVisibility;

    // Previous values of userName and userHeight
    string previousUserName;
    float previousUserHeight;

    // Delay in seconds after typing stops
    float insertionDelay = 3.6f;
    Coroutine insertCoroutine;

    void Start()
    {
        previousUserName = userName;
        previousUserHeight = userHeight;
        // Initialize Firebase and get the root reference location of the database.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Set the root reference
                reference = FirebaseDatabase.DefaultInstance.RootReference;
                CheckAndInsertUser();
            }
            else
            {
                Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", task.Result));
            }
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
        reference.Child("Users").GetValueAsync().ContinueWithOnMainThread(task =>
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
                        userExists = true;
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
    reference.Child("Users").OrderByKey().LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(task =>
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
    string userIdStr = user.userId.ToString();
    // Insert the user data into the "users" node in the database
    reference.Child("Users").Child(userIdStr).SetRawJsonValueAsync(JsonUtility.ToJson(user)).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompleted)
        {
            Debug.Log("User data inserted successfully.");

            // Insert blocks for the new user
            InsertBlocksForUser(user.userId);
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
        for (int a = 1; a <= 2; a++)
        {
            for (int t = 1; t <= 3; t++)
            {
                for (int v = 0; v <= 1; v++)
                {   k++;
                int blockId = k;

                    // Set the values for the current combination
                    techniqueNumber = t;
                    areaNumber = a;
                    bodyVisibility = v == 1;

                    // Retrieve the last blockId and then insert a new block
                    InsertBlock(new Block(blockId, userId, techniqueNumber, areaNumber, bodyVisibility));
                }
            }
        }
    }

    void InsertBlock(Block block)
    {
        // Convert blockId to string to use it as a key
        string blockIdStr = block.blockId.ToString();

        // Check if the user exists
        reference.Child("Users").Child(block.userId.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot userSnapshot = task.Result;
                if (userSnapshot.Exists)
                {
                    // User exists, proceed with inserting the block
                    reference.Child("Users").Child(block.userId.ToString()).Child("Blocks").Child(blockIdStr).SetRawJsonValueAsync(JsonUtility.ToJson(block)).ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            Debug.Log("Block data inserted successfully.");
                        }else
                        {
                            Debug.LogError("Failed to insert block data: " + task.Exception);
                        }
                    });
                }
                else
                {
                    Debug.LogError("User with ID " + block.userId + " does not exist.");
                }
            }
            else
            {
                Debug.LogError("Failed to check user existence: " + task.Exception);
            }
        });
    }
}



