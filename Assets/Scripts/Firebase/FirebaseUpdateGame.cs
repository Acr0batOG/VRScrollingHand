using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using GameState;
using TMPro;
using UnityEngine;

// For ContinueWithOnMainThread

namespace Firebase
{
    public class FirebaseUpdateGame : Singleton<FirebaseUpdateGame>
    {
        [SerializeField] protected int userId;
        [SerializeField] protected int blockId;
        [SerializeField] protected bool loadData = false;
        [SerializeField] protected TextMeshPro dataText;
        protected DatabaseReference reference;
        protected GameManager gameManager;
        protected FirebaseAuth auth;
        protected int areaNumber;
        protected int techniqueNumber;
        protected bool bodyVisibility;
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
            }
        }

        void RetrieveAndSetUserData()
        {
            reference.Child("Game").Child("Users").Child(userId.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                
                if (task.IsCompleted)
                {
                    
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        Debug.Log("User Retrieved");
                        // Retrieve user data
                        string name = snapshot.Child("name").Value.ToString();
                        float userHeight = float.Parse(snapshot.Child("userHeight").Value.ToString());

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
            reference.Child("Game").Child("Users").Child(userId.ToString()).Child("Blocks").Child(blockId.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
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
                        dataText.text = "User Id: " + userId + "\n Area Number: " + areaNumber + "\n " +
                                        "(1 = Arm, 2 = Hand, 3 = Finger,\n 4 = Fingertip)\n Technique Number: "
                                        + techniqueNumber +
                                        " \n(1 = Rate, 2 = Select, 3 = Dynamic,\n 4 = Select-> Rate, " +
                                        "5 = Select -> Dynamic, 6 Dynamic One to One)\n";

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
