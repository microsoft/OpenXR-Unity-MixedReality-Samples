using UnityEngine;

public class InGameMenuController : MonoBehaviour 
{
    private GameObject pauseMenu;
    private bool pauseMenuActive = false;

	void Start()
    {
        pauseMenu = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            ChangeMenuState(!pauseMenuActive);
    }

    public void ChangeMenuState(bool isPaused)
    {
        if (pauseMenuActive == isPaused)
            return;

        pauseMenu.SetActive(isPaused);

        pauseMenuActive = isPaused;
        GameManager.IsPaused = isPaused;
    }
}
