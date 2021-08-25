using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Player : MonoBehaviour
{
    private enum AnimState
    {
        Stand,
        Run,
        Jump,
        Flap,
        Fall,
        Slam,
        Dash
    }

    private const float runAcceleration = 20;
    public const float maxRunSpeed = 9;
    private const float jumpForce = 10;
    private const float doubleJumpForce = 15;
    private const float gravityForce = 20;
    private const float maxFallSpeed = 30;
    private const float slamSpeed = 35;
    private const float slamHoldTime = 0.3f;
    private const float dashForce = 40;
    private const float dashTime = 0.5f;
    private const float minBounceForce = 10;
    private const float minBreakXForce = 10;
    private const float breakRecoilForce = 5;
    //private const float slamParticlesMultiplier = 10;
    //private const float dashParticlesMultiplier = 10;
    private const float pitchVariation = 0.15f;

    private Rigidbody2D rb;
    private EdgeCollider2D ec;

    private bool triggerWasHeld = false;
    private bool jumpQueued = false;
    private bool slamQueued = false;
    private bool dashQueued = false;
    private bool canDoubleJump = true;
    private bool isSlamming = false;
    private float slamHeldTime = 0;
    private bool canDash = true;
    private float dashCountdown = 0;
    private float currentDashForce = 0;
    private float xForce = 0;

    private bool canJump = false;
    private bool wasOnGround = false;
    private Coroutine crtCancelQueuedJump;
    private const float jumpBufferTime = 0.1f; //time before hitting ground a jump will still be queued
    private const float jumpGraceTime = 0.1f; //time after leaving ground player can still jump (coyote time)

    private Transform respawnPoint;

    private CinemachineImpulseSource impulseSource;

    private const float runFrameTime = 0.1f;
    private SpriteRenderer sr;
    private AnimState animState = AnimState.Stand;
    private int animFrame = 0;
    private float frameTime; //max time of frame
    private float frameTimer; //goes from frameTime down to 0
    private bool facingLeft = false; //for animation (images face right)
    public Sprite standSprite;
    public Sprite jumpSprite;
    public Sprite flapSprite;
    public Sprite fallSprite;
    public Sprite slamSprite;
    public Sprite dashSprite;
    public Sprite[] runSprites;

    public GameObject particleBurst;

    public AudioSource sfxAudioSource;
    public AudioSource slamLoopAudioSource;
    public AudioClip[] jumpSounds;
    public AudioClip[] flapSounds;
    public AudioClip landSound;
    public AudioClip[] slamStartSounds;
    public AudioClip slamHitSound;
    public AudioClip[] dashSounds;
    public AudioClip refillSound;
    public AudioClip collectSound;
    public AudioClip[] smashSounds;
    public AudioClip bounceSound;

    public GameObject transitionPlayerPrefab;
    public GameObject hurtPlayerPrefab;

    [HideInInspector]
    public int sprinkles = 0;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        ec = gameObject.GetComponent<EdgeCollider2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        impulseSource = gameObject.GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            TryStopCoroutine(crtCancelQueuedJump);
            jumpQueued = true;
            crtCancelQueuedJump = StartCoroutine(CancelQueuedJump());
        }

        if (Input.GetButtonDown("Dash"))
		{
            dashQueued = true;
		}

        bool triggerHeld = Input.GetAxis("LTrigger") > 0 || Input.GetAxis("RTrigger") > 0;
        bool triggerPressed = !triggerWasHeld && triggerHeld;
        if (Input.GetButtonDown("Slam") || triggerPressed)
        {
            slamQueued = true;
        }
        triggerWasHeld = triggerHeld;

        sr.flipX = facingLeft;
        AdvanceAnim();
        sr.sprite = GetAnimSprite();
    }

    private Collider2D BoxcastTiles(Vector2 direction, float distance)
	{
        Vector2 size = new Vector2(ec.points[2].x - ec.points[1].x, ec.points[1].y - ec.points[0].y);
        RaycastHit2D hit = Physics2D.BoxCast(rb.position, size, 0, direction, distance, LayerMask.GetMask("Tiles"));
        return hit.collider;
    }

    private Collider2D RaycastTiles(Vector2 startPoint, Vector2 endPoint)
	{
        RaycastHit2D hit = Physics2D.Raycast(startPoint, endPoint - startPoint, Vector2.Distance(startPoint, endPoint), LayerMask.GetMask("Tiles"));
        return hit.collider;
    }

    private bool CheckSide(int point0, int point1, Vector2 direction)
    {
        Vector2 startPoint = rb.position + ec.points[point0] + direction * 0.02f;
        Vector2 endPoint = rb.position + ec.points[point1] + direction * 0.02f;
        Collider2D collider = RaycastTiles(startPoint, endPoint);
        return collider != null;
    }

    private void FixedUpdate()
    {
        float xInput = Input.GetAxis("Horizontal");
        float prevXVel = rb.velocity.x;
        float xVel;
        float dx = runAcceleration * Time.fixedDeltaTime * xInput;
        if (prevXVel != 0 && Mathf.Sign(xInput) != Mathf.Sign(prevXVel))
		{
            xVel = 0;
		}
        else
		{
            xVel = prevXVel + dx;
            float speedCap = Mathf.Abs(xInput * maxRunSpeed);
            xVel = Mathf.Clamp(xVel, -speedCap, speedCap);
        }

        if (xForce != 0)
		{
            //if not moving: keep xForce
            if (xInput == 0)
			{
                xVel = xForce;
			}
            else
			{
                if (Mathf.Sign(xInput) == Mathf.Sign(xForce)) {
                    //moving in same direction
                    if (Mathf.Abs(xVel) >= Mathf.Abs(xForce))
					{
                        //xVel has higher magnitude: set xForce to 0 (replace little momentum push)
                        xForce = 0;
                    }
                    else
					{
                        //xForce has higher magnitude: set xVel to xForce (pushed by higher momentum)
                        xVel = xForce;
                    }
                }
                else
				{
                    //moving in other direction
                    //decrease xForce by dx (stopping at 0)
                    float prevSign = Mathf.Sign(xForce);
                    xForce += dx;
                    if (Mathf.Sign(xForce) != prevSign)
					{
                        xForce = 0;
					}
                    xVel = xForce;
                }
            }
		}

        if (xInput != 0)
        {
            facingLeft = xInput < 0;
        }
        else if (xVel != 0)
        {
            //facingLeft = xVel < 0;
        }

        float yVel;

        bool onGround = CheckSide(4, 3, Vector2.down); //BoxcastTiles(Vector2.down, 0.15f) != null;
        bool onCeiling = CheckSide(1, 2, Vector2.up); //BoxcastTiles(Vector2.up, 0.15f) != null;

        if (onGround)
        {
            canJump = true;
            canDoubleJump = true;

            if (!wasOnGround || dashCountdown == 0)
            {
                canDash = true;
			}

            if (xForce != 0)
			{
                xForce *= 0.8f;
                if (Mathf.Abs(xForce) < 0.05f)
				{
                    xForce = 0;
				}
			}

            if (rb.velocity.y < 0)
            {
                if (isSlamming)
				{
                    PlaySound(slamHitSound);
                    ScreenShake();
                    Instantiate(particleBurst, transform.position, Quaternion.identity);
                }
                else
                {
                    PlaySound(landSound);
                }
            }

            StopSlamming();
            yVel = 0;

            animState = xVel == 0 ? AnimState.Stand : AnimState.Run;
        }
        else
		{
            yVel = Mathf.Max(rb.velocity.y - gravityForce * Time.fixedDeltaTime, -maxFallSpeed);

            if (wasOnGround)
			{
                StartCoroutine(LeaveGround());
			}

            if (yVel < 0)
			{
                animState = AnimState.Fall;
            }
        }
        wasOnGround = onGround;

        if (onCeiling && yVel > 0)
        {
            yVel = 0;
            PlaySound(landSound);
        }

        //if on ground or just left it: first jump
        //if can double jump: second jump
        //else: keep queued
        if (jumpQueued)
        {
            if (canJump)
            {
                StopCancelQueuedJump();
                jumpQueued = false;
                canJump = false;
                yVel = jumpForce; //Mathf.Max(jumpForce, yVel + jumpForce);
                PlayRandomSound(jumpSounds, false);
                animState = AnimState.Jump;
            }
            else if (canDoubleJump)
            {
                StopCancelQueuedJump();
                jumpQueued = false;
                yVel = doubleJumpForce; //Mathf.Max(doubleJumpForce, yVel + doubleJumpForce);
                PlayRandomSound(flapSounds);
				PlayRandomSound(jumpSounds, false);
                canDoubleJump = false;
                StopSlamming();
                animState = AnimState.Flap;
            }
        }

        if (dashQueued)
		{
            dashQueued = false;
            if (canDash)
			{
                canDash = false;
                dashCountdown = dashTime;
                currentDashForce = dashForce * (facingLeft ? -1 : 1);
                xForce = currentDashForce;
                yVel = 0;
                StopSlamming();
                PlayRandomSound(dashSounds);
                animState = AnimState.Dash;
            }
		}

        if (dashCountdown > 0)
		{
            dashCountdown -= Time.fixedDeltaTime;
            if (dashCountdown < Time.fixedDeltaTime)
			{
                dashCountdown = 0;
                xForce = 0;
			}
            else
			{
                xForce = Mathf.Lerp(0, currentDashForce, dashCountdown / dashTime);
            }
		}

        if (slamQueued)
		{
            slamQueued = false;
            if (!onGround && !isSlamming)
            {
                isSlamming = true;
                slamHeldTime = 0;
                dashCountdown = 0;
                PlayRandomSound(slamStartSounds, false);
            }
		}

        if (isSlamming)
		{
            xVel = 0;
            xForce = 0;

            if (slamHeldTime < slamHoldTime)
			{
                slamHeldTime += Time.fixedDeltaTime;
                yVel = 0;
			}
            else
			{
                if (!slamLoopAudioSource.isPlaying)
                {
                    slamLoopAudioSource.Play();
                }
                yVel = -slamSpeed;
            }

            Collider2D collider = BoxcastTiles(Vector2.up, 1.5f * yVel * Time.fixedDeltaTime);
            if (collider != null && collider.GetComponent<Breakable>() != null)
			{
                collider.GetComponent<Breakable>().Break();
                StopSlamming();
                canDoubleJump = true;
                canDash = true;
                yVel = breakRecoilForce;
                ScreenShake();
                Instantiate(particleBurst, transform.position, Quaternion.identity);
                PlayRandomSound(smashSounds);
                PlaySound(slamHitSound);
            }

            animState = AnimState.Slam;
        }

        if (Mathf.Abs(xForce) >= minBreakXForce)
		{
            Collider2D collider = BoxcastTiles(Vector2.right, 1.5f * xForce * Time.fixedDeltaTime);
            if (collider != null && collider.GetComponent<Breakable>() != null)
            {
                collider.GetComponent<Breakable>().Break();
                xForce = -Mathf.Sign(xForce) * breakRecoilForce;
                xVel = xForce;
                dashCountdown = 0;
                ScreenShake();
                Instantiate(particleBurst, transform.position, Quaternion.identity);
                PlayRandomSound(smashSounds);
            }
        }

        Vector2 vel = new Vector2(xVel, yVel);
        rb.velocity = vel;
        rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.collider.gameObject;

        if (collider.CompareTag("Damage"))
        {
            Damage();
        }

        if (collider.layer == LayerMask.NameToLayer("Tiles"))
		{
            if (collision.GetContact(0).normal.x != 0)
            {
                //against wall, not ceiling
                //PlaySound(bonkSound);
                xForce = 0;
                dashCountdown = 0;
                PlaySound(landSound);
            }
        }

        Bouncer bouncer = collider.GetComponent<Bouncer>();
        if (bouncer != null)
        {
            PlaySound(bounceSound);
            StopSlamming();
            dashCountdown = 0;
            canDoubleJump = true;
            canDash = true;
            /*Vector2 playerPos = rb.position + new Vector2(0, ec.points[0].y);
            Vector2 bouncerPos = new Vector2(collider.transform.position.x, collider.transform.position.y);
            Vector2 bouncerToPlayer = (playerPos - bouncerPos).normalized;*/
            Vector2 normal = collision.GetContact(0).normal;
            float bounceYVel = rb.velocity.magnitude * bouncer.bounceForce * normal.y;
            if (normal.y >= 0 && bounceYVel < minBounceForce)
			{
                bounceYVel = minBounceForce;
            }
            if (normal.y < 0 && bounceYVel > -minBounceForce)
            {
                bounceYVel = -minBounceForce;
            }
            float bounceXVel = rb.velocity.magnitude * bouncer.bounceForce * normal.x;
            xForce = bounceXVel;
            rb.velocity = new Vector2(bounceXVel, bounceYVel);
            animState = AnimState.Jump;
        }
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
        GameObject collider = collision.gameObject;

        Portal portal = collider.GetComponent<Portal>();
        if (portal != null)
        {
            GameObject transitionPlayerObj = Instantiate(transitionPlayerPrefab, transform.position, Quaternion.identity);
            TransitionPlayer transitionPlayer = transitionPlayerObj.GetComponent<TransitionPlayer>();
            transitionPlayer.SetPortal(collider);
            gameObject.SetActive(false);
        }

        Checkpoint checkpoint = collider.GetComponent<Checkpoint>();
        if (checkpoint != null)
		{
            respawnPoint = checkpoint.respawnPoint;
		}

        CollectibleSprinkle sprinkle = collider.GetComponent<CollectibleSprinkle>();
        if (sprinkle != null)
		{
            sprinkle.Collect();
            Destroy(collider);
            sprinkles++;
			PlaySound(collectSound);
		}

        RefillCrystal refill = collider.GetComponent<RefillCrystal>();
        if (refill != null && refill.isUsable)
        {
            switch (refill.refillType)
            {
                case RefillCrystal.RefillType.Jump:
                    canDoubleJump = true;
                    break;
                case RefillCrystal.RefillType.Dash:
                    canDash = true;
                    break;
			}
            refill.Use();
			PlaySound(refillSound);
        }
    }

    private void Damage()
    {
        ScreenShake();
        GameObject hurtPlayerObj = Instantiate(hurtPlayerPrefab, transform.position, Quaternion.identity);
        HurtPlayer hurtPlayer = hurtPlayerObj.GetComponent<HurtPlayer>();
        hurtPlayer.SetPlayer(this);
        gameObject.SetActive(false);
    }

    public void Respawn()
    {
        xForce = 0;
        StopSlamming();
        dashCountdown = 0;
        rb.velocity = Vector2.zero;
        rb.position = respawnPoint.transform.position;
    }

	private void TryStopCoroutine(Coroutine crt)
    {
        if (crt != null)
        {
            StopCoroutine(crt);
        }
    }

    private void StopCancelQueuedJump()
    {
        TryStopCoroutine(crtCancelQueuedJump);
    }

    private IEnumerator CancelQueuedJump()
    {
        yield return new WaitForSeconds(jumpBufferTime);
        jumpQueued = false;
    }

    private IEnumerator LeaveGround()
    {
        yield return new WaitForSeconds(jumpGraceTime);
        canJump = false;
    }

    private void StopSlamming()
	{
        isSlamming = false;
        slamLoopAudioSource.Stop();
    }

    private Sprite GetAnimSprite()
    {
        switch (animState)
        {
            case AnimState.Stand:
                return standSprite;
            case AnimState.Run:
                return runSprites[animFrame];
            case AnimState.Jump:
                return jumpSprite;
            case AnimState.Flap:
                return flapSprite;
            case AnimState.Fall:
                return fallSprite;
            case AnimState.Slam:
                return slamSprite;
            case AnimState.Dash:
                return dashSprite;
        }
        return standSprite;
    }

    private void AdvanceAnim()
    {
        if (animState == AnimState.Run)
        {
            frameTime = runFrameTime;
            AdvanceFrame(runSprites.Length);
        }
        else
        {
            animFrame = 0;
            frameTimer = frameTime;
        }
    }

    private void AdvanceFrame(int numFrames)
    {
        if (animFrame >= numFrames)
        {
            animFrame = 0;
        }

        frameTimer -= Time.deltaTime;
        if (frameTimer <= 0)
        {
            frameTimer = frameTime;
            animFrame = (animFrame + 1) % numFrames;
        }
    }

    private void ScreenShake()
	{
        impulseSource.GenerateImpulse();
    }

    public void PlaySound(AudioClip sound, bool randomizePitch = true)
    {
        if (randomizePitch)
        {
            sfxAudioSource.pitch = Random.Range(1 - pitchVariation, 1 + pitchVariation);
        }
        else
        {
            sfxAudioSource.pitch = 1;
        }
        sfxAudioSource.PlayOneShot(sound);
    }

    public void PlayRandomSound(AudioClip[] sounds, bool randomizePitch = true)
	{
        int i = Random.Range(0, sounds.Length);
        AudioClip sound = sounds[i];
        PlaySound(sound, randomizePitch);
	}
}
