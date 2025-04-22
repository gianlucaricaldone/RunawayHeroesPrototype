using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int currentLevelIndex = 0;
    public int currentWorldIndex = 0;
    public bool endlessMode = true;
    public int targetFramerate = 60;
    public float levelDistance = 1000f;

    [Header("References")]
    public PlayerController playerController;
    public PlayerHealth playerHealth;
    public InventoryManager inventoryManager;
    public FocusTimeController focusTimeController;
    public LevelGenerator levelGenerator;

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject levelCompletePanel;

    private bool isGameActive = false;
    private bool isPaused = false;
    private float distanceTraveled = 0f;
    private int score = 0;

    void Start()
    {
        // Set target framerate
        Application.targetFrameRate = targetFramerate;
        
        // Show main menu initially
        ShowMainMenu();
        
        // Pause the game
        PauseGame();
    }

    void Update()
    {
        if (isGameActive)
        {
            // Track distance traveled
            if (playerController != null)
            {
                distanceTraveled += playerController.forwardSpeed * Time.deltaTime;
                
                // Check for level completion
                if (!endlessMode && distanceTraveled >= levelDistance)
                {
                    CompleteLevel();
                }
            }
            
            // Check for pause input
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }
    }

    public void StartGame()
    {
        // Nasconde tutti i panel, incluso il menu principale
        HideAllPanels();

        if (mainMenuPanel != null)
        {
            CanvasGroup canvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            mainMenuPanel.SetActive(false);
        }
        
        // Mostra solo l'HUD
        if (hudPanel != null)
        {
            hudPanel.SetActive(true);
            
            // Se l'HUD ha un CanvasGroup, assicuriamoci che sia visibile e interagibile
            CanvasGroup hudCanvasGroup = hudPanel.GetComponent<CanvasGroup>();
            if (hudCanvasGroup != null)
            {
                hudCanvasGroup.alpha = 1f;
                hudCanvasGroup.interactable = true;
                hudCanvasGroup.blocksRaycasts = true;
            }
        }
        
        // Reset stats
        distanceTraveled = 0f;
        score = 0;
        
        // Attiva level generator
        if (levelGenerator != null)
        {
            levelGenerator.StartGenerator();
        }
        
        // Imposta lo stato di gioco e riprendi
        isGameActive = true;
        ResumeGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
            ShowPanel(hudPanel);
            HidePanel(pausePanel);
        }
        else
        {
            PauseGame();
            ShowPanel(pausePanel);
        }
    }

    public void OnPlayerDeath()
    {
        isGameActive = false;
        // Show game over panel
        ShowPanel(gameOverPanel);
    }

    public void CompleteLevel()
    {
        isGameActive = false;
        // Show level complete panel
        ShowPanel(levelCompletePanel);
        // Increase level index
        currentLevelIndex++;
    }

    public void ShowMainMenu()
    {
        // Nasconde tutti gli altri panel
        HideAllPanels();
        
        // Mostra il main menu panel
        if (mainMenuPanel != null)
        {
            // Attiva il gameObject se non è attivo
            mainMenuPanel.SetActive(true);
            
            // Gestione del CanvasGroup se presente
            CanvasGroup canvasGroup = mainMenuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            // Assicuriamoci che il pannello sia in primo piano
            if (mainMenuPanel.transform.parent != null)
            {
                mainMenuPanel.transform.SetAsLastSibling();
            }
        }
        
        // Pausa il gioco mentre si è nel menu principale
        PauseGame();
    }

    public void RestartLevel()
    {
        // Reset player position
        if (playerController != null)
        {
            playerController.transform.position = Vector3.up; // Start position
        }
        
        // Reset health
        if (playerHealth != null)
        {
            playerHealth.Heal(100); // Full heal
        }
        
        // Start game again
        StartGame();
    }

    public void StartNextLevel()
    {
        currentLevelIndex++;
        StartGame();
    }

    public void ShowWorldMap()
    {
        // In the prototype this can just return to the main menu
        ShowMainMenu();
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) HidePanel(mainMenuPanel);
        if (hudPanel != null) HidePanel(hudPanel);
        if (pausePanel != null) HidePanel(pausePanel);
        if (gameOverPanel != null) HidePanel(gameOverPanel);
        if (levelCompletePanel != null) HidePanel(levelCompletePanel);
    }

    private void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
            // Fade in if it has CanvasGroup
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }

    private void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            // Se ha un CanvasGroup, impostiamo alpha a 0 e disabilitiamo interazioni
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            
            // Disattiviamo comunque il GameObject del panel
            panel.SetActive(false);
        }
    }


}