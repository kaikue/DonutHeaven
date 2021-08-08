using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCleanup : MonoBehaviour
{
    private void Start()
    {
		float lifetime = gameObject.GetComponent<ParticleSystem>().main.startLifetime.constant;
        StartCoroutine(Disappear(lifetime));
    }

    private IEnumerator Disappear(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
