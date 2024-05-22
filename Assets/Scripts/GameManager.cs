using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] List<ArmUIController> armUIControllers;
    [SerializeField] private int techniqueNumber = -1;
    [SerializeField] private int areaNumber = -1;

    private int previousTechnique = -1;
    private int previousArea = -1;

    // Property for techniqueNumber
    public int TechniqueNumber
    {
        get { return techniqueNumber; }
    }

    // Property for areaNumber
    public int AreaNumber
    {
        get { return areaNumber; }
    }

    // Start is called before the first frame update
    void Start()
    {
        //Set all collision detection objects as disabled
        Disable();
        
    }

    // Update is called once per frame
   void Update()
    {
        // Only check and update technique or area if they have changed
        if (techniqueNumber != previousTechnique || areaNumber != previousArea)
        {
            // Composite value for the switch case
            int compositeValue = areaNumber * 10 + techniqueNumber;

            // Update current object selected as enabled, disable the other objects before doing so.
            switch (compositeValue)
            {
                case 11:
                    Disable();
                    armUIControllers[0].gameObject.SetActive(true);
                    break;
                case 12:
                    Disable();
                    armUIControllers[1].gameObject.SetActive(true);
                    break;
                case 13:
                    Disable();
                    armUIControllers[2].gameObject.SetActive(true);
                    break;
                case 21:
                    Disable();
                    armUIControllers[3].gameObject.SetActive(true);
                    break;
                case 22:
                    Disable();
                    armUIControllers[4].gameObject.SetActive(true);
                    break;
                case 23:
                    Disable();
                    armUIControllers[5].gameObject.SetActive(true);
                    break;
            }

            // Update previous values
            previousTechnique = techniqueNumber;
            previousArea = areaNumber;
        }
    }
    void Disable(){
        for(int i = 0; i < armUIControllers.Count; i++){
            armUIControllers[i].gameObject.SetActive(false);
        }
    }
}

