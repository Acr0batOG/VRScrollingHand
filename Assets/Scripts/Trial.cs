using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trial { //Object for database insertion
    public int blockId; //Which user is being tested in what block; used as key
    public int trialId; //Id of trial, multiple per user
    public float timeToComplete; //During trial time to take and proper selection
    public bool correctSelection;
    public int userId;
    public int areaNumber;
    public int techniqueNumber;
    public int selectedItem;
    public int correctItem;
    public float itemLocation;
    public float distanceFromItem;
    public Trial() {
    }

    public Trial(int userId, int blockId, int trialId, float timeToComplete, bool correctSelection, int areaNumber, int techniqueNumber, int selectedItem, int correctItem, float itemLocation, float distanceFromItem) {
        this.userId = userId;
        this.blockId = blockId;
        this.trialId = trialId;
        this.timeToComplete = timeToComplete;
        this.correctSelection = correctSelection;  
        this.areaNumber = areaNumber;
        this.techniqueNumber = techniqueNumber;
        this.selectedItem = selectedItem;
        this.correctItem = correctItem;
        this.itemLocation = itemLocation;
        this.distanceFromItem = distanceFromItem;

    }
}

