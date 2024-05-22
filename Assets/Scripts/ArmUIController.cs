using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class ArmUIController : MonoBehaviour
{
    [SerializeField] protected Transform startPoint; //Get the start and end point of objects on the users arm
    [SerializeField] protected Transform endPoint;
    [SerializeField] protected ScrollRect scrollableList; //The actual list we will be scrolling in unity. 
    protected GameManager gameManager;
    
    //[SerializeField] private float scaleFactor = 2f;
    // private int itemsInList = 10;
    [SerializeField] protected TextMeshPro distText; //Used to update text on the Dev canvas. And it tells us the current values of Vector3's being used

    [SerializeField] protected TextMeshProUGUI menuText; //Used to detect when a collision is happening. Also updates text on the Dev canvas
    protected void Start()
    {   
        gameManager = GameManager.instance;
        int areaNum = gameManager.AreaNumber; //Load different start and end points based on areaNumber
        switch(areaNum) //Switch to change which area is selected
        {   
            case 1:
                startPoint = GameObject.FindWithTag("Elbow").transform; //Else just make sure original start and end points assigned
                endPoint = GameObject.FindWithTag("WristOther").transform;
                break;
            case 2:
                startPoint = GameObject.FindWithTag("Wrist").transform; //If area num 2 selected, change start and end points to the hand
                endPoint = GameObject.FindWithTag("Finger").transform;
                break;            
        }
        
       
    }

    void Update()
    {

    }
  
  
    protected virtual void Scroll(Collider collisionInfo){

    }


}