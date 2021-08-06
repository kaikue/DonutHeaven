using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private const float maxRunSpeed = 9;
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

    [HideInInspector]
    public int sprinkles = 0;

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        ec = gameObject.GetComponent<EdgeCollider2D>();
        sr = gameObject.GetComponent<SpriteRenderer>();
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

        bool onGround = CheckSide(4, 3, Vector2.down);
        bool onCeiling = CheckSide(1, 2, Vector2.up);

        if (onGround)
        {
            canJump = true;
            canDoubleJump = true;
            isSlamming = false;

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
                    //PlaySound(slamLandSound);
				}
                //PlaySound(landSound);
            }

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
            //PlaySound(bonkSound);
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
                //PlaySound(jumpSound);
                animState = AnimState.Jump;
            }
            else if (canDoubleJump)
            {
                StopCancelQueuedJump();
                jumpQueued = false;
                yVel = doubleJumpForce; //Mathf.Max(doubleJumpForce, yVel + doubleJumpForce);
                //PlaySound(doubleJumpSound);
                canDoubleJump = false;
                isSlamming = false;
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
                isSlamming = false;
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
                yVel = -slamSpeed;
            }

            Vector2 startPoint = rb.position;
            Vector2 endPoint = rb.position + (ec.points[0].y + 1.5f * yVel * Time.fixedDeltaTime) * Vector2.up;
            Collider2D collider = RaycastTiles(startPoint, endPoint);
            if (collider != null && collider.CompareTag("Breakable"))
			{
                Destroy(collider.gameObject);
                isSlamming = false;
                canDoubleJump = true;
                canDash = true;
                yVel = breakRecoilForce;
            }

            animState = AnimState.Slam;
        }

        if (Mathf.Abs(xForce) >= minBreakXForce)
		{
            float offset = 1.5f * xForce * Time.fixedDeltaTime;// * Vector2.right;
            Vector2 startPoint = rb.position;// + ec.points[xForce > 0 ? 2 : 1] + offset;
            Vector2 endPoint = rb.position + (ec.points[xForce > 0 ? 2 : 1].x + offset) * Vector2.right;// + ec.points[xForce > 0 ? 3 : 0] + offset;
            Collider2D collider = RaycastTiles(startPoint, endPoint);
            if (collider != null && collider.CompareTag("Breakable"))
            {
                Destroy(collider.gameObject);
                xForce = -Mathf.Sign(xForce) * breakRecoilForce;
                xVel = xForce;
                dashCountdown = 0;
            }
        }

        Vector2 vel = new Vector2(xVel, yVel);
        rb.velocity = vel;
        rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.collider.gameObject;

        if (collider.layer == LayerMask.NameToLayer("Tiles"))
		{
            if (collision.GetContact(0).normal.x != 0)
            {
                //against wall, not ceiling
                xForce = 0;
            }
        }

        Bouncer bouncer = collider.GetComponent<Bouncer>();
        if (bouncer != null)
        {
            //PlaySound(bounceSound);
            isSlamming = false;
            dashCountdown = 0;
            canDoubleJump = true;
            canDash = true;
            Vector2 playerPos = rb.position + new Vector2(0, ec.points[0].y);
            Vector2 bouncerPos = new Vector2(collider.transform.position.x, collider.transform.position.y);
            Vector2 bouncerToPlayer = (playerPos - bouncerPos).normalized;
            float bounceYVel = rb.velocity.magnitude * bouncer.bounceForce * bouncerToPlayer.y;
            if (bouncerToPlayer.y >= 0 && bounceYVel < minBounceForce)
			{
                bounceYVel = minBounceForce;
            }
            if (bouncerToPlayer.y < 0 && bounceYVel > -minBounceForce)
            {
                bounceYVel = -minBounceForce;
            }
            float bounceXVel = rb.velocity.magnitude * bouncer.bounceForce * bouncerToPlayer.x;
            xForce = bounceXVel;
            rb.velocity = new Vector2(bounceXVel, bounceYVel);
            animState = AnimState.Jump;
        }
    }

	private void OnTriggerEnter2D(Collider2D collision)
	{
        GameObject collider = collision.gameObject;

        CollectibleSprinkle sprinkle = collider.GetComponent<CollectibleSprinkle>();
        if (sprinkle != null)
		{
            Destroy(collider);
            sprinkles++;
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
        }
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
}
