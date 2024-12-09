using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    public enum CharacterState
    {
        idle, walk, jump, die
    }

    public CharacterState currentState = CharacterState.idle;
    public CharacterState previousState = CharacterState.idle;

    public FacingDirection direction = FacingDirection.right;

    public LayerMask groundLayer;

    private Rigidbody2D playerRB;

    private float acceleration;
    private float deceleration;
    public float maxSpeed;
    public float timeToReachMaxSpeed;
    public float timeToReachDecelerate;

    private bool isWalkingLeft = false;
    private bool isWalkingRight = false;

    private float gravity;
    private float initialJumpVelocity;
    public float apexHeight;
    public float apexTime;
    public float terminalSpeed;
    public float coyoteTime;

    private bool didWeJump = false;
    private bool isOnGround = true;

    private BoxCollider2D boxCollider;
    private Vector2 directionGround = Vector2.down;
    private float distance = 0.1f;
    private float ungroundedTime = 0f;

    private float dashingSpeed;
    private float dashingTime;
    public float dashingDistance;
    public float dashTimer;
    public float dashTimeColddown;
    
    private bool isDashing = false;

    public int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        initialJumpVelocity = 2 * apexHeight / apexTime;

        acceleration = maxSpeed / timeToReachMaxSpeed;
        deceleration = maxSpeed / timeToReachDecelerate;

        dashingSpeed = dashingDistance;

        dashTimeColddown = 0f;

        playerRB = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    // Update is called once per frame
    void Update()
    {

        previousState = currentState;

        if (IsDead())
        {
            currentState = CharacterState.die;
        }

        switch (currentState)
        {
            case CharacterState.idle:
                if (IsWalking())
                {
                    //transition to walk state
                    currentState = CharacterState.walk;
                }
                if (!IsGrounded())
                {
                    //transition to jump state
                    currentState = CharacterState.jump;
                }
                break;
            case CharacterState.walk:
                if (!IsWalking())
                {
                    //transition to the idle state
                    currentState = CharacterState.idle;
                }
                if (!IsGrounded())
                {
                    //transition to jump state
                    currentState = CharacterState.jump;
                }
                break;
            case CharacterState.jump:
                if (IsGrounded())
                {
                    if (IsWalking())
                    {
                        //transition to walk state
                        currentState = CharacterState.walk;
                    }
                    else
                    {
                        //transition to idle state
                        currentState = CharacterState.idle;
                    }
                }
                break;
            case CharacterState.die:
                break;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            isWalkingLeft = true;
        }
        else
        {
            isWalkingLeft = false;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            isWalkingRight = true;
        }
        else
        {
            isWalkingRight = false;
        }

        if (Input.GetKeyDown(KeyCode.Z) && IsGrounded() | ungroundedTime < coyoteTime)
        {
            didWeJump = true;
            //Debug.Log("Jump");
        }

        if (Input.GetKeyDown(KeyCode.X) && dashTimeColddown <= 0)
        {
            isDashing = true;
        }

        //Debug.Log(("Time off the ground: " + ungroundedTime));

        //Debug.Log(("Dash Timer: " + dashingTime));
    }

    void FixedUpdate()
    {
        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);

        CheckForGround();
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 velocity = playerRB.velocity;

        //Debug.Log(velocity);

        if (isWalkingLeft)
        {
            velocity += Vector2.left * acceleration * Time.fixedDeltaTime;
            direction = FacingDirection.left;

            //Debug.Log("Walking left: " + Vector2.left * acceleration * Time.fixedDeltaTime);
        }

        if (isWalkingRight)
        {
            velocity += Vector2.right * acceleration * Time.fixedDeltaTime;
            direction = FacingDirection.right;

            //Debug.Log("Walking right: " + Vector2.right * acceleration * Time.fixedDeltaTime);
        }

        if (velocity.x >= maxSpeed)
        {
            velocity.x = maxSpeed;
        }

        if (velocity.x <= -maxSpeed)
        {
            velocity.x = -maxSpeed;
        }

        if (velocity.x != 0 && !isWalkingLeft && !isWalkingRight)
        {
            if (velocity.x > 0)
            {
                velocity.x -= deceleration * Time.fixedDeltaTime;
            }

            if (velocity.x < 0)
            {
                velocity.x += deceleration * Time.fixedDeltaTime;
            }

            if (velocity.x < deceleration * Time.fixedDeltaTime && velocity.x > -deceleration * Time.fixedDeltaTime)
            {
                velocity.x = 0;
            }
        }

        acceleration = maxSpeed / timeToReachMaxSpeed;

        if (didWeJump && IsGrounded())
        {
            velocity.y = initialJumpVelocity;
            didWeJump = false;

            //Debug.Log("Jumping");
        }

        if (!IsGrounded())
        {
            velocity += Vector2.up * gravity * Time.fixedDeltaTime;

            if (velocity.y < -terminalSpeed)
            {
                velocity.y = -terminalSpeed;
            }

            ungroundedTime += Time.fixedDeltaTime;

            //Debug.Log(("Time off the ground: " + ungroundedTime));
        }
        else
        {
            ungroundedTime = 0f;
        }


        if (isDashing)
        {
            if (direction == FacingDirection.right)
            {
                velocity.x = dashingSpeed;

                Debug.Log("Dash Right: " + dashingSpeed);
            }
            else if (direction == FacingDirection.left)
            {
                velocity.x = -dashingSpeed;

                Debug.Log("Dash Left: " + dashingSpeed);
            }

            dashingTime += Time.fixedDeltaTime;

            if (dashingTime >= dashTimer)
            {
                dashingTime = 0f;
                dashTimeColddown = 2f;
                isDashing = false;
            }
        }

        if (!isDashing)
        {
            dashTimeColddown -= Time.fixedDeltaTime;
        }

        if (velocity.x >= dashingDistance)
        {
            velocity.x = dashingDistance;
        }

        if (velocity.x <= -dashingDistance)
        {
            velocity.x = -dashingDistance;
        }

        playerRB.velocity = velocity;
    }

    public void CheckForGround()
    {
        RaycastHit2D onGround = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, directionGround, distance, groundLayer);

        //Debug.DrawRay(playerRB.position, directionGround * (distance), Color.green);

        if (onGround.collider != null)
        {
            isOnGround = true;
            //Debug.Log("is on ground");
        }
        else
        {
            isOnGround = false;
            //Debug.Log("not on ground");
        }
    }

    public bool IsWalking()
    {
        if (playerRB.velocity.x == 0f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsGrounded()
    {
        if (!isOnGround)
        {
            return false;
        }

        return true;
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    public void OnDeathAnimationComplete()
    {
        gameObject.SetActive(false);
    }

    public FacingDirection GetFacingDirection()
    {
        return direction;
    }
}
