using System.Collections;
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
    private int m_score;
    //private int puntuacion;
    public Text textoContador;

    // Referencia al TriviaManager
    [SerializeField] private TriviaManager m_triviaManager = null;

    
    public AudioSource m_audioSource;

    public GameObject blockOption;

    public GameObject timerBar;
    private Animator m_anim;

    public float totalTime = 0.0f; // Total time for the quiz
    private float timeLeft; // Time left for the quiz
    private bool timerStarted; // Flag to check if the timer has started
    void Start()
    {
        m_triviaManager = FindObjectOfType<TriviaManager>();
        //m_audioSource = GetComponent<AudioSource>();

        m_score = 0;
        timeLeft = totalTime;
        timerStarted = false;

        m_anim = timerBar.GetComponent<Animator>();

        StartTrivia(); // Inicia la secuencia de preguntas
    }
    public void StartTrivia()
    {
        StartTimer();
        m_triviaManager.StartTrivia(); // Inicia la secuencia de preguntas
        
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
    }

    public void StartTimer()
    {
        timerStarted = true;
    }

    public void EndTimer()   // Do something when the timer ends, such as show the quiz results
    {
        timerStarted = false;

        if (m_audioSource.isPlaying)
            m_audioSource.Stop();

        m_audioSource.clip = m_incorrectSound;

        m_audioSource.Play();

        GameOver();
    }
    public IEnumerator GiveAnswerRoutine(Button optionButton, bool answer)
    {
        Debug.Log("Entre a al handler de respuestas");

        Debug.Log(optionButton.ToString());
        if (m_audioSource.isPlaying)
        {
            m_audioSource.Stop();
            Debug.Log("DeteniendoAudio");
        }
        Debug.Log("Cambiando audio");
        Debug.Log(answer);
        m_audioSource.clip = answer ? m_correctSound : m_incorrectSound;
        Debug.Log("Cambiando color");
        optionButton.GetComponent<Image>().color = answer ? m_correctColor : m_incorrectColor;       
        Debug.Log("Bloqueando interacción");
        blockOption.SetActive(true);

        m_audioSource.Play();

        if (answer)
        {
            Debug.Log("Logica para rta Correcta");
            m_score++;
            textoContador.text = m_score.ToString();

            m_anim.enabled = false;
            timeLeft = 17.5f;

            yield return StartCoroutine(WaitAndNextQuestion());
        }
        else
        {
            Debug.Log("Logica para rta inCorrecta");
            m_anim.enabled = false;

            yield return StartCoroutine(WaitAndGameOver());
        }
    }
    // Corutina para esperar y luego mostrar la siguiente pregunta
    private IEnumerator WaitAndNextQuestion()
    {
        yield return new WaitForSeconds(m_waitTime);
        m_triviaManager.ShowNextQuestion();
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
    public void GameOver()
    {
        SceneManager.LoadScene("MainMenu");
    }

}