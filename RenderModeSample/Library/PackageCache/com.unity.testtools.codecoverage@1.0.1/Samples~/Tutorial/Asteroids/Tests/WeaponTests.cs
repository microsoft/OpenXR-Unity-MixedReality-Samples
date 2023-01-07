using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEditor;

public class WeaponTests
{
    GameObject projectilePrefab;
    GameObject laserPrefab;
    GameObject asteroidPrefab;
    GameObject spaceshipPrefab;

    [SetUp]
    public void Setup()
    {
        GameManager.InitializeTestingEnvironment(false, false, false, false, false);

        spaceshipPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().spaceshipPrefab;
        asteroidPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().asteroidPrefab;
        projectilePrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().projectilePrefab;
        laserPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().laserPrefab;
    }

    void ClearScene()
    {
        Transform[] objects = Object.FindObjectsOfType<Transform>();
        foreach (Transform obj in objects)
        {
            if(obj != null)
                Object.DestroyImmediate(obj.gameObject);
        }
    }

/*
    // Uncomment the code from line 35 up to line 237, run the tests again and compare the code coverage results

    [Test]
    public void _01_ProjectilePrefabExists()
    {
        Assert.NotNull(projectilePrefab);
    }

    [Test]
    public void _02_ProjectilePrefabCanBeInstantiated()
    {
        ClearScene();
        GameObject projectile = (GameObject)Object.Instantiate(projectilePrefab);
        projectile.name = "Projectile";
        Assert.NotNull(GameObject.Find("Projectile"));
    }

    [Test]
    public void _03_ProjectilePrefabHasRequiredComponentTransform()
    {
        Assert.IsNotNull(projectilePrefab.GetComponent<Transform>());
    }

    [Test]
    public void _04_ProjectilePrefabHasRequiredComponentCollider()
    {
        Assert.IsNotNull(projectilePrefab.GetComponent<BoxCollider2D>());
        Assert.IsTrue(projectilePrefab.GetComponent<BoxCollider2D>().size == new Vector2(0.2f, 0.2f));
    }

    [Test]
    public void _05_ProjectilePrefabHasRequiredComponentControllerScript()
    {
        Assert.IsNotNull(projectilePrefab.GetComponent<ProjectileController>());
    }

    [Test]
    public void _06_ProjectilePrefabHasRequiredVisual()
    {
        Transform visualChild = projectilePrefab.transform.GetChild(0);
        Assert.IsTrue(visualChild.name == "Visual");
        Assert.IsNotNull(visualChild);
        Assert.IsNotNull(visualChild.GetComponent<MeshRenderer>());
        Assert.IsNotNull(visualChild.GetComponent<MeshRenderer>().sharedMaterials[0]);
        Assert.IsNotNull(visualChild.GetComponent<MeshFilter>());
        Assert.IsNotNull(visualChild.GetComponent<MeshFilter>().sharedMesh);
    }

    [Test]
    public void _07_ProjectileCanMove()
    {
        ClearScene();
        ProjectileController projectile = Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity).GetComponent<ProjectileController>();
        projectile.Move();
        Assert.IsTrue(projectile.transform.position != Vector3.zero);
    }

    [Test]
    public void _08_ProjectileDirectionCanBeChanged()
    {
        ClearScene();
        ProjectileController projectile = Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity).GetComponent<ProjectileController>();
        projectile.SetDirection(Vector2.up);
        Assert.IsTrue(projectile.GetDirection() == Vector2.up);
    }

    [UnityTest]
    public IEnumerator _09_ProjectileMovesAccordingToItsDirectionVector()
    {
        ClearScene();
        ProjectileController projectile = Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity).GetComponent<ProjectileController>();
        projectile.SetDirection(Vector2.up);
        Assert.IsTrue(projectile.GetDirection() == Vector2.up);
        
        float t = 0.5f;
        while (t > 0.0f)
        {
            t -= Time.deltaTime;
            yield return null;
        }

        Assert.IsTrue(projectile.transform.position.x == 0.0f && projectile.transform.position.y > 0.0f);  //check if projectile moves according to given trajectory along the Y axis
    }

    [UnityTest]
    public IEnumerator _10_ProjectileRotatesAccordingToItsDirectionVector()
    {
        ClearScene();
        ProjectileController projectile = Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity).GetComponent<ProjectileController>();
        projectile.SetDirection(new Vector2(0.714f, -0.156f).normalized);
        
        yield return null;

        Assert.IsTrue((Vector2)projectile.transform.up == projectile.GetDirection());  
    }

    [UnityTest]
    public IEnumerator _11_ProjectileIsDestroyedWhenOffsceen()
    {
        ClearScene();
        ProjectileController projectile = Object.Instantiate(projectilePrefab, Vector3.right * 100, Quaternion.identity).GetComponent<ProjectileController>();
        
        yield return null;
        
        Assert.IsTrue(projectile == null);
    }

    [Test]
    public void _12_ProjectilePrefabHasRequiredComponentRigidbody()
    {
        Assert.IsNotNull(projectilePrefab.GetComponent<Rigidbody2D>());
        Assert.IsTrue(projectilePrefab.GetComponent<Rigidbody2D>().isKinematic);
        Assert.IsTrue(projectilePrefab.GetComponent<Rigidbody2D>().collisionDetectionMode == CollisionDetectionMode2D.Continuous);
        Assert.IsTrue(projectilePrefab.GetComponent<Rigidbody2D>().interpolation == RigidbodyInterpolation2D.Interpolate);
    }

    [UnityTest]
    public IEnumerator _13_ProjectileIsDestroyedOnCollisionWithAsteroid()
    {
        ClearScene();
        GameObject projectile = Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);
        
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(projectile == null);                                       
    }

    [UnityTest]
    public IEnumerator _14_ProjectileIgnoresCollisionWithSpaceship()
    {
        ClearScene();
        GameObject projectile = Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(spaceshipPrefab, Vector2.zero, Quaternion.identity);
        
        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(projectile != null);
    }

    [UnityTest]
    public IEnumerator _15_ProjectileTriggersAsteroidSplit()
    {
        ClearScene();
        Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForFixedUpdate();
        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();
        Assert.IsTrue(asteroids.Length > 1);
     }


    [UnityTest]
    public IEnumerator _16_ProjectilesCannotSplitTheSameAsteroidTwice()
    {
        ClearScene();
        Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForFixedUpdate();
        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();
        Assert.IsTrue(asteroids.Length == 2);
    }


    [UnityTest]
    public IEnumerator _17_ProjectilesDontMoveDuringPause()
    {
        ClearScene();
        ProjectileController projectile = Object.Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity).GetComponent<ProjectileController>();
        projectile.SetDirection(Vector2.up);
        Vector3 startPosition = projectile.transform.position;
        GameManager.IsPaused = true;
        
        for (int i = 0; i < 10; i++)
            yield return null;

        Assert.IsTrue(projectile.transform.position == startPosition);
    }

    [UnityTest]
    public IEnumerator _18_LaserFiresSuccessfully()
    {
        // ClearScene();
        // SpaceshipController spaceship = Object.Instantiate(spaceshipPrefab).GetComponent<SpaceshipController>();
        // spaceship.currentWeapon = SpaceshipController.Weapon.Laser;
        // spaceship.Shoot();

        yield return null;

        // LaserController laser = Object.FindObjectOfType<LaserController>();
        // Assert.NotNull(laser);
    }
*/
}
