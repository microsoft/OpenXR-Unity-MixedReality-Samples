using UnityEngine;

public class LaserController : BaseProjectile 
{
    public bool isActive = true;
    public float duration = 0.75f;

    private void Update()
    {
        if (!GameManager.IsPaused)
        {
            if (isActive)
                Expand();
            else
                Shrink();

            duration -= Time.deltaTime;
            if (duration <= 0.0f)
                isActive = false;
        }
    }

    private void Expand()
    {
        if (transform.localScale.y <= 25.0f)
            transform.localScale += Vector3.up * Time.deltaTime * 75.0f;
    }

    private void Shrink()
    {
        transform.localScale -= Vector3.up * Time.deltaTime * 75.0f;
        transform.position += transform.up * Time.deltaTime * 75.0f;
        if (transform.localScale.y <= 0.0f)
            Destroy(gameObject);
    }
}
