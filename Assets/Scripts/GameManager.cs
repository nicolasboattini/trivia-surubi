using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [SerializeField] public AudioClip m_correctSound = null;
    [SerializeField] public AudioClip m_incorrectSound = null;
    [SerializeField] private Color m_correctColor = Color.black;
    [SerializeField] private Color m_incorrectColor = Color.black;
    [SerializeField] private float m_waitTime = 0.0f;
    [SerializeField] private TriviaManager m_triviaManager = null;
    // Añadir una variable para almacenar las categorías seleccionadas
    private List<string> selectedCategories = new List<string>();
    public int m_score;
    public int? scoreLimit;
    public Text textoContador;
    public AudioSource m_audioSource;
    public GameObject blockOption;
    public GameObject timerBar;
    public GameObject winnerScreen;
    public GameObject rankingScreen;
    private Animator m_anim;
    public float totalTime = 0.0f; // Total time for the quiz
    private float timeLeft; // Time left for the quiz
    private bool timerStarted; // Flag to check if the timer has started
    public RankingUIManager rankingUI;
    void Start()
    {
        SetSelectedCategories();
        m_triviaManager = FindObjectOfType<TriviaManager>();
        m_score = 0;
        timeLeft = totalTime;
        timerStarted = false;
        m_anim = timerBar.GetComponent<Animator>();
        StartTrivia(); // Inicia la secuencia de preguntas
        //rankingUI.OnLevelWasLoaded(1);
    }
    public void SetSelectedCategories()
    {
        // Recuperar las categorías seleccionadas desde PlayerPrefs
        string categoriesString = PlayerPrefs.GetString("SelectedCategories");
        string[] categoriesArray = categoriesString.Split(',');

        // Convertir el array de strings en una lista de strings
        selectedCategories = new List<string>(categoriesArray);
    }
    public void StartTrivia()
    {
        StartTimer();
        m_triviaManager.StartTrivia(selectedCategories); // Inicia la secuencia de preguntas
    }
    void Update()
    {
        if (timerStarted)
        {
            timeLeft -= Time.deltaTime; // Decrement time left
            if (timeLeft <= 0.0f)
            {
                EndTimer(); // End timer when time is up
            }
        }
        if (PlayerPrefs.HasKey("ScoreLimit") && PlayerPrefs.GetInt("ScoreLimit") != 0)
        {
            if (scoreLimit == null)
            {
                scoreLimit = PlayerPrefs.GetInt("ScoreLimit");
                Debug.Log("Se asigno un limite");
            }
            
            if (checkScore())
            {
                timerStarted = false;
                showWinnerScreen(false);
                StartCoroutine(WaitAndEnd(5f));
            }
        }

    }
    public bool checkScore()
    {
        return m_score >= scoreLimit;
    }
    public IEnumerator WaitAndEnd(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        EndTimer();
    }
    public void StartTimer()
    {
        timerStarted = true;
    }
    public void EndTimer()   // Do something when the timer ends, such as show the quiz results
    {
        timerStarted = false;
        StartCoroutine(WaitAndShowKeyboard());
    }
    public void showWinnerScreen(bool val)
    {
        if (val)
        {
            winnerScreen.SetActive(true);
        }
    }
    public IEnumerator GiveAnswerRoutine(Button optionButton, bool answer)
    {
        if (m_audioSource.isPlaying) m_audioSource.Stop();        
        m_audioSource.clip = answer ? m_correctSound : m_incorrectSound;
        optionButton.GetComponent<Image>().color = answer ? m_correctColor : m_incorrectColor;       
        blockOption.SetActive(true);
        m_audioSource.Play();
        if (answer)
        {
            m_score++;
            textoContador.text = m_score.ToString();
            m_anim.enabled = false;
            timeLeft = 17.5f;
            yield return StartCoroutine(WaitAndNextQuestion());
        }
        else
        {
            m_anim.enabled = false;
            yield return StartCoroutine(WaitAndShowKeyboard());
        }
    }
    public void ShowCorrectOnFail(Button optionButton)
    {
        optionButton.GetComponent<Image>().color = m_correctColor;
    }
    // Corutina para esperar y luego mostrar la siguiente pregunta
    private IEnumerator WaitAndNextQuestion()
    {
        yield return new WaitForSeconds(m_waitTime);
        if (!winnerScreen.activeSelf) m_triviaManager.ShowNextQuestion();
        blockOption.SetActive(false);
        m_anim.Rebind();
        m_anim.Update(0f);
        m_anim.enabled = true;
    }
    // Corutina para esperar y luego mostrar el juego terminado
    private IEnumerator WaitAndGameOver()
    {
        yield return new WaitForSeconds(m_waitTime);
        m_triviaManager.GameOver();
        blockOption.SetActive(false);
    }

    private IEnumerator WaitAndShowKeyboard()
    {
        if (m_audioSource != null)
        {
            m_audioSource.PlayOneShot(m_incorrectSound);
        }
        yield return new WaitForSeconds(m_waitTime);
        submitName();
        blockOption.SetActive(false);
    }

    public void GameOver()
    {
        PlayerPrefs.DeleteKey("SelectedCategories");
        SceneManager.LoadScene("MainMenu");
    }
    private void OnApplicationQuit()
    {
        // Limpia todas las claves de PlayerPrefs al salir de la aplicación
        Debug.Log("Limpiando prefs");
        PlayerPrefs.DeleteAll();
    }
    private void submitName()
    {
        showWinnerScreen(true);
    }

    public void showRanking()
    {
        rankingUI.UpdateLeaderboardUI();
        showWinnerScreen(false);
        rankingScreen.SetActive(true);
    }
}