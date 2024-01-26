using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class TriviaManager : MonoBehaviour
{
    public Text questionText;
    public Button[] answerButtons; // Suponiendo que tienes 4 botones para las respuestas
    private List<Question> questions = new List<Question>();
    private List<int> questionIndexes = new List<int>(); // Índices de las preguntas que se han mostrado
    private int currentQuestionIndex = -1; // Índice de la pregunta actual

    [Serializable]
    public class Question
    {
        public string question;
        public string[] answers;
        public int correctAnswerIndex;
    }
    [SerializeField] private GameManager m_gameManager = null; // Referencia al GameManager

    void Start()
    {
        m_gameManager = FindObjectOfType<GameManager>();
        LoadQuestionsFromFile("questions.txt");
        ShuffleQuestions();
        //StartTrivia();
    }
    public void StartTrivia()
    {
        ShowNextQuestion();
    }


    void LoadQuestionsFromFile(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);

        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 6)
                {
                    Question question = new Question();
                    question.question = parts[0];
                    question.answers = new string[4];
                    Array.Copy(parts, 1, question.answers, 0, 4);
                    question.correctAnswerIndex = int.Parse(parts[5]);
                    questions.Add(question);
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
        // Genera una lista de índices de preguntas mezclada
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
        currentQuestionIndex++;
        if (currentQuestionIndex >= questionIndexes.Count)
        {
            Debug.Log("End of questions. Restarting...");
            ShuffleQuestions();
            currentQuestionIndex = 0;
        }

        if(questionIndexes.Count > 0)
        {
            int questionIndex = questionIndexes[currentQuestionIndex];
            Question currentQuestion = questions[questionIndex];
            questionText.text = currentQuestion.question;            
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
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerButtons[buttonIndex], answerIndex, currentQuestion.correctAnswerIndex) );
            }
        }
        else
        {
            Debug.LogError("No questions available");
        }
        
    }

    void OnAnswerSelected(Button selectedButton, int answerIndex, int correctAnswerIndex)
    {
        Debug.Log(selectedButton);
        int selectedAnswerIndex = 0;
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] == selectedButton)
            {
                Debug.Log("Boton coincide " + answerButtons[i] + " vs " + selectedButton);
                selectedAnswerIndex = i;
                break;
            }
            Debug.Log("Boton no coincide " + answerButtons[i] + " vs " + selectedButton);
            
        }
        if (answerIndex == correctAnswerIndex)
        {
            Debug.Log("Respuesta Correcta");
            Debug.Log(" Esperada " + correctAnswerIndex + " Recibida: " + selectedAnswerIndex + " O :" + answerIndex);
            Debug.Log(answerButtons[selectedAnswerIndex].GetComponent<Button>());

            StartCoroutine(m_gameManager.GiveAnswerRoutine(selectedButton, true));
        }
        else
        {
            Debug.Log("Respuesta inCorrecta");
            Debug.Log(" Esperada " + correctAnswerIndex + " Recibida: " + selectedAnswerIndex + " O :" + answerIndex);
            Debug.Log(answerButtons[selectedAnswerIndex].GetComponent<Button>());
            StartCoroutine(m_gameManager.GiveAnswerRoutine(selectedButton.GetComponent<Button>(), false));
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
// Método de extensión para mezclar una lista genérica
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
