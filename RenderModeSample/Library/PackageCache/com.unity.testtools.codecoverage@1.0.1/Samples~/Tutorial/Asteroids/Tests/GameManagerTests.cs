using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class GameManagerTests {

    GameObject gameManagerPrefab;
    GameObject asteroidPrefab;
    GameObject cameraPrefab;
    string asteroidsScenePath;
    LoadSceneParameters loadSceneParameters;

    [SetUp]
    public void Setup()
    {
        loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None);

        Object asteroidsScene = ((GameObject)Resources.Load("TestsReferences")).GetComponent<TestsReferences>().asteroidsScene;
        asteroidsScenePath = AssetDatabase.GetAssetPath(asteroidsScene);

        gameManagerPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().gameManagerPrefab;
        asteroidPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().asteroidPrefab;
        cameraPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().cameraPrefab;
    }

    void ClearScene()
    {
        Transform[] objects = Object.FindObjectsOfType<Transform>();
        foreach (Transform obj in objects)
        {
            if (obj != null)
                Object.DestroyImmediate(obj.gameObject);
        }
    }

    [Test]
    public void _01_GameManagerPrefabExists()
    {
        Assert.NotNull(gameManagerPrefab);
    }
    
    [Test]
    public void _02_GameManagerPrefabHasRequiredComponentScript()
    {
        Assert.IsNotNull(gameManagerPrefab.GetComponent<GameManager>());
    }

    [UnityTest]
    public IEnumerator _03_GameManagerExistsInScene()
    {
        EditorSceneManager.LoadSceneInPlayMode(asteroidsScenePath, loadSceneParameters);
        
        yield return null;

        Assert.NotNull(Object.FindObjectOfType<GameManager>());
    }

    [UnityTest]
    public IEnumerator _04_GameManagerCanSpawnSpaceshipOnLoad()
    {
        ClearScene();
        Object.Instantiate(gameManagerPrefab).GetComponent<GameManager>();
        GameManager.InitializeTestingEnvironment(true, false, false, false, false);

        yield return null;

        SpaceshipController spaceship = Object.FindObjectOfType<SpaceshipController>();        
        Assert.IsTrue(spaceship != null);
    }

    [UnityTest]
    public IEnumerator _05_GameManagerRespawnsSpaceshipAfterItHasBeenDestroyed()
    {
        ClearScene();
        Object.Instantiate(gameManagerPrefab).GetComponent<GameManager>();
        GameManager.InitializeTestingEnvironment(true, false, false, false, false);

        yield return null;

        GameObject asteroid = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        SpaceshipController spaceship = Object.FindObjectOfType<SpaceshipController>();
        spaceship.transform.position = Vector2.right * 10;
        asteroid.transform.position = spaceship.transform.position;

        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(spaceship == null);

        yield return new WaitForSeconds(GameManager.SPACESHIP_RESPAWN_DELAY);

        spaceship = Object.FindObjectOfType<SpaceshipController>();
        Assert.IsTrue(spaceship != null);
    }

    [UnityTest]
    public IEnumerator _06_GameManagerDoesNotRespawnShipAfterThreeDeaths()
    {
        ClearScene();
        GameManager gameManager = Object.Instantiate(gameManagerPrefab).GetComponent<GameManager>();
        GameManager.InitializeTestingEnvironment(true, false, false, false, false);

        Object.DestroyImmediate(Object.FindObjectOfType<SpaceshipController>());
        gameManager.deaths = 2;
        gameManager.RespawnShip(0.0f);

        yield return new WaitForSeconds(GameManager.SPACESHIP_RESPAWN_DELAY);

        SpaceshipController spaceship = Object.FindObjectOfType<SpaceshipController>();
        Assert.IsTrue(spaceship != null);
        Object.DestroyImmediate(spaceship);
        gameManager.RespawnShip(0.0f);

        yield return new WaitForSeconds(GameManager.SPACESHIP_RESPAWN_DELAY);

        spaceship = Object.FindObjectOfType<SpaceshipController>();
        Assert.IsTrue(spaceship == null);
    }

    [UnityTest]
    public IEnumerator _07_GameManagerSpawnsAsteroids() 
    {
        ClearScene();
        Object.Instantiate(gameManagerPrefab);
        GameManager.InitializeTestingEnvironment(false, true, false, true, false);

        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();
        Assert.IsTrue(asteroids.Length > 0);                                                   
    }

    [UnityTest]
    public IEnumerator _08_GameManagerSpawnsAsteroidsOverTime()
    {
        ClearScene();
        GameManager gameManager = Object.Instantiate(gameManagerPrefab).GetComponent<GameManager>();
        GameManager.InitializeTestingEnvironment(false, true, false, true, false);

        yield return new WaitForSeconds(gameManager.asteroidSpawnDelay + 0.5f);

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();
        Assert.IsTrue(asteroids.Length > 1);
    }

    [Test]
    public void _09_GameManagerOnlySpawnsAsteroidsOffscreen()
    {
        ClearScene();
        GameManager gameManager = Object.Instantiate(gameManagerPrefab).GetComponent<GameManager>();
        GameManager.InitializeTestingEnvironment(false, false, false, false, false);

        Object.Instantiate(cameraPrefab);

        for (int i = 0; i < 100; i++) 
            gameManager.SpawnAsteroids();

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();

        foreach (AsteroidController roid in asteroids)
        {
            Vector2 positionOnCamera = Camera.main.WorldToViewportPoint(roid.transform.position);

            Assert.IsTrue(((positionOnCamera.x >= -0.1f && positionOnCamera.x <= 1.1f) && (positionOnCamera.y <= -0.05f || positionOnCamera.y >= 1.05f)) || 
                            ((positionOnCamera.x <= -0.05f || positionOnCamera.x >= 1.05f) && (positionOnCamera.y >= -0.1f && positionOnCamera.y <= 1.1f)));
        }     
    }

    [Test]
    public void _10_GameManagerSpawnsRandomSizeAsteroids() 
    {
        ClearScene();
        GameManager gameManager = Object.Instantiate(gameManagerPrefab).GetComponent<GameManager>();
        GameManager.InitializeTestingEnvironment(false, false, false, false, false);

        bool small = false;
        bool medium = false;
        bool big = false;

        for (int i = 0; i < 100; i++)
            gameManager.SpawnAsteroids();

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();
        foreach (AsteroidController roid in asteroids)
        {
            if (roid.GetSplitCount() == 2)
                small = true;
            else if (roid.GetSplitCount() == 1)
                medium = true;
            else if (roid.GetSplitCount() == 0)
                big = true;
            if (small && medium && big)
                break;
        }

        Assert.IsTrue(small && medium && big);
    }

    [UnityTest]
    public IEnumerator _11_GameManagerScoreIsIncreasedAfterAsteroidsAreDestroyed()
    {
        ClearScene();
        Object.Instantiate(gameManagerPrefab);
        GameManager.InitializeTestingEnvironment(false, false, false, false, false);

        yield return null;

        AsteroidController asteroid = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        int score = GameManager.score;
        asteroid.Split();

        Assert.IsTrue(score != GameManager.score);
        Assert.IsTrue(GameManager.score == 1000);

        yield return null;

        score = GameManager.score;
        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();

        foreach(AsteroidController ast in asteroids)
            ast.Split();

        Assert.IsTrue(score != GameManager.score);
        Assert.IsTrue(GameManager.score == 2000);

        yield return null;

        score = GameManager.score;
        asteroids = Object.FindObjectsOfType<AsteroidController>();

        foreach (AsteroidController ast in asteroids)
            ast.Split();

        Assert.IsTrue(score != GameManager.score);
        Assert.IsTrue(GameManager.score == 3000);
    }


    [UnityTest]
    public IEnumerator _12_ReachingCertainScoreChangesSpaceshipWeapons()
    {
        ClearScene();
        Object.Instantiate(gameManagerPrefab);
        GameManager.InitializeTestingEnvironment(true, false, false, false, false);

        yield return null;

        for (int i = 0; i < 8; i++)
            GameManager.AddToScore(0);

        SpaceshipController spaceship = Object.FindObjectOfType<SpaceshipController>();
        Assert.IsTrue(spaceship != null);
        // Weapon changes to laser upon reaching 8000 points
        Assert.IsTrue(spaceship.currentWeapon == SpaceshipController.Weapon.Laser);
    }
}
