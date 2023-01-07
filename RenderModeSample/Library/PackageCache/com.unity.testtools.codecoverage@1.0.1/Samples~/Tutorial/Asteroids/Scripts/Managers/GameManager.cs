using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour 
{
    // Values and methods for testing
    public static bool effectsEnabled = true;
    public static bool startEnabled = true;
    public static bool updateEnabled = true;
    public static bool asteroidsEnabled = true;

    public static void InitializeTestingEnvironment(bool start, bool update, bool effects, bool asteroids, bool paused)
    {
        effectsEnabled = effects;
        startEnabled = start;
        updateEnabled = update;
        asteroidsEnabled = asteroids;
        IsPaused = paused;
    }

    public static GameManager instance;
    public static SpaceshipController spaceship;
    public static int score;
    public static int scoreSinceLastDeath;
    private static bool isPaused;

    public GameObject spaceshipPrefab;
    public GameObject asteroidPrefab;

    [HideInInspector]
    public float asteroidSpawnDelay = 1.0f;
    [HideInInspector]
    public float asteroidSpawnTimer = 0.0f;
    [HideInInspector]
    public int deaths = 0;

    public const float SPACESHIP_RESPAWN_DELAY = 1.0f;
    public const float RELOAD_SCENE_DELAY = 3.0f;

    public static bool IsPaused
    {
        get { return isPaused; }
        set { Time.timeScale = value ? 0.0f : 1.0f; isPaused = value; }
    }

    public static bool SpaceshipIsActive() { return spaceship != null; }

    void OnEnable()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        
        Time.timeScale = 1.0f;
        IsPaused = false;
        score = 0;
        scoreSinceLastDeath = 0;
    }

    void Start()
    {
        if (startEnabled)
        {
            spaceship = Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity).GetComponent<SpaceshipController>();
        }
    }

    void Update()
    {
        if (updateEnabled && !IsPaused)
        {
            UpdateTimers();
            if (asteroidSpawnTimer <= 0.0f && asteroidsEnabled)
                SpawnAsteroids();
        }
	}

    private void UpdateTimers()
    {
        if (asteroidSpawnTimer > 0.0f)
            asteroidSpawnTimer -= Time.deltaTime;
    }

    public void RespawnShip(float delay = SPACESHIP_RESPAWN_DELAY)
    {
        StartCoroutine(RespawnShipCoroutine(delay));
    }

    public IEnumerator RespawnShipCoroutine(float delay)
    {
        deaths++;
        if (LifeCounter.instance)
            LifeCounter.instance.SetLives(3 - deaths);
        yield return new WaitForSeconds(delay);

        if (deaths < 3)
        {
            spaceship = Instantiate(spaceshipPrefab, Vector2.zero, Quaternion.identity).GetComponent<SpaceshipController>();
        }
        else
        {
            StartCoroutine(ReloadScene());
        }
    }

    public void SpawnAsteroids()
    {
        AsteroidController asteroid;
        if (Camera.main != null)
        {
            Vector2 spawnPosition = Random.value > 0.5f ? Camera.main.ViewportToWorldPoint(new Vector2(Random.value > 0.5f ? Random.Range(-0.1f, -0.05f) : Random.Range(1.05f, 1.1f), Random.Range(-0.1f, 1.1f))) : Camera.main.ViewportToWorldPoint(new Vector2(Random.Range(-0.1f, 1.1f), Random.value > 0.5f ? Random.Range(-0.1f, -0.05f) : Random.Range(1.05f, 1.1f)));
            asteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity).GetComponent<AsteroidController>();
            asteroid.SetDirection(Camera.main.ViewportToWorldPoint(new Vector2(Random.value, Random.value)));
        }
        else
            asteroid = Instantiate(asteroidPrefab, Vector2.zero, Quaternion.identity).GetComponent<AsteroidController>(); //for test purposes

        float randomValue = Random.value;
        if (randomValue < 0.333f)
            asteroid.SetSplitCount(0); // big asteroid
        else if (randomValue > 0.666f)
            asteroid.SetSplitCount(1); // medium asteroid
        else
            asteroid.SetSplitCount(2); // small asteroid
        
        asteroidSpawnTimer = asteroidSpawnDelay;
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(RELOAD_SCENE_DELAY);
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public static void AddToScore(int splitCount)
    {
        switch (splitCount)
        {
            case 0:
                score += 1000;
                scoreSinceLastDeath += 1000;
                break;
            case 1:
                score += 500;
                scoreSinceLastDeath += 500;
                break;
            case 2:
                score += 250;
                scoreSinceLastDeath += 250;
                break;
        }
        
        if (SpaceshipIsActive())
            spaceship.UpdateWeapon(scoreSinceLastDeath);
        if(ScoreCounter.instance)
            ScoreCounter.instance.StartCounting(score);
    }
}
