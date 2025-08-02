using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameStage
{
    MainMenu,    // Stage 1: Starting menu with start button
    Credits,     // Stage 2: Credits screen
    Playing,     // Stage 3: Actual gameplay
    Paused,      // Stage 4: Paused state
    GameOver     // Stage 5: Game over state
}

public class GameManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int player1Score = 0;
    public int player2Score = 0;
    public int winningScore = 5;
    
    [Header("Time Settings")]
    public float matchTime = 60f; // 1 minute in seconds
    public bool useTimer = true;
    
    [Header("UI References - Background")]
    public Image backgroundTint;
    public bool autoFindBackgroundTint = true;
    private UIFadeController backgroundFadeController;
    
    [Header("UI References - Main Menu")]
    public Button startButton;
    public Button creditButton;
    public GameObject titleText;
    
    [Header("UI References - Credits")]
    public GameObject creditsPanel;
    public Button backButton;
    
    [Header("UI References - Gameplay")]
    public CustomSpriteFontRenderer combinedScoreText; // Format: "0 : 0"
    public CustomSpriteFontRenderer timerText;
    
    [Header("UI References - Pause Menu")]
    public GameObject pauseMenu;
    public Button resumeButton;
    public GameObject tutorialText;
    
    [Header("UI References - Game Over")]
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI winnerText;
    public Button restartButton;
    
    [Header("Game State")]
    public GameStage currentStage = GameStage.MainMenu;
    public bool gameActive = false;
    
    private float currentTime;
    
    void Start()
    {
        currentTime = matchTime;
        InitializeComponents();
        SetGameStage(GameStage.MainMenu);
        
        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        if (creditButton != null)
            creditButton.onClick.AddListener(ShowCredits);
        if (backButton != null)
            backButton.onClick.AddListener(BackToMainMenu);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
    }
    
    void InitializeComponents()
    {
        // Auto-find background tint if needed
        if (autoFindBackgroundTint && backgroundTint == null)
        {
            // Look for an Image component with "background", "tint", or "overlay" in the name
            Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);
            foreach (Image img in allImages)
            {
                string name = img.name.ToLower();
                if (name.Contains("background") || name.Contains("tint") || name.Contains("overlay"))
                {
                    backgroundTint = img;
                    Debug.Log($"Auto-found background tint: {img.name}");
                    break;
                }
            }
        }
        
        // Setup fade controller for background
        if (backgroundTint != null)
        {
            backgroundFadeController = backgroundTint.GetComponent<UIFadeController>();
            if (backgroundFadeController == null)
            {
                backgroundFadeController = backgroundTint.gameObject.AddComponent<UIFadeController>();
                backgroundFadeController.startVisible = true;
                backgroundFadeController.fadeInDuration = 0.3f;
                backgroundFadeController.fadeOutDuration = 0.5f;
                Debug.Log($"Added UIFadeController to background tint: {backgroundTint.name}");
            }
        }
    }
    
    void Update()
    {
        // Handle input based on current stage
        switch (currentStage)
        {
            case GameStage.MainMenu:
                // No special input handling needed, button handles start
                break;
                
            case GameStage.Playing:
                if (useTimer && gameActive)
                {
                    UpdateTimer();
                }
                
                // Pause game with Escape
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGame();
                }
                break;
                
            case GameStage.Paused:
                // Resume with Escape
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ResumeGame();
                }
                break;
                
            case GameStage.GameOver:
                // Game over input handled by restart button
                break;
        }
    }
    
    void SetGameStage(GameStage newStage)
    {
        currentStage = newStage;
        UpdateUIVisibility();
        
        switch (newStage)
        {
            case GameStage.MainMenu:
                gameActive = false;
                Time.timeScale = 1f;
                break;
                
            case GameStage.Credits:
                gameActive = false;
                Time.timeScale = 1f;
                break;
                
            case GameStage.Playing:
                gameActive = true;
                Time.timeScale = 1f;
                break;
                
            case GameStage.Paused:
                gameActive = false;
                Time.timeScale = 0f;
                break;
                
            case GameStage.GameOver:
                gameActive = false;
                Time.timeScale = 1f; // Don't pause time in game over
                break;
        }
    }
    
    void UpdateUIVisibility()
    {
        Debug.Log($"UpdateUIVisibility - Current Stage: {currentStage}");
        
        // Background Tint - visible during all UI states, invisible during gameplay
        if (backgroundFadeController != null)
        {
            bool shouldBeVisible = currentStage != GameStage.Playing;
            backgroundFadeController.SetVisible(shouldBeVisible);
        }
        
        // Main Menu UI
        if (startButton != null)
        {
            bool shouldShow = currentStage == GameStage.MainMenu;
            startButton.gameObject.SetActive(shouldShow);
            Debug.Log($"Start Button: {(shouldShow ? "SHOWN" : "HIDDEN")}");
        }
        if (creditButton != null)
        {
            bool shouldShow = currentStage == GameStage.MainMenu;
            creditButton.gameObject.SetActive(shouldShow);
            Debug.Log($"Credit Button: {(shouldShow ? "SHOWN" : "HIDDEN")}");
        }
        if (titleText != null)
            titleText.gameObject.SetActive(currentStage == GameStage.MainMenu);
        
        // Credits UI
        if (creditsPanel != null)
            creditsPanel.SetActive(currentStage == GameStage.Credits);
        if (backButton != null)
            backButton.gameObject.SetActive(currentStage == GameStage.Credits);
        
        // Gameplay UI
        if (combinedScoreText != null)
            combinedScoreText.gameObject.SetActive(currentStage == GameStage.Playing);
        if (timerText != null)
            timerText.gameObject.SetActive(currentStage == GameStage.Playing);
        
        // Pause Menu UI
        if (pauseMenu != null)
            pauseMenu.SetActive(currentStage == GameStage.Paused);
        if (tutorialText != null)
            tutorialText.gameObject.SetActive(currentStage == GameStage.Paused);
        
        // Game Over UI
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(currentStage == GameStage.GameOver);
        if (winnerText != null)
            winnerText.gameObject.SetActive(currentStage == GameStage.GameOver);
        if (restartButton != null)
            restartButton.gameObject.SetActive(currentStage == GameStage.GameOver);
    }
    
    public void StartGame()
    {
        // Reset game state
        player1Score = 0;
        player2Score = 0;
        currentTime = matchTime;
        
        // Start playing
        SetGameStage(GameStage.Playing);
        UpdateUI();
        
        // Spawn ball if there's a spawner
        BallSpawner spawner = FindFirstObjectByType<BallSpawner>();
        if (spawner != null)
        {
            spawner.DestroyAllBalls();
            spawner.SpawnBall();
        }
        
        Debug.Log("Game Started!");
    }
    
    public void PauseGame()
    {
        if (currentStage == GameStage.Playing)
        {
            SetGameStage(GameStage.Paused);
            Debug.Log("Game Paused");
        }
    }
    
    public void ResumeGame()
    {
        if (currentStage == GameStage.Paused)
        {
            SetGameStage(GameStage.Playing);
            Debug.Log("Game Resumed");
        }
    }
    
    void UpdateTimer()
    {
        currentTime -= Time.deltaTime;
        
        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGameByTime();
        }
        
        UpdateTimerUI();
    }
    
    void UpdateTimerUI()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        
        timerText.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));
        
        if (currentTime <= 10f)
        {
            timerText.SetColor(Color.red);
        }
        else if (currentTime <= 30f)
        {
            timerText.SetColor(Color.yellow);
        }
        else
        {
            timerText.SetColor(Color.white);
        }
    }
    
    public void PlayerScored(int playerNumber)
    {
        if (!gameActive || currentStage != GameStage.Playing) return;
        
        if (playerNumber == 1)
        {
            player1Score++;
            Debug.Log($"Player 1 scored! Score: {player1Score} - {player2Score}");
        }
        else if (playerNumber == 2)
        {
            player2Score++;
            Debug.Log($"Player 2 scored! Score: {player1Score} - {player2Score}");
        }
        
        UpdateUI();
        
        if (player1Score >= winningScore || player2Score >= winningScore)
        {
            EndGameByScore();
        }
    }
    
    void UpdateUI()
    {
        if (combinedScoreText != null)
            combinedScoreText.SetText($"{player1Score} : {player2Score}");
    }
    
    void EndGameByScore()
    {
        string winner = player1Score >= winningScore ? "Player 1" : "Player 2";
        
        Debug.Log($"Game Over! {winner} wins with {winningScore} goals!");
        
        ShowGameOver($"{winner} Wins!", $"Final Score: {player1Score} - {player2Score}");
    }
    
    void EndGameByTime()
    {
        string result;
        if (player1Score > player2Score)
        {
            result = "Player 1 Wins!";
        }
        else if (player2Score > player1Score)
        {
            result = "Player 2 Wins!";
        }
        else
        {
            result = "It's a Draw!";
        }
        
        Debug.Log($"Time's up! {result} Final Score: {player1Score} - {player2Score}");
        
        ShowGameOver("Time's Up!", $"{result}\nFinal Score: {player1Score} - {player2Score}");
    }
    
    void ShowGameOver(string gameOverMessage, string winnerMessage)
    {
        SetGameStage(GameStage.GameOver);
        
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }
        
        if (winnerText != null)
        {
            winnerText.text = winnerMessage;
        }
    }
    
    public void ShowCredits()
    {
        SetGameStage(GameStage.Credits);
        Debug.Log("Showing Credits");
    }
    
    public void BackToMainMenu()
    {
        SetGameStage(GameStage.MainMenu);
        Debug.Log("Back to Main Menu");
    }
    
    public void RestartGame()
    {
        SetGameStage(GameStage.MainMenu);
        Debug.Log("Returned to Main Menu");
    }
    
    void OnGUI()
    {
        // Show debug info
        GUI.Label(new Rect(10, 10, 200, 30), $"Stage: {currentStage}");
        
        if (currentStage == GameStage.Playing)
        {
            GUI.Label(new Rect(10, 30, 200, 30), "ESC: Pause");
        }
        else if (currentStage == GameStage.Paused)
        {
            GUI.Label(new Rect(10, 30, 200, 30), "ESC: Resume");
        }
    }
}
