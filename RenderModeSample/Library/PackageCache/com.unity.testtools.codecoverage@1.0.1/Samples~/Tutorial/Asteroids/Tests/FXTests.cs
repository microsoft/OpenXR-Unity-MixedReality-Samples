using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

public class FXTests 
{
    GameObject spaceshipPrefab;
    GameObject asteroidPrefab;
    GameObject spaceshipDebrisPrefab;
    GameObject explosionPrefab; 

    [SetUp]
    public void Setup()
    {
        GameManager.InitializeTestingEnvironment(false, false, true, false, false);

        spaceshipPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().spaceshipPrefab;
        asteroidPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().asteroidPrefab;
        spaceshipDebrisPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().spaceshipDebrisPrefab;
        explosionPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().explosionPrefab;
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
    public void _01_FXPrefabsExist()
    {
        Assert.NotNull(spaceshipDebrisPrefab);
        Assert.NotNull(spaceshipDebrisPrefab.GetComponent<DebrisController>());
        Assert.NotNull(explosionPrefab);
    }

    [UnityTest]
    public IEnumerator _02_DebrisFragmentsHaveVelocityOnStart()
    {
        ClearScene();
        GameObject debris = Object.Instantiate(spaceshipDebrisPrefab, Vector3.zero, Quaternion.identity);
        
        yield return null;

        foreach(Rigidbody2D fragment in debris.GetComponentsInChildren<Rigidbody2D>())
        {
            Assert.IsTrue(fragment.velocity != Vector2.zero);
        }
    }

    [UnityTest]
    public IEnumerator _03_DebrisFragmentsAreDestroyedAfterSeconds()
    {
        ClearScene();
        GameObject debris = Object.Instantiate(spaceshipDebrisPrefab, Vector3.zero, Quaternion.identity);

        yield return new WaitForSeconds(1.0f);                      // Debris should be destroyed after 1 sec
        
        Assert.IsTrue(debris == null);
        Assert.IsTrue(Object.FindObjectsOfType<Rigidbody2D>().Length == 0);
    }

    [UnityTest]
    public IEnumerator _04_ExplosionParticlesAreSpawnedWithDebris()
    {
        ClearScene();
        GameObject debris = Object.Instantiate(spaceshipDebrisPrefab, Vector3.zero, Quaternion.identity);
        
        yield return null;

        ParticleSystem explosion = Object.FindObjectOfType<ParticleSystem>();
        Assert.IsTrue(explosion != null);
        
        yield return new WaitForSeconds(explosion.main.duration);

        Assert.IsTrue(explosion == null);
    }

    [UnityTest]
    public IEnumerator _05_ExplosionParticlesActivateChildParticlesAndSubEmmitersSuccessfully()
    {
        ClearScene();
        ParticleSystem explosion = Object.Instantiate(spaceshipDebrisPrefab.GetComponent<DebrisController>().explosionParticles, Vector3.zero, Quaternion.identity).GetComponent<ParticleSystem>();
        Assert.IsTrue(explosion != null);
        Assert.IsTrue(explosion.transform.GetChild(0).GetComponent<ParticleSystem>() != null);
        Assert.IsTrue(explosion.transform.GetChild(1).GetComponent<ParticleSystem>() != null);
        Assert.IsTrue(explosion.transform.GetChild(2).GetComponent<ParticleSystem>() != null);
        
        yield return null;

        Assert.IsTrue(explosion.particleCount == 50); // Main explosion PS burst emits 50 particles on the first frame
        Assert.IsTrue(explosion.transform.GetChild(0).GetComponent<ParticleSystem>().particleCount == 1); // Glow emits 1 particle on the first frame
        Assert.IsTrue(explosion.transform.GetChild(1).GetComponent<ParticleSystem>().particleCount == 2); // Shockwave emits 2 particles on the first frame
        Assert.IsTrue(explosion.subEmitters.GetSubEmitterSystem(0) != null); // Subemitter exists, indexOutOfRange if subemitter cannot be found
        
        yield return new WaitForSeconds(0.2f);

        Assert.IsTrue(explosion.subEmitters.GetSubEmitterSystem(0).particleCount > 0); // Subemitter emits particles over time
    }
}
