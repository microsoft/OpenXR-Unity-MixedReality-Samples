using UnityEngine;

public class AsteroidController : MonoBehaviour 
{
    public GameObject asteroidExplosion;

    private int splitCount = 0;
    private Vector2 direction = Vector2.right;
    
    private void Start()
    {
        transform.rotation = Random.rotation;
    }

    private void Update()
    {
        if(!GameManager.IsPaused)
            Move();
    }
    
    public void Move()
    {
        transform.position += (Vector3) direction * Time.deltaTime / transform.localScale.x / 2.0f;
        if (Vector2.Distance(transform.position, Vector2.zero) > 20.0f)
            DestroyImmediate(gameObject);
    }
    
    public void Split()
    {
        if(splitCount < 2)
        for (int i = 0; i < 2; i++)
        {
            AsteroidController asteroid = Instantiate(gameObject, transform.position, Quaternion.identity).GetComponent<AsteroidController>();
            asteroid.SetSplitCount(splitCount + 1);
            asteroid.SetDirection(new Vector2(Random.Range(-20.0f, 20.0f), Random.Range(-20.0f, 20.0f)));
        }
        GameManager.AddToScore(splitCount);
        Instantiate(asteroidExplosion, transform.position, transform.GetChild(0).rotation).transform.localScale = transform.localScale;
        Destroy(gameObject); 
    }

    public void SetSplitCount(int value)
    {
        splitCount = value;
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f)/ (Mathf.Pow(2, splitCount));
    }

    public int GetSplitCount()
    {
        return splitCount;
    }

    public void SetDirection(Vector2 value)
    {
        direction = (value - (Vector2)transform.position).normalized;
    }

    public Vector2 GetDirection()
    {
        return direction;
    }
}
