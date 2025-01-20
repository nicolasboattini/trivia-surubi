using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections;
using UnityEngine.Networking;
using MongoDB.Bson.Serialization.Attributes;


public class TriviaManager : MonoBehaviour
{
    public Text questionText;
    public Text currentQuestion;
    public Text currentCat;
    [BsonSerializer(typeof(Button[]))]
    public Button[] answerButtons; // Suponiendo que tienes 4 botones para las respuestas
    [BsonSerializer(typeof(List<Question>))]
    public List<Question> questions = new List<Question>();
    [BsonSerializer(typeof(List<int>))]
    public List<int> questionIndexes = new List<int>(); // Índices de las preguntas que se han mostrado
    public int currentQuestionIndex = -1; // Índice de la pregunta actual
    [SerializeField] 
    private GameManager m_gameManager = null; // Referencia al GameManager
    private MongoClient mongoClient;
    private IMongoDatabase database;
    private void Start()
    {
        BsonClassMap.RegisterClassMap<Question>(cm =>
        {
            cm.AutoMap(); // Realiza el mapeo automático de las propiedades
            cm.MapMember(c => c._id).SetElementName("_id"); // Mapea el campo '_id' si es necesario
            cm.MapMember(c => c.question).SetElementName("question");
            cm.MapMember(c => c.answers).SetElementName("answers");
            cm.MapMember(c => c.correctAnswerIndex).SetElementName("correctAnswerIndex");
            cm.MapMember(c => c.catName).SetElementName("catName");
        });
        m_gameManager = FindObjectOfType<GameManager>();
        InitConnection();
    }
    private void InitConnection()
    {
        string connectionString = "mongodb://nicolas123:aHm1FcLmD01heZd5@leadmanagement-shard-00-00.eefur.mongodb.net:27017,leadmanagement-shard-00-01.eefur.mongodb.net:27017,leadmanagement-shard-00-02.eefur.mongodb.net:27017/?ssl=true&replicaSet=atlas-i7v84r-shard-0&authSource=admin&retryWrites=true&w=majority&appName=LeadManagement"; // Usa variables de entorno o un archivo seguro

        var settings = MongoClientSettings.FromConnectionString(connectionString);
        mongoClient = new MongoClient(connectionString);
        database = mongoClient.GetDatabase("TriviaGame");
    }
    public void StartTrivia(List<string> selectedCategories)
    {
        StartCoroutine(LoadQuestionsFromDatabase(selectedCategories));
        ShuffleQuestions();
        ShowNextQuestion();
    }
    public void LoadQuestionsFromFileDesktop(string fileName, List<string> selectedCategories)
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
            }
        }
        else
        {
            Debug.LogError("Questions file not found: " + filePath);
        }
    }
    public IEnumerator LoadQuestionsFromDatabase(List<string> selectedCategories)
    {
        var collection = database.GetCollection<Question>("Questions");
        // Filtro para las categorías seleccionadas
        var filter = Builders<Question>.Filter.In(q => q.catName, selectedCategories);

        // Obtener las preguntas de la base de datos
        var fetchedQuestions = collection.Find(filter).ToListAsync().Result;

        questions.Clear();
        questions.AddRange(fetchedQuestions);
        Debug.Log(fetchedQuestions[0].question);
        Debug.Log(questions[0]);
        yield return null;
    }
    public IEnumerator LoadQuestionsFromFileWebGL(List<string> selectedCategories)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "questions.txt");
        UnityWebRequest request = UnityWebRequest.Get(filePath);

        // Enviar la solicitud y esperar la respuesta
        yield return request.SendWebRequest();

        // Verificar errores de conexi�n
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error al cargar el archivo: " + request.error);
            yield break;
        }

        // Leer el contenido del archivo
        Debug.Log($"Request {request} {request.downloadHandler.text}");
        string[] lines = request.downloadHandler.text.Split('\n');

        // Procesar cada l�nea del archivo
        foreach (string line in lines)
        {
            string[] parts = line.Split(';');
            if (parts.Length >= 1)
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
                    Debug.Log($"Pregunta cargada con exito {question.question}");
                }
            }

        }
        Debug.Log("Entrando por webgl shuffle");
        ShuffleQuestions();
    }
    void ShuffleQuestions()
    {
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
            button.GetComponent<Image>().color = Color.white;
        }
        if (AllQuestions())
        {
            Debug.Log("Todas las preguntas respondidas");
            m_gameManager.showWinnerScreen(true);
            StartCoroutine(m_gameManager.WaitAndEnd(5f));
            return;
        }

        if (!m_gameManager.checkScore()) currentQuestion.text = ("Pregunta " + (currentQuestionIndex + 1).ToString() + " / " + questionIndexes.Count.ToString());
        int questionIndex = questionIndexes[currentQuestionIndex];
        Question currentQuestionData = questions[questionIndex];

        questionText.text = currentQuestionData.question;
        currentCat.text = currentQuestionData.catName;

        List<int> answerIndexes = new List<int>() { 0, 1, 2, 3 };
        answerIndexes.Shuffle();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = answerIndexes[i];
            answerButtons[i].GetComponentInChildren<Text>().text = currentQuestionData.answers[answerIndex];
            answerButtons[i].onClick.RemoveAllListeners();
            int buttonIndex = i;
            answerButtons[i].onClick.AddListener(() =>
                OnAnswerSelected(answerButtons[buttonIndex], answerIndex, currentQuestionData));
        }
    }
    void OnAnswerSelected(Button selectedButton, int answerIndex, Question qst)
    {
        if (answerIndex == qst.correctAnswerIndex)
        {
            StartCoroutine(m_gameManager.GiveAnswerRoutine(selectedButton, true));
        }
        else
        {
            foreach (Button btn in answerButtons)
            {
                if (btn.GetComponentInChildren<Text>().text == qst.answers[qst.correctAnswerIndex])
                {
                    m_gameManager.ShowCorrectOnFail(btn);
                }
            }
            StartCoroutine(m_gameManager.GiveAnswerRoutine(selectedButton, false));
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
