using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decoration : MonoBehaviour
{
    public Sprite[] sprites;

    private void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        int x = Mathf.FloorToInt(transform.position.x);
        int y = Mathf.FloorToInt(transform.position.y);
        int seed = x ^ y;//x * (x + 17) * y + 2 * (y + 13) * x + 15;
        Random.InitState(seed);
        sr.sprite = sprites[Random.Range(0, sprites.Length)];
        sr.flipX = Random.value > 0.5f;
    }
}
