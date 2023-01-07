using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class SpaceshipTests {

    GameObject spaceshipPrefab;
    GameObject asteroidPrefab;
    GameObject cameraPrefab;

    [SetUp]
    public void Setup()
    {
        GameManager.InitializeTestingEnvironment(false, false, true, false, false);

        spaceshipPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().spaceshipPrefab;
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
    public void _01_SpaceshipPrefabExists() {
        Assert.NotNull(spaceshipPrefab);
    }

    [Test]
    public void _02_SpaceshipPrefabCanBeInstantiated()
    {
        ClearScene();
        GameObject spaceship = (GameObject)Object.Instantiate(spaceshipPrefab);
        spaceship.name = "Spaceship";
        Assert.NotNull(GameObject.Find("Spaceship"));
    }

    [Test]
    public void _03_SpaceshipPrefabHasRequiredComponentTransform()
    {
        Assert.IsNotNull(spaceshipPrefab.GetComponent<Transform>());
    }

    [Test]
    public void _04_SpaceshipPrefabHasRequiredComponentCollider()
    {
        Assert.IsNotNull(spaceshipPrefab.GetComponent<PolygonCollider2D>());
    }

    [Test]
    public void _05_SpaceshipPrefabHasRequiredComponentControllerScript()
    {
        Assert.IsNotNull(spaceshipPrefab.GetComponent<SpaceshipController>());
        // Checking if script component has required references 
        Assert.IsNotNull(spaceshipPrefab.GetComponent<SpaceshipController>().spaceshipDebris);
        Assert.IsNotNull(spaceshipPrefab.GetComponent<SpaceshipController>().weaponList);
    }

    [Test]
    public void _06_SpaceshipPrefabHasRequiredVisual()
    {
        Transform visualChild = spaceshipPrefab.transform.GetChild(0);
        Assert.IsTrue(visualChild.name == "Visual");
        Assert.IsNotNull(visualChild);
        Assert.IsNotNull(visualChild.GetComponent<MeshRenderer>());
        Assert.IsNotNull(visualChild.GetComponent<MeshRenderer>().sharedMaterials[0]);
        Assert.IsNotNull(visualChild.GetComponent<MeshRenderer>().sharedMaterials[1]);
        Assert.IsNotNull(visualChild.GetComponent<MeshFilter>());
        Assert.IsNotNull(visualChild.GetComponent<MeshFilter>().sharedMesh);
    }

    [Test]
    public void _07_SpaceshipPrefabHasRequiredComponentRigidbody()
    {
        Assert.IsNotNull(spaceshipPrefab.GetComponent<Rigidbody2D>());
        Assert.IsTrue(spaceshipPrefab.GetComponent<Rigidbody2D>().isKinematic);
        Assert.IsTrue(spaceshipPrefab.GetComponent<Rigidbody2D>().collisionDetectionMode == CollisionDetectionMode2D.Continuous);
        Assert.IsTrue(spaceshipPrefab.GetComponent<Rigidbody2D>().interpolation == RigidbodyInterpolation2D.Interpolate);
    }

    [UnityTest]
    public IEnumerator _08_SpaceshipIsDestroyedOnCollisionWithAsteroid()
    {
        ClearScene();
        GameObject spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(spaceship == null);
    }

    [UnityTest]
    public IEnumerator _09_SpaceshipTriggersAsteroidSplit()
    {
        ClearScene();
        Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForFixedUpdate();
        yield return null;

        AsteroidController[] asteroids = Object.FindObjectsOfType<AsteroidController>();
        Assert.IsTrue(asteroids.Length > 1);
    }

    [Test]
    public void _10_SpaceshipCanMove()
    {
        ClearScene();
        SpaceshipController spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity).GetComponent<SpaceshipController>();
        spaceship.direction = Vector2.up;
        spaceship.Move();
        Assert.IsTrue(spaceship.transform.position != Vector3.zero);
    }

    [UnityTest]
    public IEnumerator _11_SpaceshipRotationCanBeChanged()
    {
        ClearScene();
        SpaceshipController spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity).GetComponent<SpaceshipController>();
        spaceship.transform.eulerAngles = new Vector3(0.0f, 0.0f, 180.0f);

        float startingRotation = spaceship.transform.eulerAngles.z;
        spaceship.Turn(1.0f);   // Turn right

        yield return null;

        Assert.IsTrue(spaceship.transform.eulerAngles.z < startingRotation);

        startingRotation = spaceship.transform.eulerAngles.z;
        spaceship.Turn(-1.0f);  // Turn left

        yield return null;

        Assert.IsTrue(spaceship.transform.eulerAngles.z > startingRotation);
    }

    [Test]
    public void _12_SpaceshipMovesAccordingToItsDirectionVector()
    {
        ClearScene();
        SpaceshipController spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 0.0f)).GetComponent<SpaceshipController>();
        spaceship.Thrust(1.0f);
        spaceship.Move();
        Assert.IsTrue(spaceship.transform.position.y >= 0.0f && spaceship.transform.position.x == 0.0f);
    }

    [UnityTest]
    public IEnumerator _13_SpaceshipIsWarpedToTheOtherSideOfTheScreenAfterMovingOffscreen()
    {
        ClearScene();
        Object.Instantiate(cameraPrefab);
        SpaceshipController spaceship = Object.Instantiate(spaceshipPrefab, Vector2.right * 100.0f, Quaternion.identity).GetComponent<SpaceshipController>();
        
        yield return null;

        Assert.IsTrue(spaceship.transform.position.x < 0.0f);
        spaceship.transform.position = Vector2.left * 100.0f;

        yield return null;

        Assert.IsTrue(spaceship.transform.position.x > 0.0f);
        spaceship.transform.position = Vector2.up * 100.0f;

        yield return null;

        Assert.IsTrue(spaceship.transform.position.y < 0.0f);
        spaceship.transform.position = Vector2.down * 100.0f;

        yield return null;

        Assert.IsTrue(spaceship.transform.position.y > 0.0f);
    }

    [Test]
    public void _14_SpaceshipCanFireProjectiles()
    {
        ClearScene();
        SpaceshipController spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.Euler(0.0f, 0.0f, 0.0f)).GetComponent<SpaceshipController>();
        spaceship.Shoot();
        ProjectileController projectile = Object.FindObjectOfType<ProjectileController>();
        Assert.IsTrue(projectile != null);
    }

    [UnityTest]
    public IEnumerator _15_SpaceshipSpawnsDebrisWhenDestroyed()
    {
        ClearScene();
        GameObject spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity);
        Object.Instantiate(asteroidPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForFixedUpdate();
        yield return null;

        Assert.IsTrue(spaceship == null);
        DebrisController[] objects = Object.FindObjectsOfType<DebrisController>();
        Assert.IsTrue(objects.Length > 0);
    }

    [UnityTest]
    public IEnumerator _16_SpaceshipEngineEmitsParticles()
    {
        ClearScene();
        GameObject spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity);
        Assert.IsTrue(spaceship.transform.GetChild(0).GetChild(0) != null);
        Assert.IsTrue(spaceship.transform.GetChild(0).GetChild(0).GetComponent<EngineTrail>() != null);
        ParticleSystem ps = spaceship.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();
        Assert.IsTrue(ps != null);
        Assert.IsTrue(ps.particleCount == 0);

        yield return null;

        Assert.IsTrue(ps.particleCount > 0);
    }

    [UnityTest]
    public IEnumerator _17_SpaceshipEngineParticlesAreClearedAfterWarp()
    {
        ClearScene();
        Object.Instantiate(cameraPrefab);

        GameObject spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity);
        ParticleSystem ps = spaceship.transform.GetChild(0).GetChild(0).GetComponent<ParticleSystem>();

        yield return null; // Wait for particles to spawn

        spaceship.transform.position = Vector2.left * 100.0f;

        yield return null;

        Assert.IsTrue(ps.particleCount == 0);
    }

    [UnityTest]
    public IEnumerator _18_SpaceshipDoesntMoveDuringPause()
    {
        ClearScene();
        SpaceshipController spaceship = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity).GetComponent<SpaceshipController>();
        spaceship.direction = Vector2.up;
        Vector3 startPosition = spaceship.transform.position;

        GameManager.IsPaused = true;

        for (int i = 0; i < 20; i++)
            yield return null;

        Assert.IsTrue(spaceship.transform.position == startPosition);
    }

    [Test]
    public void _19_SpaceshipPrefabHasRequiredComponentAnimator()
    {
        Assert.IsNotNull(spaceshipPrefab.transform.GetChild(0).GetComponent<Animator>());
        Assert.IsNotNull(spaceshipPrefab.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController != null);
        Assert.IsTrue(spaceshipPrefab.transform.GetChild(0).GetComponent<Animator>().runtimeAnimatorController.name == "ShipAnimator");
        Assert.IsTrue(spaceshipPrefab.transform.GetChild(0).GetComponent<Animator>().cullingMode == AnimatorCullingMode.AlwaysAnimate);
        Assert.IsTrue(spaceshipPrefab.transform.GetChild(0).GetComponent<Animator>().updateMode == AnimatorUpdateMode.Normal);
    }

    [UnityTest]
    public IEnumerator _20_SpaceshipAnimatorPlaysSpawnAnimationOnSpawn()
    {
        ClearScene();
        Animator animator = Object.Instantiate(spaceshipPrefab, Vector3.zero, Quaternion.identity).transform.GetChild(0).GetComponent<Animator>();
        
        yield return null;
        
        Assert.IsNotNull(animator);
        Assert.IsTrue(animator.GetCurrentAnimatorStateInfo(0).IsName("SpaceshipSpawn"));
        
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            yield return null;
        }

        Assert.IsTrue(animator.transform.localScale == Vector3.one);
    }

    [Test]
    public void _21_SpaceshipWeaponListContainsData()
    {
        WeaponList weaponList = spaceshipPrefab.GetComponent<SpaceshipController>().weaponList;
        Assert.IsNotNull(weaponList);
        Assert.IsTrue(weaponList.weapons.Count == 2);

        foreach(WeaponList.Weapon weapon in weaponList.weapons)
        {
            Assert.IsTrue(weapon.weaponName.Length != 0);
            Assert.IsNotNull(weapon.weaponPrefab);
        }
    }
}
