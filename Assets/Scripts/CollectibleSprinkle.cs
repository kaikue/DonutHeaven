using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSprinkle : MonoBehaviour
{
    public Color[] colors;
    public GameObject collectParticles;

    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        int x = Mathf.FloorToInt(transform.position.x);
        int y = Mathf.FloorToInt(transform.position.y);
        int seed = x ^ y;//x * (x + 17) * y + 2 * (y + 13) * x + 15;
        Random.InitState(seed);
        //print(transform.position + " " + seed);
        sr.color = colors[Random.Range(0, colors.Length)];
        sr.flipX = Random.value > 0.5f;
    }

    public void Collect()
	{
        GameObject particles = Instantiate(collectParticles, transform.position, Quaternion.identity);
        ParticleSystem.MainModule main = particles.GetComponent<ParticleSystem>().main;
        main.startColor = sr.color;
    }
}
