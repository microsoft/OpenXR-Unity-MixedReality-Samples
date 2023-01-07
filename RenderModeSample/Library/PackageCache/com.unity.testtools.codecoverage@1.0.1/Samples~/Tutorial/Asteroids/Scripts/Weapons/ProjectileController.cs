using UnityEngine;

public class ProjectileController : BaseProjectile 
{
    private Vector2 direction = Vector2.right;

    private void Update()
    {
        if (!GameManager.IsPaused)
            Move();
    }

    public void Move()
    {
        transform.position += (Vector3)direction * Time.deltaTime * 16;
        transform.up = direction;
        if (Vector2.Distance(transform.position, Vector2.zero) > 20.0f)
            DestroyImmediate(gameObject);
    }

    public void SetDirection(Vector2 value)
    {
        direction = value;
    }

    public Vector2 GetDirection()
    {
        return direction;
    }
}
