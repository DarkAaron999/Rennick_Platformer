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
        idle, walk, jump, die, dash, onWall
    }

    public CharacterState currentState = CharacterState.idle;
    public CharacterState previousState = CharacterState.idle;

    public FacingDirection direction = FacingDirection.right;

    public LayerMask groundLayer;

    private Rigidbody2D playerRB;

    private float acceleration;
    private float deceleration;

    [Header("Walking")]
    public float maxSpeed;
    public float timeToReachMaxSpeed;
    public float timeToReachDecelerate;

    private bool isWalkingLeft = false;
    private bool isWalkingRight = false;

    private float gravity;
    private float initialJumpVelocity;
    private float ungroundedTime;

    [Header("Jumping")]
    public float apexHeight;
    public float apexTime;
    public float terminalSpeed;
    public float coyoteTime;

    private bool didWeJump = false;
    private bool isOnGround = true;

    private BoxCollider2D boxCollider;
    private Vector2 directionGround = Vector2.down;
    private float distance = 0.5f;

    private float dashingSpeed;
    private float dashingTime;

    [Header("Dashing")]
    public float dashingDistance;
    public float dashTimer;
    public float dashTimeColddown;
    
    private bool isDashing = false;

    private bool didWeWallJump = false;
    private bool onWall = false;
    private float wallJumpVelocity;

    [Header("Wall Jumping")]
    public float wallJumpSpeed;
    public float apexWallHeight;
    public float apexWallTime;

    private bool isWallJumping = false;

    [Header("Health")]
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

        wallJumpVelocity = 2 * apexWallHeight / apexWallTime;

        playerRB = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    // Update is called once per frame
    void Update()
    {

        previousState = currentState;

        if (IsDead())
        {
            //transition to die state
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

                    if (IsOnWall())
                    {
                        //transition to on wall state
                        currentState = CharacterState.onWall;
                    }
                }

                if (IsDashing())
                {
                    //transition to dash state
                    currentState = CharacterState.dash;
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

                    if (IsOnWall())
                    {
                        //transition to on wall state
                        currentState = CharacterState.onWall;
                    }
                }

                if (IsDashing())
                {
                    //transition to dash state
                    currentState = CharacterState.dash;
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

                if (IsDashing())
                {
                    //transition to dash state
                    currentState = CharacterState.dash;
                }

                if (IsOnWall())
                {
                    //transition to on wall state
                    currentState = CharacterState.onWall;
                }
                break;
            case CharacterState.dash:
                if (!IsDashing())
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

                    if (!IsGrounded())
                    {
                        //transition to walk state
                        currentState = CharacterState.jump;

                        if (IsOnWall())
                        {
                            //transition to on wall state
                            currentState = CharacterState.onWall;
                        }
                    }
                }
                break;
            case CharacterState.onWall:
                if (!IsOnWall())
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

                    if (!IsGrounded())
                    {
                        //transition to jump state
                        currentState = CharacterState.jump;
                    }

                    if (IsDashing())
                    {
                        //transition to dash state
                        currentState = CharacterState.dash;
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

        if (Input.GetKeyDown(KeyCode.Z) && IsGrounded() | ungroundedTime > coyoteTime | IsOnWall())
        {
            didWeWallJump = true;
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

        CheckForWall();
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
        }

        if (IsDashing())
        {
            if (direction == FacingDirection.right)
            {
                velocity.x = dashingSpeed;

                //Debug.Log("Dash Right: " + dashingSpeed);
            }
            else if (direction == FacingDirection.left)
            {
                velocity.x = -dashingSpeed;

                //Debug.Log("Dash Left: " + dashingSpeed);
            }

            dashingTime += Time.fixedDeltaTime;

            if (dashingTime >= dashTimer)
            {
                dashingTime = 0f;
                dashTimeColddown = 2f;
                isDashing = false;
            }
        }

        if (!IsDashing())
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

        if (IsWallJumping() && !IsGrounded() && (isWalkingLeft || isWalkingRight))
        {
            velocity.y = 0f;

            if (direction == FacingDirection.left)
            {
                if (didWeWallJump)
                {
                    velocity = new Vector2(wallJumpSpeed, wallJumpVelocity);
                    didWeJump = false;
                }
            }

            if (direction == FacingDirection.right)
            {
                if (didWeWallJump)
                {
                    velocity = new Vector2(-wallJumpSpeed, wallJumpVelocity);
                    didWeJump = false;
                }
            }

            onWall = true;
        }

        if (!IsWallJumping())
        {
            didWeWallJump = false;
            onWall = false;
        }

        playerRB.velocity = velocity;
    }

    public void CheckForGround()
    {
        RaycastHit2D onGround = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x, 0.65f), 0f, directionGround, distance, groundLayer);

        Debug.DrawRay(boxCollider.bounds.center, directionGround * (distance), Color.red);

        if (onGround.collider != null)
        {
            isOnGround = true;
            ungroundedTime = 0f;
            //Debug.Log("is on ground");
        }
        else
        {
            isOnGround = false;
            ungroundedTime += Time.fixedDeltaTime;
            //Debug.Log("not on ground");
        }
    }

    public void CheckForWall()
    {
        if (direction == FacingDirection.left)
        {
            RaycastHit2D onWall = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(0.65f, 0.65f), 0f, Vector2.left, 0.05f, groundLayer);

            if (onWall.collider != null)
            {
                isWallJumping = true;
                ungroundedTime = 0f;
                //Debug.Log("Left: Is on wall");
            }
            else
            {
                isWallJumping = false;
                //Debug.Log("Left: Is not wall");
            }
        }

        if (direction == FacingDirection.right)
        {
            RaycastHit2D onWall = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(0.65f, 0.65f), 0f, Vector2.right, 0.05f, groundLayer);

            if (onWall.collider != null)
            {
                isWallJumping = true;
                ungroundedTime = 0f;
                //Debug.Log("Right: Is on wall");
            }
            else
            {
                isWallJumping = false;
                //Debug.Log("Right: Is not wall");
            }
        }

        //Debug.DrawRay(boxCollider.bounds.center, Vector2.left * (1f), Color.red);
        //Debug.DrawRay(boxCollider.bounds.center, Vector2.right * (1f), Color.red);
    }

    public bool IsWalking()
    {
        if (isWalkingLeft || isWalkingRight)
        {
            return true;
        }

        return false;
    }

    public bool IsGrounded()
    {
        if (isOnGround)
        {
            return true;
        }

        return false;
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    public bool IsDashing()
    {
        if (isDashing)
        {
            return true;
        }

        return false;
    }

    public bool IsWallJumping()
    {
        if (isWallJumping)
        {
            return true;
        }

        return false;
    }

    public bool IsOnWall()
    {
        if (onWall && !IsGrounded() && (isWalkingLeft || isWalkingRight))
        {
            return true;
        }

        return false;
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
