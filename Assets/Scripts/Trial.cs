using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trial {
    public int blockId; //Which user is being tested in what block; used as key
    public int trialId; //Id of trial, multiple per user
    public int numOfTrials;
    public float timeToComplete; //During trial time to take and proper selection
    public bool correctSelection;
    public int userId;
 
    public Trial() {
    }

    public Trial(int blockId, int trialId, int numOfTrials, float timeToComplete, bool correctSelection, int userId) {
        this.blockId = blockId;
        this.trialId = trialId;
        this.numOfTrials = numOfTrials;
        this.timeToComplete = timeToComplete;
        this.correctSelection = correctSelection;
        this.userId = userId;
    }
}

