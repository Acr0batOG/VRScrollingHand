using System.Collections.Generic;
using UnityEngine;
using static OVRPlugin;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] List<ArmUIController> armUIControllers;
    [SerializeField] private int techniqueNumber = -1;
    [SerializeField] private int areaNumber = -1;
    [SerializeField] private bool bodyVisibility = true;
    [SerializeField] private float userHeight = 1.8f; 
    [SerializeField] private int numberOfItems = 50; // Number of items to populate

    private int previousTechnique = -1;
    private int previousArea = -1;
    private bool previousBodyVisibility = true;
    
    private GameObject handMesh; //Used to change the visibility of the hand mesh
    private GameObject upperBodyMesh; //Used to change the visibility of the hand mesh

    // Property for techniqueNumber
    public int TechniqueNumber
    {
        get { return techniqueNumber; }
        set { techniqueNumber = value; }
    }

    // Property for areaNumber
    public int AreaNumber
    {
        get { return areaNumber; }
        set { areaNumber = value; }
    }

    // Property for bodyVisibility
    public bool BodyVisibility
    {
        get { return bodyVisibility; }
        set { bodyVisibility = value; }
    }

    // Property for userHeight with notification logic
    public float UserHeight
    {
        get { return userHeight; }
        set {
                userHeight = value;
                NotifyHeightChange();
            }
    }
    public int NumberOfItems
    {
        get { return numberOfItems; }
        set { numberOfItems = value; }
    }

    // Start is called before the first frame update
    void Start()
    {
        Disable();
        handMesh = GameObject.Find("Hand_ply");
        upperBodyMesh = GameObject.Find("upper_body_ply");

        if (handMesh == null || upperBodyMesh == null)
        {
            return;
        }
        UpdateVisibility();
        UpdateHeight();
    }

    // Update is called once per frame
    void Update()
    {
        if (techniqueNumber != previousTechnique || areaNumber != previousArea)
        {
            int compositeValue = areaNumber * 10 + techniqueNumber;
            switch (compositeValue) //Switch case to enable the capsule needed
            {
                case 11: //Area 1 = Arm, Technique 1 = Rate Control
                    Disable();
                    armUIControllers[0].gameObject.SetActive(true);
                    break;
                case 12: // Technique 2 = Point Select (Volume Scroll)
                    Disable();
                    armUIControllers[1].gameObject.SetActive(true);
                    break;
                case 13: // Technique 3 = Dynamic Scroll
                    Disable();
                    armUIControllers[2].gameObject.SetActive(true);
                    break;
                case 14: // Technique 4 = Point Select then Rate Control
                    Disable();
                    armUIControllers[3].gameObject.SetActive(true);
                    break;
                case 15: // Technique 5 = Point Select then Dynamic Scroll
                    Disable();
                    armUIControllers[4].gameObject.SetActive(true);
                    break;
                case 21: //Area 2 = Hand
                    Disable();
                    armUIControllers[5].gameObject.SetActive(true);
                    break;
                case 22:
                    Disable();
                    armUIControllers[6].gameObject.SetActive(true);
                    break;
                case 23:
                    Disable();
                    armUIControllers[7].gameObject.SetActive(true);
                    break;
                case 24:
                    Disable();
                    armUIControllers[8].gameObject.SetActive(true);
                    break;
                case 25:
                    Disable();
                    armUIControllers[9].gameObject.SetActive(true);
                    break;
                case 31: //Area 3 =  Finger
                    Disable();
                    armUIControllers[10].gameObject.SetActive(true);
                    break;
                case 32:
                    Disable();
                    armUIControllers[11].gameObject.SetActive(true);
                    break;
                case 33:
                    Disable();
                    armUIControllers[12].gameObject.SetActive(true);
                    break;
                case 34:
                    Disable();
                    armUIControllers[13].gameObject.SetActive(true);
                    break;
                case 35:
                    Disable();
                    armUIControllers[14].gameObject.SetActive(true);
                    break;
                case 41: //Area 4 = Fingertip
                    Disable();
                    armUIControllers[15].gameObject.SetActive(true);
                    break;
                case 42:
                    Disable();
                    armUIControllers[16].gameObject.SetActive(true);
                    break;
                case 43:
                    Disable();
                    armUIControllers[17].gameObject.SetActive(true);
                    break;
                case 44:
                    Disable();
                    armUIControllers[18].gameObject.SetActive(true);
                    break;
                case 45:
                    Disable();
                    armUIControllers[19].gameObject.SetActive(true);
                    break;
            }
            previousTechnique = techniqueNumber;
            previousArea = areaNumber;
        }

        if (bodyVisibility != previousBodyVisibility)
        {
            UpdateVisibility();
            previousBodyVisibility = bodyVisibility;
        }
                    
    }

    void Disable()
    {
        for (int i = 0; i < armUIControllers.Count; i++)
        {
            armUIControllers[i].gameObject.SetActive(false);
        }
    }

    void UpdateVisibility()
    {
        bool isVisible = bodyVisibility == true;
        SetMeshVisibility(handMesh, isVisible); //Set visibility of hand and arm
        SetMeshVisibility(upperBodyMesh, isVisible);
    }

    void SetMeshVisibility(GameObject meshObject, bool isVisible)
    {
        if (meshObject != null)
        {
            Renderer meshRenderer = meshObject.GetComponent<Renderer>(); //Get component and set visibility
            if (meshRenderer != null)
            {
                meshRenderer.enabled = isVisible;
            }
        }
    }

    void NotifyHeightChange()
    {
        UpdateHeight();
        
    }

    // Called when the script is loaded or a value changes
    private void OnValidate()
    {
        NotifyHeightChange(); //When height value changes, update value
    }
    private void UpdateHeight(){
        BodyTrackingCalibrationInfo calibrationInfo; //On height change. Update body height info
        calibrationInfo.BodyHeight = userHeight; //Update body height
    }
}
