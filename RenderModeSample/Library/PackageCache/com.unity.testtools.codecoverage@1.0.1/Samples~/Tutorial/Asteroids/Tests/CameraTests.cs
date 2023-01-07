using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CameraTests 
{
    GameObject cameraPrefab;
    string asteroidsScenePath;
    LoadSceneParameters loadSceneParameters;

    [SetUp]
    public void Setup()
    {
        GameManager.InitializeTestingEnvironment(true, true, true, false, false);

        loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None);

        Object asteroidsScene = ((GameObject)Resources.Load("TestsReferences")).GetComponent<TestsReferences>().asteroidsScene;
        asteroidsScenePath = AssetDatabase.GetAssetPath(asteroidsScene);

        cameraPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().cameraPrefab;
    }

    [Test]
    public void _01_CameraPrefabExists()
    {
        Assert.NotNull(cameraPrefab);
    }

    [Test]
    public void _02_CameraPrefabHasRequiredComponents()
    {
        Assert.IsTrue(cameraPrefab.GetComponent<Camera>().clearFlags == CameraClearFlags.Skybox);
        Assert.IsTrue(cameraPrefab.GetComponent<Camera>().orthographic);
    }
    
    [UnityTest]
    public IEnumerator _03_CameraExistsInScene()
    {
        EditorSceneManager.LoadSceneInPlayMode(asteroidsScenePath, loadSceneParameters);
        
        yield return null;

        Assert.IsTrue(Object.FindObjectOfType<Camera>().name == "Camera");
    }
}
