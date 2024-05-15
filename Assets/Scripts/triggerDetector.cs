using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;

public class TriggerDetector : MonoBehaviour
{
    // Called when another collider enters this trigger collider
    [SerializeField] private Transform elbowObject;
    [SerializeField] private Transform wristObject;
    [SerializeField] private float scrollFactor = 1000f;
   public GameObject fingerGameObject;
   public TextMeshProUGUI textComponent; // Assign the Text component in the Unity Editor
   public TextMeshPro distText;

    private float listLength;
    private Vector3 basePosition; //Position to hold last spot the list was held in


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
         if (textComponent != null)
            {
                textComponent.text = other.gameObject.tag;
                distText.text = "Enter";
                
            }
            else
            {
                Debug.LogWarning("Text component not assigned!");
            }
        //basePosition = other.transform.localScale;
        SetScrolling(other, basePosition);
    }
    

    private void OnTriggerStay(Collider other)
    {
        SetScrolling(other, basePosition);
    }

    private void OnTriggerExit(Collider other)
    {   
         Debug.Log("Another collider exited the trigger!");
        if (textComponent != null)
            {
                textComponent.text = "Outside Trigger";
                distText.text = "Exit";
            }
        //basePosition = other.transform.localScale;
    }  

    // Called when another collider stays within this trigger collider


    private void SetScrolling(Collider collision, Vector3 pos){
        if (!collision.gameObject.CompareTag("Detector")) return;
        
        Vector3 contactPoint = collision.ClosestPoint(elbowObject.position);
        Vector3 middlePoint = (wristObject.position + elbowObject.position) / 2f;
        float distanceFromStart = (contactPoint - elbowObject.position).magnitude;
        float distanceFromEnd = (contactPoint - wristObject.position).magnitude;
       
       int polarity = distanceFromStart < distanceFromEnd ? -1 : 1;
       
       scrollFactor += (contactPoint - middlePoint).magnitude/listLength*polarity*5;
       // TODO: change this so it interacts with scrolling interactableCube.localScale = scaleFactor * baseScale;

        // Update the distance text
        distText.text = "Contact:" + contactPoint;

    }
}