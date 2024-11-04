using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName;
    public int score;

    public Player(string name, int score)
    {
        playerName = name;
        this.score = score;
    }
}
