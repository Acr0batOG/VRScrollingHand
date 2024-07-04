using _Scripts.GameState;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

// For ContinueWithOnMainThread

namespace _Scripts.Firebase
{
    public class FirebaseUpdateGame : Singleton<FirebaseUpdateGame>
    {
        [SerializeField] protected int userId;
        [SerializeField] protected int blockId;
        [SerializeField] protected bool loadData;
        [SerializeField] protected bool startGame;
        [SerializeField] protected TextMeshPro dataText;
        private DatabaseReference reference;
        private GameManager gameManager;
        private FirebaseAuth auth;
        private int areaNumber;
        private int techniqueNumber;
        private bool bodyVisibility;
        //Property for UserId
        public int UserId
        {
            get { return userId; }
            set { userId = value; }
        }

        // Property for BlockId
        public int BlockId
        {
            get { return blockId; }
            set { blockId = value; }
        }
        //Property for LoadData
        public bool LoadData
        {
            get { return loadData; }
            set { loadData = value; }
        }
        public bool StartGame
        {
            get { return startGame; }
            set { startGame = value; }
        }
        void Start()
        {
            // Get the reference to GameManager
            gameManager = GameManager.instance;
            // Initialize Firebase and authenticate the user
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
                }
            });
        }

        void Update()
        {
            if (loadData)
            {
                // Retrieve user data and update GameManager
                RetrieveAndSetUserData();

                // Retrieve block data and update GameManager
                RetrieveAndSetBlockData();

                loadData = false;
                startGame = false;
            }
        }

        void RetrieveAndSetUserData()
        {
            reference.Child("Game").Child("Users").Child(userId.ToString()).GetValueAsync().ContinueWithOnMainThread(
                task =>
            {
                
                if (task.IsCompleted)
                {
                    
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        Debug.Log("User Retrieved");
                        // Retrieve user data
                        float userHeight = float.Parse(snapshot.Child("UserHeight").Value.ToString());

                        // Update GameManager with retrieved user data
                        gameManager.UserHeight = userHeight;
                    }
                    else
                    {
                        Debug.LogError("User data does not exist for userId: " + userId);
                    }
                }
                else
                {
                    Debug.LogError("Failed to retrieve user data: " + task.Exception);
                }
            });
        }

        void RetrieveAndSetBlockData()
        {
            // Query block data based on the blockId under the specified user ID
            reference.Child("Game").Child("Users").Child(userId.ToString()).Child("Blocks").Child(blockId.ToString()).
                GetValueAsync().ContinueWithOnMainThread(task =>
            {
                
                if (task.IsCompleted)
                {
                    
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        // Retrieve block data
                        areaNumber = int.Parse(snapshot.Child("AreaNumber").Value.ToString());
                        techniqueNumber = int.Parse(snapshot.Child("TechniqueNumber").Value.ToString());
                        bodyVisibility = bool.Parse(snapshot.Child("BodyVisibility").Value.ToString());
                        //Load the block data into the game. Changing Area and Technique based on block 
                        dataText.text = "User Id: " + userId + "\n Area Number: " + areaNumber + "\n Technique Number: "
                                        + techniqueNumber +" \n";

                        // Update GameManager with retrieved block data
                        gameManager.AreaNumber = areaNumber;
                        gameManager.TechniqueNumber = techniqueNumber;
                        gameManager.BodyVisibility = bodyVisibility;
                    }
                    else
                    {
                        Debug.LogError("Block data does not exist for blockId: " + blockId);
                    }
                }
                else
                {
                    Debug.LogError("Failed to retrieve block data: " + task.Exception);
                }
            });
        }
    
    }
}
