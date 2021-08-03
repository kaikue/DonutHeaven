using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const float speed = 8;
    private const float jumpForce = 10;
    private const float doubleJumpForce = 10;
    private const float gravityForce = 20;
    private const float slamForce = 40;
    private const float minBounceForce = 5;

    private Rigidbody2D rb;
    private EdgeCollider2D ec;
    private bool triggerWasHeld = false;
    private bool jumpQueued = false;
    private bool slamQueued = false;
    private bool canDoubleJump = true;
    private bool isSlamming = false;
    private float xForce = 0;

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
            jumpQueued = true;
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
        float xVel = xInput * speed;

        if (xForce != 0)
		{
            //if not moving: keep xForce
            if (xInput == 0)
			{
                xVel = xForce;
			}
            else
			{
                if (Mathf.Sign(xVel) == Mathf.Sign(xForce)) {
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
                    //decrease xForce by xVel (stopping at 0)
                    float prevSign = Mathf.Sign(xForce);
                    xForce += xVel / 4;
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
            yVel = rb.velocity.y - gravityForce * Time.fixedDeltaTime;
        }

        if (onCeiling && yVel > 0)
        {
            yVel = 0;
            //PlaySound(bonkSound);
        }

        if (jumpQueued)
        {
            jumpQueued = false;
            if (onGround)
            {
                yVel = Mathf.Max(jumpForce, yVel + jumpForce);
                //PlaySound(jumpSound);
            }
            else if (canDoubleJump)
			{
                yVel = Mathf.Max(doubleJumpForce, yVel + doubleJumpForce);
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
            yVel = -slamForce;
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
            //TODO add scaled bounce x force
            //TODO clamp to min/max y for bouncing vertical? (terminal velocity)
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
            //rb.velocity = new Vector2(rb.velocity.x, bounceYVel);
            rb.velocity = new Vector2(bounceXVel, bounceYVel);
        }
    }
}
