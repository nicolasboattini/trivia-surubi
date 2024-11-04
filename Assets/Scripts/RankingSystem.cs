using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class RankingSystem : MonoBehaviour
{
    public List<Player> players = new List<Player>();
    public static RankingSystem instance;
    private bool flag = true;

    // Add player scores
    public void AddPlayer(string playerName, int score)
    {
        players.Add(new Player(playerName, score));
    }

    // Sort the players by score in descending order
    public void SortPlayersByScore()
    {
        players = players.OrderByDescending(player => player.score).ToList();
    }

    // Display rankings
    public void DisplayRankings()
    {
        SortPlayersByScore();
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log((i + 1) + ". " + players[i].playerName + ": " + players[i].score);
        }
    }

    void Start()
    {
        if (flag)
        {
            AddPlayer("Kaiba", 10);
            AddPlayer("Mateo", 5);
            AddPlayer("Facu", 3);

            flag = false;
        }
        
        DisplayRankings();
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

}
