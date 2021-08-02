using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const float speed = 8;
    private const float jumpForce = 10;
    private const float doubleJumpForce = 10;
    private const float gravityForce = 20;

    private Rigidbody2D rb;
    private EdgeCollider2D ec;
    private bool triggerWasHeld = false;
    private bool jumpQueued = false;
    private bool slamQueued = false;
    private bool canDoubleJump = true;
    private bool isSlamming = false;

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

        bool onGround = CheckSide(4, 3, Vector2.down);
        bool onCeiling = CheckSide(1, 2, Vector2.up);

        if (onGround)
        {
            /*if (!canDoubleJump)
            {
                PlaySound(rechargeDoubleJumpSound);
            }*/
            canDoubleJump = true;

            if (rb.velocity.y < 0)
            {
                //PlaySound(bonkSound);
            }
        }
        float yVel = onGround ? 0 : rb.velocity.y - gravityForce * Time.fixedDeltaTime;
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
                yVel = jumpForce;
                //PlaySound(jumpSound);
            }
            else if (canDoubleJump)
			{
                yVel = doubleJumpForce;
                //PlaySound(doubleJumpSound);
                canDoubleJump = false;
			}
        }

        if (slamQueued)
		{
            slamQueued = false;
            print("slam");
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
}
