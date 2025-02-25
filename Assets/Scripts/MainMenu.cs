using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class MainMenu : MonoBehaviour
{
    public Toggle[] categoryToggles;
    private bool allSelected = false;
    [SerializeField] private GameObject configPanel;
    private int scoreLimit = 0;
    public Text limitUI;
    public bool evento = false;
    public GameObject togglePrefab;
    public Transform toggleParent;
    public Toggle eventoMode;
    public Dropdown fileDropdown;
    public GameObject fileSelector;
    private string FILE_NAME = "questions.txt";
    private bool fileChanged;
    private const string selectedCategoriesKey = "SelectedCategories";

    private void Start()
    {
        if (PlayerPrefs.HasKey("FILE_NAME"))
        {
            FILE_NAME = PlayerPrefs.GetString("FILE_NAME");
        }
#if !UNITY_WEBGL
        Debug.Log("Entrando por Desktop");
        PopulateDropdown();
        fileDropdown.onValueChanged.AddListener(index => OnFileSelected(index));
        GenerateCategoryTogglesDesktop();
        toggleListeners();
        
#else
        Debug.Log("Entrando por webgl");
        GenerateCategoryTogglesWebGL();
#endif
    }
    public void toggleListeners()
    {
        for (int i = 0; i < categoryToggles.Length; i++)
        {
            int index = i;
            categoryToggles[i].onValueChanged.AddListener((value) => UpdateToggleColor(categoryToggles[index], value));
            // Establecer el estado inicial como false sin invocar onValueChanged
            categoryToggles[i].SetIsOnWithoutNotify(false);
            // Configura el color inicial
            UpdateToggleColor(categoryToggles[i], false);
        }
    }
    public void PopulateDropdown()
    {
        List<string> fileNames = new();
        string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*.txt"); // Solo archivos .txt

        foreach (string file in files)
        {
            fileNames.Add(Path.GetFileName(file)); // Guardamos solo el nombre del archivo
        }
        fileDropdown.ClearOptions();
        fileDropdown.AddOptions(fileNames);
    }
    public void OnFileSelected(int index)
    {
        string selectedFile = fileDropdown.options[index].text;
        Debug.Log("Archivo seleccionado: " + selectedFile);
        FILE_NAME = selectedFile;
        fileChanged = true;
        SaveFileName();
    }

    public void GenerateCategoryTogglesDesktop()
    {
        List<Toggle> togglesList = new List<Toggle>(); // Usamos una lista temporal para agregar los toggles
        HashSet<string> uniqueCategories = new HashSet<string>(); // Usamos un HashSet para almacenar las categorías únicas

        string filePath = Path.Combine(Application.streamingAssetsPath, FILE_NAME);
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(';');
                if (parts.Length >= 1)
                {
                    string category = parts[0];
                    if (!uniqueCategories.Contains(category))
                    {
                        // Si la categoría no existe, creamos el toggle y lo agregamos a la lista temporal
                        Toggle newToggle = CreateToggle(category);
                        togglesList.Add(newToggle);
                        uniqueCategories.Add(category); // Agregamos la categoría al HashSet para evitar duplicados
                    }
                }
            }
        }

        categoryToggles = togglesList.ToArray();
    }
    public void GenerateCategoryTogglesWebGL()
    {
        List<Toggle> togglesList = new List<Toggle>(); // Usamos una lista temporal para agregar los toggles
        List<string> internalCategories = new List<string>
        {
        "AMBIENTAL", "CULTURA", "GEOGRAFIA", "HISTORIA", "DEPORTES", "BIOLOGIA" // Aquí agregas las categorías que quieras
        };

        // Leer el contenido del archivo
        foreach (string category in internalCategories)
        {
                // Si la categoría no existe, creamos el toggle y lo agregamos a la lista temporal
                Toggle newToggle = CreateToggle(category);
                togglesList.Add(newToggle);
        }
        categoryToggles = togglesList.ToArray();
        //toggleParent.transform.position = Vector3.zero;
        toggleListeners();
    }
    public Toggle CreateToggle(string category)
    {
        GameObject toggleGO = Instantiate(togglePrefab, toggleParent);
        Toggle toggle = toggleGO.GetComponent<Toggle>();
        toggle.GetComponentInChildren<Text>().text = category;
        return toggle;
    }
    public void PlayButton()
    {
        List<string> selectedCategories = GetSelectedCategories();

        if (selectedCategories.Count == 0)
        {
            Debug.Log("Por favor, selecciona al menos una categoría.");
            return;
        }
        // Guardar las categorías seleccionadas en PlayerPrefs
        PlayerPrefs.SetString(selectedCategoriesKey, string.Join(",", selectedCategories));
        PlayerPrefs.Save();
        Debug.Log(PlayerPrefs.GetString(selectedCategoriesKey));
        Debug.Log(PlayerPrefs.GetInt("ScoreLimit"));
        if (togglePrefab.name == "TOGGLE PREFAB HOR")
        {
            SceneManager.LoadScene("GameHor");
            return;
        }
        SceneManager.LoadScene("Game");

    }
    public void ExitButton()
    {
        Application.Quit();
    }
    private List<string> GetSelectedCategories()
    {
        List<string> selectedCategories = new List<string>();
        foreach (Toggle toggle in categoryToggles)
        {
            if (toggle.isOn)
            {
                selectedCategories.Add(toggle.GetComponentInChildren<Text>().text);
            }
        }
        return selectedCategories;
    }
    public void SelectAllCategories()
    {
        allSelected = !allSelected;
        foreach (Toggle toggle in categoryToggles)
        {            
            toggle.isOn = allSelected;            
            UpdateToggleColor(toggle, allSelected);
        }
    }
    public void DeleteToggles()
    {
        foreach (Toggle toggle in categoryToggles)
        {
            Destroy(toggle.gameObject);
        }
        categoryToggles = new Toggle[0];
    }
    public void UpdateToggleColor(Toggle toggle, bool val)
    {
        ColorBlock cb = toggle.colors;
        if (val)
        {
            // Configura el color cuando está seleccionado
            cb.normalColor = new Color(121 / 255f, 255 / 255f, 148 / 255f);
            cb.selectedColor = new Color(121 / 255f, 255 / 255f, 148 / 255f);
            cb.highlightedColor = new Color(121 / 255f, 255 / 255f, 148 / 255f);
        }
        else
        {
            // Configura el color cuando no está seleccionado
            cb.normalColor = Color.white;
            cb.selectedColor = Color.white;
            cb.highlightedColor= Color.white;
        }
        toggle.colors = cb;
    }
    public void SwapConfig()
    {        
        configPanel.SetActive(!configPanel.activeSelf);
#if !UNITY_WEBGL
        fileSelector.SetActive(configPanel.activeSelf);
#endif
        UpdateScoreLimitText();
        UpdateState();
        if (fileChanged)
        {
            fileChanged = !fileChanged;
            DeleteToggles();
            GenerateCategoryTogglesDesktop();
            toggleListeners();
        }
    }
    public void IncrementScoreLimit()
    {
        scoreLimit++;
        SaveScoreLimit();
        UpdateScoreLimitText();
    }
    public void DecrementScoreLimit()
    {
        if (scoreLimit > 0)
        {
            scoreLimit--;
        }
        SaveScoreLimit();
        UpdateScoreLimitText();
        
    }
    public void SwapEvent()
    {
        evento = !evento;
        SaveEventState();        
    }    
    private void UpdateState()
    {
        if (PlayerPrefs.HasKey("ModoEvento"))
        {
            int pref = PlayerPrefs.GetInt("ModoEvento");
            evento = pref == 1 ? true : false;
            Debug.Log("EventoMode de pref " + pref);
            eventoMode.isOn = evento;
        }
        else
        {
            Debug.Log("EventoMode de UI " + evento);
            eventoMode.isOn = evento;
        }
    }
    private void UpdateScoreLimitText()
    {
        if (PlayerPrefs.HasKey("ScoreLimit"))
        {
            int pref = PlayerPrefs.GetInt("ScoreLimit");
            Debug.Log("ScoreLimite de pref " + pref);
            scoreLimit = pref;
            limitUI.text = pref.ToString();
        }
        else
        {
            Debug.Log("ScoreLimite de UI " + scoreLimit);
            limitUI.text = scoreLimit.ToString();
        }
    }
    private void SaveScoreLimit()
    {
        PlayerPrefs.SetInt("ScoreLimit", scoreLimit);
        PlayerPrefs.Save();
    }
    private void SaveEventState()
    {
        int state = evento ? 1 : 0;
        PlayerPrefs.SetInt("ModoEvento", state);
        PlayerPrefs.Save(); 
    }
    private void SaveFileName()
    {
        PlayerPrefs.SetString("FILE_NAME", FILE_NAME);
        PlayerPrefs.Save();
    }
}
