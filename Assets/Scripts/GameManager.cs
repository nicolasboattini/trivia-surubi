using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour
{
    [SerializeField] public AudioClip m_correctSound = null;
    [SerializeField] public AudioClip m_incorrectSound = null;
    [SerializeField] private Color m_correctColor = Color.black;
    [SerializeField] private Color m_incorrectColor = Color.black;
    [SerializeField] private float m_waitTime = 0.0f;
    private int puntuacion;
    public Text textoContador;

    private QuizDB m_quizDB = null;
    private QuizUI m_quizUI = null;
    public AudioSource m_audioSource = null;

    public GameObject blockOption;

    public GameObject timerBar;
    Animator anim;

    public float totalTime = 0.0f; // Total time for the quiz
    private float timeLeft; // Time left for the quiz
    private bool timerStarted; // Flag to check if the timer has started

    public void Start()
    {
        m_quizDB = GameObject.FindObjectOfType<QuizDB>();
        m_quizUI = GameObject.FindObjectOfType<QuizUI>();
        m_audioSource = GetComponent<AudioSource>();

        puntuacion = 0;

        timeLeft = totalTime;
        timerStarted = false;

        anim = timerBar.GetComponent<Animator>();

        StartTimer();
        NextQuestion();
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

    public void NextQuestion()
    {
        m_quizUI.Construct(m_quizDB.GetRandom(), GiveAnswer);
    }

    public void GiveAnswer(OptionButton optionButton)
    {
        StartCoroutine(GiveAnswerRoutine(optionButton));
    }

    public IEnumerator GiveAnswerRoutine(OptionButton optionButton)
    {
        if(m_audioSource.isPlaying)
            m_audioSource.Stop();

        m_audioSource.clip = optionButton.Option.correct ? m_correctSound : m_incorrectSound;
        optionButton.SetColor(optionButton.Option.correct ? m_correctColor : m_incorrectColor);

        blockOption.SetActive(true);

        m_audioSource.Play();

        if(optionButton.Option.correct) {
            puntuacion = puntuacion + 1;
            textoContador.text = puntuacion.ToString();

            anim.enabled = false;

            timeLeft = 17.5f;

            yield return new WaitForSeconds(m_waitTime);

            NextQuestion();
            blockOption.SetActive(false);
            anim.Rebind();
            anim.Update(0f);
            anim.enabled = true;
        }
        else
        {
            anim.enabled = false;

            yield return new WaitForSeconds(m_waitTime);

            GameOver();
            blockOption.SetActive(false);
        }
    }

    public void GameOver()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
