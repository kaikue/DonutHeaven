using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefillCrystal : MonoBehaviour
{
    public enum RefillType
	{
        Jump,
        Dash
	}

    public RefillType refillType;
    public Sprite usedSprite;
    private Sprite originalSprite;

    private const float rechargeTime = 3;

    [HideInInspector]
    public bool isUsable = true;
    private SpriteRenderer sr;

    private void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        originalSprite = sr.sprite;
    }

    public void Use()
	{
        isUsable = false;
        sr.sprite = usedSprite;
        StartCoroutine(Recharge());
	}

    private IEnumerator Recharge()
	{
        yield return new WaitForSeconds(rechargeTime);
        isUsable = true;
        sr.sprite = originalSprite;
	}
}
