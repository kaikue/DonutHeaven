using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public float bounceForce;
    public bool animated;
    public Transform sprite;
    public AnimationCurve sizeAnim;
    public float animLength;

    public void Bounce()
	{
        if (animated)
		{
            StartCoroutine(Animate());
		}
	}

    private IEnumerator Animate()
	{
        for (float t = 0; t < animLength; t += Time.deltaTime)
		{
            sprite.localScale = new Vector3(1, 1, 1) * sizeAnim.Evaluate(t / animLength);
            yield return null;
        }
        sprite.localScale = new Vector3(1, 1, 1);
    }
}
