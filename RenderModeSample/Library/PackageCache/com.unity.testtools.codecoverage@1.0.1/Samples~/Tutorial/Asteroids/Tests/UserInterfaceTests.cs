using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

public class UserInterfaceTests
{
    GameObject gameManagerPrefab;
    GameObject inGameMenuPrefab;
    string asteroidsScenePath;
    LoadSceneParameters loadSceneParameters;

    [SetUp]
    public void Setup()
    {
        GameManager.InitializeTestingEnvironment(true, false, false, false, false);

        loadSceneParameters = new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.None);

        Object asteroidsScene = ((GameObject)Resources.Load("TestsReferences")).GetComponent<TestsReferences>().asteroidsScene;
        asteroidsScenePath = AssetDatabase.GetAssetPath(asteroidsScene);

        gameManagerPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().gameManagerPrefab;
        inGameMenuPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().inGameMenuPrefab;
    }

    void ClearScene()
    {
        Transform[] objects = Object.FindObjectsOfType<RectTransform>();
        foreach (Transform obj in objects)
        {
            if (obj != null)
                Object.DestroyImmediate(obj.gameObject);
        }
    }
    
    [UnityTest]
    public IEnumerator _01_InGameMenuExistsInScene()
    {
        EditorSceneManager.LoadSceneInPlayMode(asteroidsScenePath, loadSceneParameters);
        
        yield return null;

        Assert.NotNull(Object.FindObjectOfType<InGameMenuController>());
    }

    [UnityTest]
    public IEnumerator _02_InGameMenuHasCorrectUIElements()
    {
        ClearScene();
        GameObject inGameMenu = Object.Instantiate(inGameMenuPrefab, Vector3.zero, Quaternion.identity);
        
        yield return null;

        Assert.NotNull(inGameMenu);
        Canvas canvas = inGameMenu.GetComponent<Canvas>();
        Assert.NotNull(canvas);
        Assert.NotNull(canvas.GetComponent<InGameMenuController>());

        Assert.NotNull(canvas.transform.GetChild(0));
        Assert.IsFalse(canvas.transform.GetChild(0).gameObject.activeInHierarchy);
        Assert.NotNull(canvas.transform.GetChild(1));
        Assert.IsTrue(canvas.transform.GetChild(1).gameObject.activeInHierarchy);
        Assert.NotNull(canvas.transform.GetChild(2));
        Assert.IsTrue(canvas.transform.GetChild(2).gameObject.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator _03_InGameMenuContainsButtons()
    {
        ClearScene();
        GameObject inGameMenu = Object.Instantiate(inGameMenuPrefab, Vector3.zero, Quaternion.identity);
        
        yield return null;

        Transform container = inGameMenu.transform.GetChild(0);
        Assert.NotNull(container.GetComponent<VerticalLayoutGroup>());

        // InGameMenu has the game title
        Assert.NotNull(container.GetChild(1).GetComponent<Image>());
        Assert.IsTrue(container.GetChild(1).GetComponent<Image>().sprite != null);

        // Pause menu has a resume button
        Assert.NotNull(container.GetChild(1).GetComponent<Button>());
        Assert.IsTrue(container.GetChild(1).name == "ResumeButton");
        Assert.IsTrue(container.GetChild(1).GetComponent<Button>().onClick.GetPersistentMethodName(0) == "ChangeMenuState");
    }

    [UnityTest]
    public IEnumerator _04_PauseMenuCanBeEnabledAndDisabled()
    {
        ClearScene();
        GameObject inGameMenu = Object.Instantiate(inGameMenuPrefab, Vector3.zero, Quaternion.identity);
        
        yield return null;

        InGameMenuController menuController = inGameMenu.GetComponent<InGameMenuController>();
        Assert.IsTrue(menuController.transform.GetChild(0).name == "PauseMenu");

        menuController.ChangeMenuState(true);
        Assert.IsTrue(menuController.transform.GetChild(0).gameObject.activeInHierarchy);
        Assert.IsTrue(menuController.transform.GetChild(1).gameObject.activeInHierarchy);
        menuController.ChangeMenuState(false);
        Assert.IsFalse(menuController.transform.GetChild(0).gameObject.activeInHierarchy);
        Assert.IsTrue(menuController.transform.GetChild(1).gameObject.activeInHierarchy);
    }

    [UnityTest]
    public IEnumerator _05_PauseMenuChangesGameManagerGameState()
    {
        EditorSceneManager.LoadSceneInPlayMode(asteroidsScenePath, loadSceneParameters);
        
        yield return null;

        InGameMenuController menuController = GameObject.Find("InGameMenu").GetComponent<InGameMenuController>();
        menuController.ChangeMenuState(true);
        Assert.IsTrue(GameManager.IsPaused);
        Assert.IsTrue(Time.timeScale == 0.0f);
        menuController.ChangeMenuState(false);
        Assert.IsFalse(GameManager.IsPaused);
        Assert.IsTrue(Time.timeScale == 1.0f);
    }

    [UnityTest]
    public IEnumerator _06_InGameScoreCounterChangesWhenScoreChanges()
    {
        EditorSceneManager.LoadSceneInPlayMode(asteroidsScenePath, loadSceneParameters);
        
        yield return null;

        Assert.NotNull(GameObject.Find("InGameMenu"));
        Canvas canvas = GameObject.Find("InGameMenu").GetComponent<Canvas>();

        Assert.NotNull(canvas.transform.GetChild(1));
        Text[] numbers = canvas.transform.GetChild(1).GetComponentsInChildren<Text>();
        Assert.IsTrue(numbers.Length == 7);         
        GameManager.AddToScore(0);

        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(numbers[0].text == "0" && numbers[1].text == "0" && numbers[2].text == "0" && numbers[3].text == "1" && numbers[4].text == "0" && numbers[5].text == "0" && numbers[6].text == "0");
    }

    [UnityTest]
    public IEnumerator _07_InGameMenuLivesControllerPlaysAnimationsWhenLifeCountIsChanged()
    {
        ClearScene();
        GameObject inGameMenu = Object.Instantiate(inGameMenuPrefab, Vector3.zero, Quaternion.identity);
        Transform livesTransform = inGameMenu.transform.GetChild(2);
        Animator[] lives = livesTransform.GetComponentsInChildren<Animator>();
        Assert.IsTrue(lives.Length == 3);

        yield return null;

        livesTransform.GetComponent<LifeCounter>().SetLives(2);

        yield return null;

        int i;

        // Going from 3 to 2 lives, therefore 1 "remove" animation should play
        for (i = 0; i < lives.Length; i++)
        {
            if (i >= 2)
            {
                Assert.IsTrue(lives[i].enabled == true);
                Assert.IsTrue(lives[i].GetCurrentAnimatorStateInfo(0).IsName("RemoveLife"));    
            }
            else
            {
                Assert.IsTrue(lives[i].enabled == false);
            }
        }

        yield return new WaitForSeconds(0.5f);

        for (i = 0; i < lives.Length; i++)
            Assert.IsTrue(lives[i].enabled == false);

        livesTransform.GetComponent<LifeCounter>().SetLives(0);

        yield return null;

        // Going from 2 to 0 lives, therefore 2 "remove" animations should play
        for (i = 0; i < lives.Length; i++)
        {
            if (i < 2)
            {
                Assert.IsTrue(lives[i].enabled == true);
                Assert.IsTrue(lives[i].GetCurrentAnimatorStateInfo(0).IsName("RemoveLife"));
            }
            else
            {
                Assert.IsTrue(lives[i].enabled == false);

            }
        }

        yield return new WaitForSeconds(0.5f);

        for (i = 0; i < lives.Length; i++)
            Assert.IsTrue(lives[i].enabled == false);

        livesTransform.GetComponent<LifeCounter>().SetLives(3);

        yield return null;

        // Going from 0 to 3 lives, therefore 3 "recover" animations should play
        for (i = 0; i < lives.Length; i++)
        {
            Assert.IsTrue(lives[i].enabled == true);
            Assert.IsTrue(lives[i].GetCurrentAnimatorStateInfo(0).IsName("RecoverLife"));
        }
    }
}