using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro; 

public class FirebaseUpdateGame : MonoBehaviour
{
    
    [SerializeField] protected int userId;
    [SerializeField] protected int blockId;
    [SerializeField] protected bool loadData;
    [SerializeField] protected TextMeshPro dataText;
    protected DatabaseReference reference;
    protected GameManager gameManager;
    protected int areaNumber;
    protected int techniqueNumber;
    protected bool bodyVisibility;
    
    void Start()
    {
        // Initialize Firebase and get the root reference location of the database.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Set the root reference
                reference = FirebaseDatabase.DefaultInstance.RootReference;

                // Get the reference to GameManager
                gameManager = GameManager.instance;
            }
            else
            {
                Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", task.Result));
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
        }
        loadData = false;
    }

    void RetrieveAndSetUserData()
    {
        reference.Child("Users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    // Retrieve user data
                    string name = childSnapshot.Child("name").Value.ToString();
                    float userHeight = float.Parse(childSnapshot.Child("userHeight").Value.ToString());

                    // Update GameManager with retrieved user data
                    gameManager.UserHeight = userHeight;
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
        reference.Child("Users").Child(userId.ToString()).Child("Blocks").Child(blockId.ToString()).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                // Retrieve block data
                areaNumber = int.Parse(snapshot.Child("areaNumber").Value.ToString());
                techniqueNumber = int.Parse(snapshot.Child("techniqueNumber").Value.ToString());
                bodyVisibility = bool.Parse(snapshot.Child("bodyVisibility").Value.ToString());

                dataText.text = "User Id: " + userId + "\n Area Number: " + areaNumber + " (1 = Arm, 2 = Hand)\n Technique Number: " + techniqueNumber + " \n(1 = Rate, 2 = Select, 3 = Dynamic)\n Body Visibility: " + bodyVisibility;

                // Update GameManager with retrieved block data
                gameManager.AreaNumber = areaNumber;
                gameManager.TechniqueNumber = techniqueNumber;
                gameManager.BodyVisibility = bodyVisibility;
            }
            else
            {
                Debug.LogError("Failed to retrieve block data: " + task.Exception);
            }
        });
    }
    
}
