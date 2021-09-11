using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class EndingPlayer : MonoBehaviour
{
    public Sprite standSprite;
    public Sprite[] walkSprites;
    public Sprite yaySprite;
    public AudioClip yaySound;
    public AudioClip landSound;
    private AudioSource audioSource;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private EndingZone endingZone;
    private SpriteRenderer frostingSR;
    private SpriteRenderer cinnamonSR;
    private AudioSource bgm;
    private float startVol;
    private bool onGround;
    private bool startedOnGround = false;
    private bool started = false;
    private bool walking = false;
    private int walkFrame = 0;
    private float walkFrameTime = 0;
    private const float walkSpeed = 3.5f;
    private const float sfxVolume = 0.2f;
    private const float walkFrameLen = 0.1f;

    private const float stopTime1 = 1;
    private const float walkTime = 1;
    private const float stopTime2 = 1;
    private const float stopTime3 = 1.5f;
    private const float stopTimeYay = 0.4f;
    private const float stopTimeEnd = 2.5f;
    private const float musicFadeTime = 3;

    private void Awake()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        rb = gameObject.GetComponent<Rigidbody2D>();
        bgm = GameObject.Find("BGM").GetComponent<AudioSource>();
        startVol = bgm.volume;
        CinemachineVirtualCamera vcam = Camera.main.transform.Find("VCam").GetComponent<CinemachineVirtualCamera>();
        vcam.Follow = transform;
        vcam.GetCinemachineComponent<CinemachineFramingTransposer>().m_DeadZoneWidth = 0;
    }

    public void SetEndingZone(EndingZone endingZone, bool startOnGround)
	{
        this.endingZone = endingZone;
        frostingSR = endingZone.angelFrosting.GetComponent<SpriteRenderer>();
        cinnamonSR = endingZone.angelCinnamon.GetComponent<SpriteRenderer>();
        if (startOnGround)
		{
            startedOnGround = true;
            sr.sprite = standSprite;
        }
    }

	private void OnCollisionEnter2D(Collision2D collision)
	{
        onGround = true;
        if (!startedOnGround)
        {
            audioSource.PlayOneShot(landSound);
        }
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
        onGround = true;
    }

	private void Update()
	{
		if (walking)
		{
            sr.sprite = walkSprites[walkFrame];
            walkFrameTime += Time.deltaTime;
            if (walkFrameTime > walkFrameLen)
			{
                walkFrameTime = 0;
                walkFrame = (walkFrame + 1) % walkSprites.Length;
			}
        }
	}

	private void FixedUpdate()
	{
		if (!started && onGround)
        {
            started = true;
            StartCoroutine(EndingSequence());
		}

        if (walking)
		{
            rb.MovePosition(new Vector2(rb.position.x + walkSpeed * Time.fixedDeltaTime, rb.position.y));
		}
	}

    private IEnumerator EndingSequence()
	{
        sr.sprite = standSprite;
        StartCoroutine(FadeBGM());
        yield return new WaitForSeconds(stopTime1);
        walking = true;
        yield return new WaitForSeconds(walkTime);
        walking = false;
        Destroy(rb);
        sr.sprite = standSprite;
        yield return new WaitForSeconds(stopTime2);
        frostingSR.flipX = true;
        yield return new WaitForSeconds(stopTime3);
        audioSource.volume = sfxVolume;
        frostingSR.sprite = endingZone.frostingYaySprite;
        audioSource.PlayOneShot(yaySound);
        yield return new WaitForSeconds(stopTimeYay);
        cinnamonSR.sprite = endingZone.cinnamonYaySprite;
        audioSource.PlayOneShot(yaySound);
        yield return new WaitForSeconds(stopTimeYay);
        sr.sprite = yaySprite;
        audioSource.PlayOneShot(yaySound);
        yield return new WaitForSeconds(stopTimeEnd);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator FadeBGM()
	{
        for (float t = 0; t < musicFadeTime; t += Time.deltaTime)
		{
            bgm.volume = Mathf.Lerp(startVol, 0, t / musicFadeTime);
            yield return null;
		}
	}
}
