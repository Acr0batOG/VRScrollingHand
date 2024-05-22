using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] List<ArmUIController> armUIControllers;
    [SerializeField] private int techniqueNumber = -1;
    [SerializeField] private int areaNumber = -1;

    private int previousTechnique = -1;


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
        //Only check change technique number if it has changed from its previous value
        if(techniqueNumber != previousTechnique){
           
        //Update current object selected as enabled, disable the other objects before doing so.
        switch (techniqueNumber)
            {
                case 1:
                    Disable();
                    armUIControllers[0].gameObject.SetActive(true);
                    break;
                case 2:
                    Disable();
                    armUIControllers[1].gameObject.SetActive(true);
                    break;
                case 3:
                    Disable();
                    armUIControllers[2].gameObject.SetActive(true);
                    break;
            }
            //Otherwise assign previous technique to the current technique number
            previousTechnique = techniqueNumber;
        }
    }
    void Disable(){
        for(int i = 0; i < 3; i++){
            armUIControllers[i].gameObject.SetActive(false);
        }
    }
}

