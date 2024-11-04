using UnityEngine;
using TMPro;

public class GameManagerr : MonoBehaviour
{
    public static GameManagerr Instance;
    [SerializeField] TextMeshProUGUI textBox;
    [SerializeField] TextMeshProUGUI printBox;
    
    GameManager gameManager;

    private void Start()
    {
        Instance = this;
        printBox.text = "";
        textBox.text = "";

    
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    public void DeleteLetter()
    {
        if(textBox.text.Length != 0) {
            textBox.text = textBox.text.Remove(textBox.text.Length - 1, 1);
        }
    }

    public void AddLetter(string letter)
    {
        textBox.text = textBox.text + letter;
    }

    public void SubmitWord()
    {
        printBox.text = textBox.text;
        textBox.text = "";
        RankingSystem.instance.AddPlayer(printBox.text, gameManager.m_score);
        RankingSystem.instance.DisplayRankings();
        gameManager.showRanking();
        // Debug.Log("Text submitted successfully!");
    }
}
