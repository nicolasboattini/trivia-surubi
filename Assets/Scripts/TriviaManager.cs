using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TriviaManager : MonoBehaviour
{
    public Text questionText;
    public Text currentQuestion;
    public Text currentCat;
    public Button[] answerButtons; // Suponiendo que tienes 4 botones para las respuestas
    public List<Question> questions = new List<Question>();
    public List<int> questionIndexes = new List<int>(); // �ndices de las preguntas que se han mostrado
    public int currentQuestionIndex = -1; // �ndice de la pregunta actual
    [Serializable]
    public class Question
    {
        public string question;
        public string[] answers;
        public int correctAnswerIndex;
        public string catName;
    }
    [SerializeField] private GameManager m_gameManager = null; // Referencia al GameManager
    void Start()
    {
        m_gameManager = FindObjectOfType<GameManager>();
        
    }
    public void StartTrivia(List<string> selectedCategories)
    {
        LoadQuestionsFromFile("questions.txt", selectedCategories);
        ShuffleQuestions();
        ShowNextQuestion();
    }
    void LoadQuestionsFromFile(string fileName, List<string> selectedCategories)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length == 7)
                {
                    string category = parts[0];
                    if (selectedCategories.Contains(category))
                    {
                        Question question = new Question();
                        question.question = parts[1];
                        question.answers = new string[4];
                        Array.Copy(parts, 2, question.answers, 0, 4);
                        question.correctAnswerIndex = int.Parse(parts[6]);
                        question.catName = category;
                        questions.Add(question);
                    }
                }
                else
                {
                    Debug.LogWarning("Invalid question format: " + line);
                }
            }
        }
        else
        {
            Debug.LogError("Questions file not found: " + filePath);
        }
    }
    void ShuffleQuestions()
    {
        // Genera una lista de �ndices de preguntas mezclada
        questionIndexes.Clear();
        for (int i = 0; i < questions.Count; i++)
        {
            questionIndexes.Add(i);
        }
        questionIndexes.Shuffle();
    }
    public void ShowNextQuestion()
    {
        foreach (Button button in answerButtons)
        {
            button.GetComponent<Image>().color = Color.white; // Restablecer a color blanco o el color original
        }
        if (AllQuestions())
        {
            Debug.Log("Todas las respuestas respondidas");
            m_gameManager.showWinnerScreen(true);
            StartCoroutine(m_gameManager.WaitAndEnd(5f));
            return;
        }
        else
        {
            if (!m_gameManager.checkScore()) currentQuestion.text = ("Pregunta " + (currentQuestionIndex + 1).ToString() + " / " + questionIndexes.Count.ToString());
            if (questionIndexes.Count > 0)
            {
                int questionIndex = questionIndexes[currentQuestionIndex];
                Question currentQuestion = questions[questionIndex];
                questionText.text = currentQuestion.question;
                currentCat.text = currentQuestion.catName;
                // Mezclar el orden de las respuestas
                List<int> answerIndexes = new List<int>() { 0, 1, 2, 3 };
                answerIndexes.Shuffle();
                Debug.Log("AnswerButton lenght: " + answerButtons.Length);
                for (int i = 0; i < answerButtons.Length; i++)
                {
                    int answerIndex = answerIndexes[i];
                    answerButtons[i].GetComponentInChildren<Text>().text = currentQuestion.answers[answerIndex];
                    answerButtons[i].onClick.RemoveAllListeners();
                    int buttonIndex = i;
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerButtons[buttonIndex], answerIndex, currentQuestion));
                }
            }
            else
            {
                Debug.LogError("No questions available");
            }
        }
        
    }
    void OnAnswerSelected(Button selectedButton, int answerIndex, Question qst)
    {
        Debug.Log(selectedButton);
        int selectedAnswerIndex = 0;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == selectedButton)
            {
                selectedAnswerIndex = i;
                break;
            }
        }
        if (answerIndex == qst.correctAnswerIndex)
        {
            StartCoroutine(m_gameManager.GiveAnswerRoutine(selectedButton, true));
        }
        else
        {
            if (PlayerPrefs.GetInt("ModoEvento") is 0 )
            {
                foreach (Button btn in answerButtons)
                {
                    if (btn.GetComponentInChildren<Text>().text == qst.answers[qst.correctAnswerIndex])
                    {
                        m_gameManager.ShowCorrectOnFail(btn);
                    }
                }
            }
            StartCoroutine(m_gameManager.GiveAnswerRoutine(selectedButton.GetComponent<Button>(), false));
        }
    }
    public void GameOver()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public bool AllQuestions()
    {
        currentQuestionIndex++;
        return currentQuestionIndex >= questionIndexes.Count;
    }
}
// M�todo de extensi�n para mezclar una lista gen�rica
public static class ListExtensions
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
