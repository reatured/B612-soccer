using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int player1Score = 0;
    public int player2Score = 0;
    public int winningScore = 5;
    
    [Header("Time Settings")]
    public float matchTime = 300f; // 5 minutes in seconds
    public bool useTimer = true;
    
    [Header("UI References")]
    public Text player1ScoreText;
    public Text player2ScoreText;
    public Text timerText;
    public Text gameOverText;
    public Text winnerText;
    public Button restartButton;
    
    [Header("Game State")]
    public bool gameActive = true;
    
    private float currentTime;
    
    void Start()
    {
        currentTime = matchTime;
        gameActive = true;
        
        UpdateUI();
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            restartButton.gameObject.SetActive(false);
        }
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
    }
    
    void Update()
    {
        if (!gameActive) return;
        
        if (useTimer)
        {
            UpdateTimer();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
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
        
        if (currentTime <= 30f)
        {
            timerText.color = Color.red;
        }
        else if (currentTime <= 60f)
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
        if (!gameActive) return;
        
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
        if (player1ScoreText != null)
            player1ScoreText.text = player1Score.ToString();
        if (player2ScoreText != null)
            player2ScoreText.text = player2Score.ToString();
    }
    
    void EndGameByScore()
    {
        gameActive = false;
        
        string winner = player1Score >= winningScore ? "Player 1" : "Player 2";
        
        Debug.Log($"Game Over! {winner} wins with {winningScore} goals!");
        
        ShowGameOver($"{winner} Wins!", $"Final Score: {player1Score} - {player2Score}");
    }
    
    void EndGameByTime()
    {
        gameActive = false;
        
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
        
        ShowGameOver("Time's Up!", $"{result}\\nFinal Score: {player1Score} - {player2Score}");
    }
    
    void ShowGameOver(string gameOverMessage, string winnerMessage)
    {
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
            gameOverText.gameObject.SetActive(true);
        }
        
        if (winnerText != null)
        {
            winnerText.text = winnerMessage;
            winnerText.gameObject.SetActive(true);
        }
        
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
        }
        
        Time.timeScale = 0.1f;
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;
        
        player1Score = 0;
        player2Score = 0;
        currentTime = matchTime;
        gameActive = true;
        
        UpdateUI();
        
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
        if (restartButton != null)
            restartButton.gameObject.SetActive(false);
        
        Ball ball = FindObjectOfType<Ball>();
        if (ball != null)
        {
            ball.ResetBall();
        }
        
        Debug.Log("Game restarted!");
    }
    
    void OnGUI()
    {
        if (!gameActive)
        {
            GUI.Label(new Rect(10, 10, 200, 30), "Press R to Restart");
        }
    }
}