using UnityEngine;

public class DebrisController : MonoBehaviour 
{
    public GameObject explosionParticles;

    public float impulse = 1.0f;
    public float debrisDuration = 0.5f;

    private Rigidbody2D[] fragments;

	void Start() 
    {
        if (GameManager.effectsEnabled)
        {
            if (explosionParticles != null)
            {
                GameObject particles = Instantiate(explosionParticles, transform.position, Quaternion.identity);
                particles.transform.localScale = transform.localScale == Vector3.one ? transform.localScale : Vector3.one / 3.0f + transform.localScale / 2.0f;
                Destroy(particles, particles.GetComponent<ParticleSystem>().main.duration);
            }

            fragments = GetComponentsInChildren<Rigidbody2D>();
            foreach (Rigidbody2D fragment in fragments)
            {
                fragment.AddForce((fragment.position - (Vector2)transform.position) * transform.localScale.x * impulse, ForceMode2D.Impulse);
            }
            Destroy(gameObject, debrisDuration);
        }
    }
}
