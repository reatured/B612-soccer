using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    
    [Header("Scene Names")]
    public string menuSceneName = "MenuScene";
    public string gameSceneName = "GameScene";
    
    [Header("Transition Settings")]
    public float transitionDelay = 0.5f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void LoadMenuScene()
    {
        StartCoroutine(LoadSceneCoroutine(menuSceneName));
    }
    
    public void LoadGameScene()
    {
        StartCoroutine(LoadSceneCoroutine(gameSceneName));
    }
    
    public void RestartCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        StartCoroutine(LoadSceneCoroutine(currentScene));
    }
    
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    System.Collections.IEnumerator LoadSceneCoroutine(string sceneName)
    {
        yield return new WaitForSeconds(transitionDelay);
        SceneManager.LoadScene(sceneName);
    }
    
    void Update()
    {
        // Quick scene switching for development
        if (Input.GetKeyDown(KeyCode.F1))
        {
            LoadMenuScene();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            LoadGameScene();
        }
    }
}