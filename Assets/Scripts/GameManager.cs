using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    Menu,
    Game,
    Pause,
    GameEnd,
    Credits
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Menu;
    private GameState previousState;
    
    [Header("UI Panels")]
    [Tooltip("Order: Menu, Game, Pause, GameEnd, Credits")]
    public GameObject[] uiPanels = new GameObject[5];
    
    [Header("UI Buttons")]
    public Button startButton;
    public Button restartButton;
    public Button pauseButton;
    public Button resumeButton;
    public Button menuButton;
    public Button creditsButton;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugUI = true;
    
    private GUIStyle debugStyle;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }
    
    void Start()
    {
        SetupDebugStyle();
        SetupButtonListeners();
        SetState(GameState.Menu);
    }

    void Update()
    {
        HandleInput();
    }
    
    void SetupDebugStyle()
    {
        debugStyle = new GUIStyle();
        debugStyle.fontSize = 24;
        debugStyle.normal.textColor = Color.white;
        debugStyle.fontStyle = FontStyle.Bold;
        debugStyle.alignment = TextAnchor.MiddleCenter;
    }

    void SetupButtonListeners()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
            
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);
            
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
            
        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);
            
        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);
    }

    void HandleInput()
    {
        // ESC key handling for all states
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Pause)
            {
                ResumeFromPause();
            }
            else if (currentState != GameState.Pause)
            {
                PauseFromState();
            }
            return;
        }
        
        switch (currentState)
        {
            case GameState.Menu:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartGame();
                }
                else if (Input.GetKeyDown(KeyCode.C))
                {
                    ShowCredits();
                }
                break;
                
            case GameState.Game:
                if (Input.GetKeyDown(KeyCode.P))
                {
                    PauseGame();
                }
                break;
                
            case GameState.Pause:
                if (Input.GetKeyDown(KeyCode.P))
                {
                    ResumeGame();
                }
                else if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }
                else if (Input.GetKeyDown(KeyCode.M))
                {
                    ReturnToMenu();
                }
                break;
                
            case GameState.GameEnd:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartGame();
                }
                else if (Input.GetKeyDown(KeyCode.M))
                {
                    ReturnToMenu();
                }
                break;
                
            case GameState.Credits:
                if (Input.GetKeyDown(KeyCode.M))
                {
                    ReturnToMenu();
                }
                break;
        }
        
        // Test shortcut to end game
        if (currentState == GameState.Game && Input.GetKeyDown(KeyCode.E))
        {
            EndGame();
        }
    }
    

    public void SetState(GameState newState)
    {
        if (currentState == newState) return;
        
        Debug.Log($"State changed from {currentState} to {newState}");
        previousState = currentState;
        currentState = newState;
        OnStateChanged(newState);
    }
    
    void OnStateChanged(GameState newState)
    {
        // Only Menu and Game states run at normal time, others are paused
        if (newState == GameState.Menu || newState == GameState.Game)
        {
            Time.timeScale = 1f;
        }
        else
        {
            Time.timeScale = 0f;
        }
        
        UpdateUIPanels();
    }



    void UpdateUIPanels()
    {
        for (int i = 0; i < uiPanels.Length; i++)
        {
            if (uiPanels[i] != null)
            {
                uiPanels[i].SetActive((int)currentState == i);
            }
        }
    }


    public void StartGame()
    {
        SetState(GameState.Game);
    }
    
    public void PauseGame()
    {
        if (currentState == GameState.Game)
        {
            SetState(GameState.Pause);
        }
    }
    
    public void ResumeGame()
    {
        if (currentState == GameState.Pause)
        {
            SetState(GameState.Game);
        }
    }
    
    public void RestartGame()
    {
        SetState(GameState.Game);
    }
    
    public void EndGame()
    {
        if (currentState == GameState.Game)
        {
            SetState(GameState.GameEnd);
        }
    }
    
    public void ReturnToMenu()
    {
        SetState(GameState.Menu);
    }
    
    public void ShowCredits()
    {
        SetState(GameState.Credits);
    }
    
    public void PauseFromState()
    {
        if (currentState != GameState.Pause)
        {
            SetState(GameState.Pause);
        }
    }
    
    public void ResumeFromPause()
    {
        if (currentState == GameState.Pause && previousState != GameState.Pause)
        {
            SetState(previousState);
        }
    }

    void OnGUI()
    {
        if (!showDebugUI) return;
        
        string debugInfo = $"Current State: {currentState}";
        GUI.Label(new Rect(10, 10, 300, 50), debugInfo, debugStyle);
    }
    
    public GameState GetCurrentState()
    {
        return currentState;
    }
}
