using UnityEngine;

public class LifeCounter : MonoBehaviour 
{
    public static LifeCounter instance;

    int previousLifeCount = 3;

    Animator[] lives;

    public Sprite lifeVisual;
    public Sprite emptyLifeVisual;

    void OnEnable()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        lives = transform.GetComponentsInChildren<Animator>();
    }

    public void SetLives(int currentLives)
    {
        if (currentLives > 3)
            currentLives = 3;

        if(currentLives <= previousLifeCount)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i >= currentLives)
                {
                    if (i < previousLifeCount)
                    {
                        lives[i].enabled = true;
                        lives[i].SetTrigger("Remove");
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                if (i <= currentLives)
                {
                    if (i >= previousLifeCount)
                    {
                        lives[i].enabled = true;
                        lives[i].SetTrigger("Recover");
                    }
                }
            }
        }
           
        previousLifeCount = currentLives;
    }
}
