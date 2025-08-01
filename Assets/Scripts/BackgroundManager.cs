using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager Instance;
    
    [Header("Star Settings")]
    public GameObject starPrefab;
    public int numberOfStars = 100;
    public float starSpawnRadius = 20f;
    public Vector2 starSizeRange = new Vector2(0.1f, 0.3f);
    public float twinkleSpeed = 2f;
    
    [Header("Background Color")]
    public Color backgroundColor = new Color(0.05f, 0.05f, 0.2f, 1f);
    
    private GameObject[] stars;
    private Camera mainCamera;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            GenerateStars();
            SetBackgroundColor();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
    }
    
    void GenerateStars()
    {
        stars = new GameObject[numberOfStars];
        
        for (int i = 0; i < numberOfStars; i++)
        {
            Vector2 randomPosition = Random.insideUnitCircle * starSpawnRadius;
            
            GameObject star;
            if (starPrefab != null)
            {
                star = Instantiate(starPrefab, transform);
            }
            else
            {
                star = CreateDefaultStar();
            }
            
            star.transform.position = randomPosition;
            star.transform.localScale = Vector3.one * Random.Range(starSizeRange.x, starSizeRange.y);
            
            StarTwinkle twinkle = star.GetComponent<StarTwinkle>();
            if (twinkle == null)
                twinkle = star.AddComponent<StarTwinkle>();
            
            twinkle.twinkleSpeed = Random.Range(twinkleSpeed * 0.5f, twinkleSpeed * 1.5f);
            
            stars[i] = star;
        }
    }
    
    GameObject CreateDefaultStar()
    {
        GameObject star = new GameObject("Star");
        star.transform.SetParent(transform);
        
        SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
        
        Texture2D starTexture = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(8, 8));
                float alpha = Mathf.Clamp01(1f - (distance / 6f));
                
                if (x == 8 || y == 8)
                    alpha = Mathf.Max(alpha, 0.8f);
                if ((x == 7 && y == 8) || (x == 9 && y == 8) || (x == 8 && y == 7) || (x == 8 && y == 9))
                    alpha = Mathf.Max(alpha, 0.6f);
                
                pixels[y * 16 + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        starTexture.SetPixels(pixels);
        starTexture.Apply();
        
        Sprite starSprite = Sprite.Create(starTexture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
        renderer.sprite = starSprite;
        renderer.sortingOrder = -10;
        
        return star;
    }
    
    void SetBackgroundColor()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = backgroundColor;
        }
    }
    
    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
                mainCamera = FindObjectOfType<Camera>();
                
            if (mainCamera != null)
                SetBackgroundColor();
        }
    }
}