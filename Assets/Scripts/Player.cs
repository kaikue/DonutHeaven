using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const float runAcceleration = 20;
    private const float maxRunSpeed = 9;
    private const float jumpForce = 10;
    private const float doubleJumpForce = 10;
    private const float gravityForce = 20;
    private const float maxFallSpeed = 30;
    private const float slamSpeed = 35;
    private const float minBounceForce = 5;

    private Rigidbody2D rb;
    private EdgeCollider2D ec;

    private bool triggerWasHeld = false;
    private bool jumpQueued = false;
    private bool slamQueued = false;
    private bool canDoubleJump = true;
    private bool isSlamming = false;
    private float xForce = 0;

    private bool canJump = false;
    private bool wasOnGround = false;
    private Coroutine crtCancelQueuedJump;
    private const float jumpBufferTime = 0.1f; //time before hitting ground a jump will still be queued
    private const float jumpGraceTime = 0.1f; //time after leaving ground player can still jump (coyote time)

    private SpriteRenderer sr;

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

        bool triggerHeld = Input.GetAxis("LTrigger") > 0 || Input.GetAxis("RTrigger") > 0;
        bool triggerPressed = !triggerWasHeld && triggerHeld;
        if (Input.GetButtonDown("Slam") || triggerPressed)
        {
            slamQueued = true;
        }
        triggerWasHeld = triggerHeld;
    }

    private bool CheckSide(int point0, int point1, Vector2 direction)
    {
        Vector2 startPoint = rb.position + ec.points[point0] + direction * 0.1f;
        Vector2 endPoint = rb.position + ec.points[point1] + direction * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(startPoint, endPoint - startPoint, Vector2.Distance(startPoint, endPoint), LayerMask.GetMask("Tiles"));
        return hit.collider != null;
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

        float yVel;

        bool onGround = CheckSide(4, 3, Vector2.down);
        bool onCeiling = CheckSide(1, 2, Vector2.up);

        if (onGround)
        {
            /*if (!canDoubleJump)
            {
                PlaySound(rechargeDoubleJumpSound);
            }*/
            canJump = true;
            canDoubleJump = true;

            isSlamming = false;
            xForce = 0;
            yVel = 0;

            if (rb.velocity.y < 0)
            {
                //PlaySound(bonkSound);
            }
        }
        else
		{
            yVel = Mathf.Max(rb.velocity.y - gravityForce * Time.fixedDeltaTime, -maxFallSpeed);

            if (wasOnGround)
			{
                StartCoroutine(LeaveGround());
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
            }
            else if (canDoubleJump)
            {
                StopCancelQueuedJump();
                jumpQueued = false;
                yVel = doubleJumpForce; //Mathf.Max(doubleJumpForce, yVel + doubleJumpForce);
                //PlaySound(doubleJumpSound);
                canDoubleJump = false;
			}
        }

        if (slamQueued)
		{
            slamQueued = false;
            print("slam");
            isSlamming = true;
		}

        if (isSlamming)
		{
            yVel = -slamSpeed;
		}

        Vector2 vel = new Vector2(xVel, yVel);
        rb.velocity = vel;
        rb.MovePosition(rb.position + vel * Time.fixedDeltaTime);

        /*if (!onGround)
        {
            sr.sprite = walkImage;
        }
        else
        {
            if (xVel != 0)
            {
                sr.sprite = isStepping ? walkImage : standImage;
                timeSinceLastStep += Time.fixedDeltaTime;
                if (timeSinceLastStep > stepTime)
                {
                    timeSinceLastStep = 0;
                    isStepping = !isStepping;
                }
            }
            else
            {
                sr.sprite = standImage;
            }
        }*/
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
            canDoubleJump = true;
            Vector2 playerPos = rb.position + new Vector2(0, ec.points[0].y);
            Vector2 bouncerPos = new Vector2(collider.transform.position.x, collider.transform.position.y);
            Vector2 bouncerToPlayer = (playerPos - bouncerPos).normalized;
            print(bouncerToPlayer);
            float bounceYVel = -rb.velocity.y * bouncer.bounceForce * Mathf.Abs(bouncerToPlayer.y);
            if (bouncerToPlayer.y >= 0 && bounceYVel < minBounceForce)
			{
                bounceYVel = minBounceForce;
            }
            if (bouncerToPlayer.y < 0 && bounceYVel > -minBounceForce)
            {
                bounceYVel = -minBounceForce;
            }
            float bounceXVel = Mathf.Abs(rb.velocity.y) * bouncer.bounceForce * bouncerToPlayer.x;
            xForce = bounceXVel;
            print("bounce " + bounceXVel + ", " + bounceYVel);
            rb.velocity = new Vector2(bounceXVel, bounceYVel);
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
}
