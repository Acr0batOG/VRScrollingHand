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
       
       
    }

    void Update()
    {

    }
  
  
    protected virtual void Scroll(Collider collisionInfo){

    }


}