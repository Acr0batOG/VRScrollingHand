using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User {
    public int userId;
    public string name;
    public float userHeight;
 
    public User() {
    }

    public User(int userId, string name, float userHeight) {
        this.userId = userId;
        this.name = name;
        this.userHeight = userHeight;
    }
}
