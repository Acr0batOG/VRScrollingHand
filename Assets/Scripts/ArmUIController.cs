using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ArmUIController : MonoBehaviour
{
    [SerializeField] protected Transform startPoint;
    [SerializeField] protected Transform endPoint;
    [SerializeField] protected ScrollRect scrollableList;
    protected GameManager gameManager;
    [SerializeField] protected TextMeshPro distText;
    [SerializeField] protected TextMeshProUGUI menuText;
    protected float userHeight;
    [SerializeField] protected CapsuleCollider capsuleCollider;
    
    protected int areaNum;
    protected float armModify = 4.5f;
    protected float handModify = 4.25f;
    protected float fingerModify = 5.15f;
    protected float fingertipModify = 3.75f;

    protected void Start()
    {
        gameManager = GameManager.instance;
        areaNum = gameManager.AreaNumber; // Get area being used
        

        switch (areaNum) // Switch to set the start and end points
        {
            case 1:
                startPoint = GameObject.FindWithTag("Elbow").transform; // Arm scrolling
                endPoint = GameObject.FindWithTag("Wrist").transform;
                break;
            case 2:
                startPoint = GameObject.FindWithTag("Wrist").transform; //Hand scrolling
                endPoint = GameObject.FindWithTag("Finger").transform;
                break;
            case 3:
                startPoint = GameObject.FindWithTag("Fingerbase").transform; //Finger scrolling
                endPoint = GameObject.FindWithTag("Fingertip").transform;
                break;
            case 4:
                startPoint = GameObject.FindWithTag("Fingermid").transform; //Fingertip scrolling
                endPoint = GameObject.FindWithTag("Fingertip").transform;
                break;
        }

    
    }

    void Update()
    {
        // Calculate the middle point between startPoint and endPoint
        Vector3 middlePoint = (startPoint.position + endPoint.position) / 2f;
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        userHeight = gameManager.UserHeight; // Get height of character

        switch (areaNum) // Switch to set the start and end points
        {
            case 1: // Arm scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, armModify);
                break;
            case 2: // Hand scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, handModify);
                break;
            case 3: // Arm scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, fingerModify);
                break;
            case 4: // Hand scrolling
                OrientCollider(capsuleCollider, middlePoint, direction, distance, userHeight, fingertipModify);
                break;
        }
    }

    void OrientCollider(CapsuleCollider collider, Vector3 middlePoint, Vector3 direction, float distance, float userHeight, float multiplier)
    {
        collider.center = Vector3.zero; // Reset center to origin
        collider.height = distance * userHeight * multiplier; // Set height based on distance and user height
        collider.transform.position = middlePoint; // Position the collider at the midpoint
        // Set the direction of the collider to Y-axis
        collider.direction = 1; // 0 for X, 1 for Y, 2 for Z

        // Calculate the rotation to align with the direction vector
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, direction);

        // Apply the rotation to the collider
        collider.transform.rotation = rotation;
    }

    

    

    protected virtual void Scroll(Collider collisionInfo)
    {
        // Add scrolling logic here if needed
    }
}
