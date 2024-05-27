using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block {
    public int blockId; //Which user is being tested in what block; used as key
    public int userId; //Id of trial, multiple per user
    public int areaNumber;
    public int techniqueNumber;
    public bool bodyVisibility;
 
    public Block() {
    }

    public Block(int blockId, int userId, int areaNumber, int techniqueNumber, bool bodyVisibility) {
        this.blockId = blockId;
        this.userId = userId;
        this.areaNumber = areaNumber;
        this.techniqueNumber = techniqueNumber;
        this.bodyVisibility = bodyVisibility;
    }
}