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
    [SerializeField] protected CapsuleCollider handCollider;
    [SerializeField] protected CapsuleCollider armCollider;
    protected int areaNum;
    protected float armLengthModify = 1.36f;
    protected float handModifyRadius = 1.65f;

    protected virtual void Start()
    {
        gameManager = GameManager.instance;
        areaNum = gameManager.AreaNumber; //Get area being used
        userHeight = gameManager.UserHeight; //Get height of character
        switch (areaNum) //Switch to set the start and end points
        {
            case 1:
                startPoint = GameObject.FindWithTag("Elbow").transform; //Set start point to elbow for arm scrolling
                endPoint = GameObject.FindWithTag("WristOther").transform; //Set to wrist as end point
                break;
            case 2:
                startPoint = GameObject.FindWithTag("Wrist").transform; //Set to wrist as start point
                endPoint = GameObject.FindWithTag("Finger").transform;  //Set fingertip as end point
                break;
        }
        UpdateUserHeight(userHeight); //Set height on start
    }

    public void OnUserHeightChanged(float newUserHeight)
    {
        UpdateUserHeight(newUserHeight);
    }

    protected void UpdateUserHeight(float newUserHeight)
    {
        userHeight = newUserHeight;
        switch (areaNum)
        {
            case 1:
                float armLength = newUserHeight;
                armCollider.height = armLength * armLengthModify;
                break;
            case 2:
                float handLength = newUserHeight / armLengthModify;
                handCollider.height = handLength;
                handCollider.radius = handLength * handModifyRadius; // Have a circle around the hand so it works across the whole hand
                break;
        }
    }

    protected virtual void Scroll(Collider collisionInfo)
    {
        
    }
}
