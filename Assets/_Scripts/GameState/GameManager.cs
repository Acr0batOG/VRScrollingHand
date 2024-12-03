using System.Collections.Generic;
using _Scripts.OldScrollingTypes;
using _Scripts.OptiTrack;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.GameState
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private List<ArmUIController> armUIControllers;
        [SerializeField] private List<CapsuleCollider> armUIDetectors;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private int techniqueNumber = -1;
        [SerializeField] private int areaNumber = -1;
        [SerializeField] private bool bodyVisibility = true;
        [SerializeField] private float userHeight = 1.8f; 
        [SerializeField] private int numberOfItems = 50; // Number of items to populate
        private int selectedItem;
        private int previousTechnique = -1;
        private int previousArea = -1;
        private bool previousBodyVisibility = true;
        private bool initalizeList;
        private bool trackData;
        private bool touchFinished;
        private bool arduinoSelect;
        private GameObject handMesh; //Used to change the visibility of the hand mesh
        private GameObject upperBodyMesh; //Used to change the visibility of the hand mesh
        

        // Property for techniqueNumber
        public int TechniqueNumber
        {
            get { return techniqueNumber; }
            set { techniqueNumber = value; }
        }
        public ScrollRect ScrollRect
        {
            get { return scrollRect; }
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
        public bool ArduinoSelect
        {
            get { return arduinoSelect; }
            set { arduinoSelect = value; }
        }
        public bool TouchFinished
        {
            get { return touchFinished; }
            set { touchFinished = value; }
        }
        public bool InitalizeList
        {
            get { return initalizeList; }
            set { initalizeList = value; }
        }
        public bool TrackData
        {
            get { return trackData; }
            set { trackData = value; }
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
        public int SelectedItem
        {
            get{ return selectedItem; }
            set{ selectedItem = value; }
        }

        public int PreviousArea
        {
            get{ return previousArea; }
            set{ previousArea = value; }
        }
        public int PreviousTechnique
        {
            get{ return previousTechnique; }
            set{ previousTechnique = value; }
        }


        void Start()
        {
            DisableAllArmUIControllers();
            armUIDetectors[0].gameObject.SetActive(true); //Enable the fingertip collider
            handMesh = GameObject.Find("Hand_ply"); //Get hand and body mesh: For changing visibility
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
                DisableAllArmUIControllers();
                //Update technique and area for comparison
                // previousTechnique = techniqueNumber;
                // previousArea = areaNumber;
            }

            if (bodyVisibility != previousBodyVisibility)
            {
                UpdateVisibility(); //Used to update body visibility
                previousBodyVisibility = bodyVisibility;
            }
        }

        public void EnableSelectedController()
        {
                Debug.Log("Controller Enabled");
                    
                int compositeValue = areaNumber * 10 + techniqueNumber;

                // Dictionary to map composite values to their corresponding UI controller index
                Dictionary<int, int> compositeToIndex = new Dictionary<int, int>
                {
                    //Area is the first number 1-3 (Arm, Hand, Other). Technique is next number 1-6
                    //(Static, Point, Dynamic, Point -> Static, Point -> Dynamic, Dynamic One to One)
                    { 11, 0 }, { 12, 1 }, { 13, 2 }, { 14, 3 }, { 15, 4 }, { 16, 5 },
                    { 21, 6 }, { 22, 7 }, { 23, 8 }, { 24, 9 }, { 25, 10 }, { 26, 11 },
                    {31, 12}, {32, 13}
                };

                // Check if the composite value exists in the dictionary
                if (compositeToIndex.TryGetValue(compositeValue, out int index))
                {
                    DisableAllArmUIControllers(); //Disable all other scroll objects
                    armUIControllers[index].gameObject.SetActive(true); //Set the item selected active
                }

                //Set the proper collider.
                Debug.Log("Enabling Controller " + armUIDetectors[0].gameObject);
                armUIDetectors[0].gameObject.SetActive(true); //Enable the fingertip collider
          
        }
        public void DisableAllArmUIControllers()
        {
            Debug.Log("Disable Controllers");
            //Disable every arm controller each time it's called
            touchFinished = false;
            foreach (ArmUIController t in armUIControllers)
            {
                t.gameObject.SetActive(false);
            }
        }

       

        // ReSharper disable Unity.PerformanceAnalysis
        void UpdateVisibility()
        {
            //Used to set body visibility
            bool isVisible = bodyVisibility;
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
            UpdateHeight(); //When user height changes, update body height and change collider size
        }

        // Called when the script is loaded or a value changes
        private void OnValidate()
        {
            NotifyHeightChange(); //When height value changes, update value in other classes
        }
        private void UpdateHeight(){
        //BodyTrackingCalibrationInfo calibrationInfo;
        //calibrationInfo.BodyHeight = userHeight;  //Update body height on the character itself
        }
    }
}
