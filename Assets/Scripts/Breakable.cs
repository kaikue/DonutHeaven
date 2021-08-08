using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour
{
    public GameObject breakParticles;

    public void Break()
    {
        Instantiate(breakParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
