using _Scripts.Database_Objects;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

namespace _Scripts.Firebase
{
    public class FirebaseNewUser : MonoBehaviour
    {
        DatabaseReference reference;
        FirebaseAuth auth;
        [SerializeField] string userName;
        [SerializeField] float userHeight;
        [SerializeField] private bool insertUser;
        int techniqueNumber;
        int areaNumber;
        int numberItems;
        // Previous values of userName and userHeight
        string previousUserName;
        private float previousUserHeight;
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
                    Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
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

                AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                // Now that the user is authenticated, check and insert user
                CheckAndInsertUser();
            });
        }

        void Update()
        {
            if (insertUser)
            {
                CheckAndInsertUser();
                insertUser = false;
            }
            // Check if userName or userHeight has changed
            if (userName != previousUserName || Mathf.Approximately(userHeight, previousUserHeight))
            {
                // Update previous values
                previousUserName = userName;
                previousUserHeight = userHeight;
            }
        }

        void CheckAndInsertUser()
        {
            reference.Child("Game").Child("Study2").Child("Users").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    bool userExists = false;

                    // Iterate through all users to check if a user with the same name and height exists
                    foreach (DataSnapshot childSnapshot in snapshot.Children)
                    {
                        string firebaseNameRetrieved = childSnapshot.Child("Name").Value.ToString();
                        float height = float.Parse(childSnapshot.Child("UserHeight").Value.ToString());

                        if (firebaseNameRetrieved == userName && Mathf.Approximately(height, userHeight))
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
            reference.Child("Game").Child("Study2").Child("Users").OrderByKey().LimitToLast(1).GetValueAsync().ContinueWithOnMainThread(
                task =>
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
            reference.Child("Game").Child("Study2").Child("Users").Child(userIdStr).SetRawJsonValueAsync(JsonUtility.ToJson(user)).
                ContinueWithOnMainThread(task =>
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
            int[] techniqueNum = { 3, 5 };
            int[] areaNum = { 1, 2 };
            int[] numberOfItems = { 50, 100, 200 };

            // Loop through each combination of techniqueNum, areaNum, and numberOfItems
            for (int i = 0; i < areaNum.Length; i++) // Loop through areaNum (1 and 2)
            {
                for (int j = 0; j < techniqueNum.Length; j++) // Loop through techniqueNum (3 and 5)
                {
                    for (int h = 0; h < numberOfItems.Length; h++) // Loop through numberOfItems (50, 100, 200)
                    {
                        int numberItems = numberOfItems[h];  
                        k++;
                        int blockId = k;

                        int techniqueNumber = techniqueNum[j];
                        int areaNumber = areaNum[i];

                        // Insert the current combination into the database
                        InsertBlock(new Block(blockId, userId, areaNumber, techniqueNumber, numberItems));
                    }
                }
            }

            // Now insert additional blocks for areaNumber = 3 AFTER area 1 & 2 are done
            int areaNumberThree = 3;

            for (int h = 0; h < numberOfItems.Length; h++) // Loop through numberOfItems (50, 100, 200)
            {
                int numberItems = numberOfItems[h]; // Get current number of items
    
                InsertBlock(new Block(++k, userId, areaNumberThree, 1, numberItems));
                
            }
            for (int h = 0; h < numberOfItems.Length; h++) 
            {
                int numberItems = numberOfItems[h]; // Get current number of items
    
                InsertBlock(new Block(++k, userId, areaNumberThree, 2, numberItems));
                
            }
            

        }




        void InsertBlock(Block block)
        {
            // Convert blockId to string to use it as a key
            string blockIdStr = block.BlockId.ToString();
            string userIdStr = block.UserId.ToString();
            // Check if the user exists
            reference.Child("Game").Child("Study2").Child("Users").Child(userIdStr).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DataSnapshot userSnapshot = task.Result;
                    if (userSnapshot.Exists)
                    {
                        // User exists, proceed with inserting the block
                        reference.Child("Game").Child("Study2").Child("Users").Child(userIdStr).Child("Blocks").Child(blockIdStr).
                            SetRawJsonValueAsync(JsonUtility.ToJson(block)).ContinueWithOnMainThread(blockInsertionTask =>
                        {
                            if (blockInsertionTask.IsCompleted)
                            {
                                //Insert block data
                                Debug.Log("Block data inserted successfully.");
                            }
                            else
                            {
                                Debug.LogError("Failed to insert block data: " + blockInsertionTask.Exception);
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
}