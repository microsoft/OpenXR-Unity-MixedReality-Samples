using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class AsteroidTests
{
    GameObject asteroidPrefab;

    [SetUp]
    public void Setup()
    {
        GameManager.InitializeTestingEnvironment(false, false, false, false, false);

        asteroidPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().asteroidPrefab;
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
    public void _01_AsteroidPrefabExists()
    {
        Assert.NotNull(asteroidPrefab);
    }

    [Test]
    public void _02_AsteroidPrefabCanBeInstantiated()
    {
        ClearScene();
        GameObject asteroid = (GameObject)Object.Instantiate(asteroidPrefab);
        asteroid.name = "Asteroid";
        Assert.NotNull(GameObject.Find("Asteroid"));
    }

    [Test]
    public void _03_AsteroidPrefabHasRequiredComponentTransform()
    {
        Assert.IsNotNull(asteroidPrefab.GetComponent<Transform>());
    }

    [Test]
    public void _04_AsteroidPrefabHasRequiredComponentCollider()
    {
        Assert.IsNotNull(asteroidPrefab.GetComponent<CircleCollider2D>());
    }

    [Test]
    public void _05_AsteroidPrefabHasRequiredComponentControllerScript()
    {
        Assert.IsNotNull(asteroidPrefab.GetComponent<AsteroidController>());
        Assert.IsNotNull(asteroidPrefab.GetComponent<AsteroidController>().asteroidExplosion); // Checking if script component has required references 
    }

    [UnityTest]
    public IEnumerator _06_AsteroidGameobjectIsDestroyedOnSplit()
    {
        ClearScene();
        GameObject asteroid = (GameObject)Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        asteroid.GetComponent<AsteroidController>().Split();
        
        yield return null;

        Assert.IsTrue(asteroid == null, "Base asteroid was not destroyed on Split");
    }

    [Test]
    public void _07_AsteroidSplitCountCanBeChanged()
    {
        ClearScene();
        AsteroidController asteroid = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        asteroid.SetSplitCount(2);
        Assert.IsTrue(asteroid.GetSplitCount() == 2);
    }

    [UnityTest]
    public IEnumerator _08_AsteroidCanSplitIntoTwo() 
    {
        ClearScene();
        GameObject asteroid = (GameObject)Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        asteroid.GetComponent<AsteroidController>().Split();                                    // Split base asteroid
        
        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();        // Find all asteroids in the scene
        Assert.IsTrue(asteroids.Length == 2);                                                   // There should be 2 asteroids in the scene now
    }

    [UnityTest]
    public IEnumerator _09_AsteroidCanSplitTwice()
    {
        ClearScene();
        GameObject asteroid = (GameObject)Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        asteroid.GetComponent<AsteroidController>().Split();                                    // Split base asteroid
        
        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();        // Find all asteroids in the scene
        foreach(AsteroidController childAsteroid in asteroids)
            childAsteroid.Split();                                                              // Split found asteroids
        
        yield return null;

        asteroids = Object.FindObjectsOfType<AsteroidController>();                             // Find all asteroids in the scene
        Assert.IsTrue(asteroids.Length == 4);                                                   // There should be 4 asteroids in the scene now
    }

    [UnityTest]
    // It takes three hits to destroy an asteroid from base size, after 3 hits the asteroid should not split anymore
    public IEnumerator _10_AsteroidCannotSplitThreeTimes() 
    {
        ClearScene();
        GameObject asteroid = (GameObject)Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        asteroid.GetComponent<AsteroidController>().Split();                                    // Split base asteroid
        
        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();        // Find all asteroids in the scene
        foreach (AsteroidController childAsteroid in asteroids)
            childAsteroid.Split();                                                              // Split found asteroids
        
        yield return null;

        asteroids = Object.FindObjectsOfType<AsteroidController>();                             // Find all asteroids in the scene
        foreach (AsteroidController childAsteroid in asteroids)
            childAsteroid.Split();                                                              // Split found asteroids
        
        yield return null;

        asteroids = Object.FindObjectsOfType<AsteroidController>();                             // Find all asteroids in the scene
        Assert.IsTrue(asteroids.Length == 0);                                                   // There should be no asteroids left in the scene
    }

    [UnityTest]
    // Splitting the asteroid should spawn 2 asteroids at half scale of the split asteroid 
    public IEnumerator _11_AsteroidScaleIsCutInHalfOnSplit() 
    {
        ClearScene();
        GameObject asteroid = (GameObject)Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        asteroid.GetComponent<AsteroidController>().Split();                                    // Split base asteroid
        
        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();        // Find all asteroids in the scene
        foreach (AsteroidController childAsteroid in asteroids)  
            Assert.IsTrue(childAsteroid.transform.localScale == new Vector3(0.5f, 0.5f, 0.5f));
    }


    [Test]
    public void _12_AsteroidCanMove()
    {
        ClearScene();
        AsteroidController asteroid = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        asteroid.Move();
        Assert.IsTrue(asteroid.transform.position != Vector3.zero);
    }

    [Test]
    public void _13_AsteroidDirectionCanBeChanged()
    {
        ClearScene();
        AsteroidController asteroid = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        asteroid.SetDirection(Vector2.up);
        Assert.IsTrue(asteroid.GetDirection() == Vector2.up);
    }

    [UnityTest]
    public IEnumerator _14_AsteroidMovesAccordingToItsDirectionVector()
    {
        ClearScene();
        AsteroidController asteroid = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        asteroid.SetDirection(Vector2.up);
        Assert.IsTrue(asteroid.GetDirection() == Vector2.up);

        float t = 0.5f;
        while (t > 0.0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(asteroid.transform.position.x == 0.0f && asteroid.transform.position.y > 0.0f);
    }

    [UnityTest]
    public IEnumerator _15_AsteroidIsDestroyedWhenOffscreen()
    {
        ClearScene();
        AsteroidController asteroid = Object.Instantiate(asteroidPrefab, Vector3.right * 100, Quaternion.identity).GetComponent<AsteroidController>();
        
        yield return null;

        Assert.IsTrue(asteroid == null, "Asteroid was not destroyed when off screen");
    }

    [Test]
    public void _16_AsteroidPrefabHasRequiredVisual()
    {
        Transform visualChild = asteroidPrefab.transform.GetChild(0);
        Assert.IsTrue(visualChild.name == "Visual");
        Assert.IsNotNull(visualChild);
        Assert.IsNotNull(visualChild.GetComponent<MeshRenderer>());
        Assert.IsNotNull(visualChild.GetComponent<MeshRenderer>().sharedMaterials[0]);
        Assert.IsNotNull(visualChild.GetComponent<MeshFilter>());
        Assert.IsNotNull(visualChild.GetComponent<MeshFilter>().sharedMesh);
    }

    [Test]
    public void _17_AsteroidPrefabHasRequiredComponentRigidbody()
    {
        Assert.IsNotNull(asteroidPrefab.GetComponent<Rigidbody2D>());
        Assert.IsTrue(asteroidPrefab.GetComponent<Rigidbody2D>().isKinematic);
        Assert.IsTrue(asteroidPrefab.GetComponent<Rigidbody2D>().collisionDetectionMode == CollisionDetectionMode2D.Continuous);
        Assert.IsTrue(asteroidPrefab.GetComponent<Rigidbody2D>().interpolation == RigidbodyInterpolation2D.Interpolate);
    }
    
    [UnityTest]
    public IEnumerator _18_AsteroidStartsWithARandomRotation()
    {
        ClearScene();
        AsteroidController asteroid1 = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        AsteroidController asteroid2 = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        AsteroidController asteroid3 = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        
        yield return null;

        Assert.IsTrue(asteroid1.transform.rotation != asteroid2.transform.rotation && asteroid1.transform.rotation != asteroid3.transform.rotation && asteroid2.transform.rotation != asteroid3.transform.rotation);
    }

    [UnityTest]
    public IEnumerator _19_AsteroidSpawnsExplosionWhenDestroyed() 
    {
        ClearScene();
        GameObject asteroid = (GameObject)Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        asteroid.GetComponent<AsteroidController>().Split();

        yield return null;

        Assert.IsTrue(asteroid == null);
        GameObject explosion = GameObject.Find("ExplosionRegular(Clone)");
        Assert.NotNull(explosion);
    }

    [UnityTest]
    public IEnumerator _20_AsteroidDoesntMoveDuringPause()
    {
        ClearScene();
        AsteroidController asteroid = Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity).GetComponent<AsteroidController>();
        asteroid.SetDirection(Vector2.up);
        Vector3 startPosition = asteroid.transform.position;
        GameManager.IsPaused = true;

        for(int i = 0; i < 20; i++)
            yield return null;

        Assert.IsTrue(asteroid.transform.position == startPosition);
    }
}
