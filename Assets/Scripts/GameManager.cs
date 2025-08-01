using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum GameStage
{
    MainMenu,    // Stage 1: Starting menu with start button
    Playing,     // Stage 2: Actual gameplay
    Paused,      // Stage 3: Paused state
    GameOver     // Stage 4: Game over state
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
    
    [Header("UI References - Main Menu")]
    public Button startButton;
    public TextMeshProUGUI titleText;
    
    [Header("UI References - Gameplay")]
    public TextMeshProUGUI combinedScoreText; // Format: "0 : 0"
    public TextMeshProUGUI timerText;
    
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
        SetGameStage(GameStage.MainMenu);
        
        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
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
        // Main Menu UI
        if (startButton != null)
            startButton.gameObject.SetActive(currentStage == GameStage.MainMenu);
        if (titleText != null)
            titleText.gameObject.SetActive(currentStage == GameStage.MainMenu);
        
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
        BallSpawner spawner = FindObjectOfType<BallSpawner>();
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
        
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        
        if (currentTime <= 10f)
        {
            timerText.color = Color.red;
        }
        else if (currentTime <= 30f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
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
            combinedScoreText.text = $"{player1Score} : {player2Score}";
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
