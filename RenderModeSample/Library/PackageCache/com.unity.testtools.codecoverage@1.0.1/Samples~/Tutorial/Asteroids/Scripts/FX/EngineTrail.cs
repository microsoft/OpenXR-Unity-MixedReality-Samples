using System.Collections;
using UnityEngine;

public class EngineTrail : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystem.EmissionModule em;

    void Start()
    {
        if (!GameManager.effectsEnabled)
            gameObject.SetActive(false);

        ps = GetComponent<ParticleSystem>();
        em = ps.emission;
        em.enabled = true;
    }

    public void ClearParticles()
    {
        StartCoroutine(Clear());
    }

    IEnumerator Clear()
    {
        em.enabled = false;
        ps.Clear();
        yield return null;
        em.enabled = true;
        ps.Clear();
    }
}
