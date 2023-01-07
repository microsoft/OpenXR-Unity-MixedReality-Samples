using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour {
    public static ScoreCounter instance;
    int currentScore = 0;
    int targetScore = 0;
    Text[]  numbers;
    
    void OnEnable()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    // Use this for initialization
    void Start () {
        numbers = transform.GetComponentsInChildren<Text>();
	}
    
    Coroutine iterator;

    public void StartCounting(int score)
    {
        targetScore = score;
         
        if (iterator != null)
            StopCoroutine(iterator);
        iterator = StartCoroutine(IterateScore());
    }

	public IEnumerator IterateScore () {
        while(currentScore < targetScore * 0.9f)
        {
            currentScore = (int)Mathf.Lerp(currentScore, targetScore, Time.deltaTime * 10.0f);
            SetScore(currentScore);
            yield return null;
        }
        SetScore(targetScore);
        StartCoroutine(Pop(targetScore.ToString().Length));
    }

    public void SetScore(int score)
    {
        char[] charScore = score.ToString().ToCharArray();
        int scoreLength = charScore.Length;
        int k = 0;
        for(int i = 0; i < numbers.Length; i++)
        {
            if (i < numbers.Length - scoreLength)
                numbers[i].text = "0";
            else
            {
                numbers[i].text = charScore[k++].ToString();
            }
        }
    }


    public IEnumerator Pop(int digits)
    {
        int index = numbers.Length - digits;
        float t = 0.0f;
        while (t < 1.0f)
        {
            Vector3 scale = t < 0.2f ? Vector3.one * (1.0f + t) : Vector3.Lerp(numbers[index].transform.localScale, Vector3.one, t);

            for (int i = index; i < numbers.Length; i++)
                numbers[i].transform.localScale = scale;

            t += Time.deltaTime * 10.0f;
            yield return null;
        }

        for (int i = index; i < numbers.Length; i++)
            numbers[i].transform.localScale = Vector3.one;
    }

}
