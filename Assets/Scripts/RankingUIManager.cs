using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RankingUIManager : MonoBehaviour
{
    public Text leaderboardText;    

    public void UpdateLeaderboardUI()
    {
        RankingSystem.instance.SortPlayersByScore();
        leaderboardText.text = "";
        for (int i = 0; i < RankingSystem.instance.players.Count; i++)
        {   
            if (i < 10)
            {
                leaderboardText.text += (i + 1) + ". " + RankingSystem.instance.players[i].playerName + ": " + RankingSystem.instance.players[i].score + "\n";
            }
        }
    }

    //public void OnLevelWasLoaded(int level)
    //{
    //    if (SceneManager.GetActiveScene().name == "MainMenu")
    //    {
    //        leaderboardText = GameObject.FindGameObjectWithTag("RankingText").GetComponent<Text>();
    //    }
    //    leaderboardText = GameObject.FindGameObjectWithTag("RankingText").GetComponent<Text>();
    //}
}
