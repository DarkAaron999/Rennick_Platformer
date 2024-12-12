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
    //An enum for left and right character facing direction
    public enum FacingDirection
    {
        left, right
    }

    //An enum for the ainmation for the character
    public enum CharacterState
    {
        idle, walk, jump, die, dash, onWall
    }

    //A character state variable for the current state ainmation
    public CharacterState currentState = CharacterState.idle;
    //A character state variable for the previous state ainmation
    public CharacterState previousState = CharacterState.idle;

    //A facing direction variable for the character facing direction set to right
    public FacingDirection direction = FacingDirection.right;

    //A layer mask variable for ground layer 
    public LayerMask groundLayer;

    //A layer mask variable for spring-loaded platform
    public LayerMask springLoadedPlatform;

    //A rigidbody variable for the players rigidbody
    private Rigidbody2D playerRB;

    //A float variable for the players walking acceleration
    private float acceleration;
    //A float variable for the players walking deceleration
    private float deceleration;

    //A header for walking
    [Header("Walking")]
    //A float variable for the player max walking speed
    public float maxSpeed;
    //A float variable for the player to reach max speed
    public float timeToReachMaxSpeed;
    //A float variable for the player to reach the end of walking
    public float timeToReachDecelerate;

    //A boolean variable for the player walking left 
    private bool isWalkingLeft = false;
    //A boolean variable for the player walking right
    private bool isWalkingRight = false;

    //A header for jumping
    [Header("Jumping")]
    //A float variable for the players max jump height 
    public float apexHeight;
    //A float variable for the player to reach the jump height
    public float apexTime;
    //A float variable for the players falling speed
    public float terminalSpeed;
    //A float variable for the player to jump again
    public float coyoteTime;

    //A float variable for the players gravity
    private float gravity;
    //A float variable for the player initial jump 
    private float initialJumpVelocity;
    //A float variable for when the player is off the ground
    private float ungroundedTime;
    //A boolean variable when the player jumps
    private bool didWeJump = false;
    //A boolean variable when the player is on the ground
    private bool isOnGround = true;

    //A box collider variable for the players box collider 
    private BoxCollider2D boxCollider;
    //A vector2 variable for the direction of the ground
    private Vector2 directionGround = Vector2.down;
    //A flaot variable for the distance
    private float distance = 0.5f;

    //A header for dashing
    [Header("Dashing")]
    //A flaot variable for the dashing distance
    public float dashingDistance;
    //A float variable for the dash timer
    public float dashTimer;
    //A float variable for the colddown of the dash
    public float dashTimeColddown;

    //A float variable for the dashing speed
    private float dashingSpeed;
    //A float variable for the dashing time
    private float dashingTime;
    //A boolean variable when the player dashes
    private bool isDashing = false;

    //A header for wall jumping
    [Header("Wall Jumping")]
    //A float variable for the players wall jump speed
    public float wallJumpSpeed;
    //A float variable for the players wall jump height
    public float apexWallHeight;
    //A float variable when the player will reach the wall jump height
    public float apexWallTime;

    //A boolean variable when the player wall jumps
    private bool didWeWallJump = false;
    //A boolean variable when the player is on the wall
    private bool onWall = false;
    //A float variable for the velocity of the wall jump
    private float wallJumpVelocity;

    //A boolean variable when the player is wall jumping
    private bool isWallJumping = false;

    //A header for the spring-loaded platform
    [Header("Spring-Loaded Jump")]
    //A float variable for the spring-loaded platform, for the players jump height 
    public float apexSpringHeight;
    //A float variable when the player will reach the spring-loaded jump height
    public float apexSpringTime;

    //A boolean variable when the player is on the spring-loaded platform
    private bool isOnSpringLoadedPlatform = false;
    //A float variable for the spring-loaded platform, for the jump velocity
    private float springLoadedJumpVelocity;

    //A header for health
    [Header("Health")]
    //A int variable for the players health
    public int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        //At start sets the players gravity
        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        //At start sets the players initial jump velocity
        initialJumpVelocity = 2 * apexHeight / apexTime;

        //At start sets the players acceleration
        acceleration = maxSpeed / timeToReachMaxSpeed;
        //At start sets the players deceleration
        deceleration = maxSpeed / timeToReachDecelerate;

        //At start sets the dashing speed
        dashingSpeed = dashingDistance;

        //At start sets the dash time colddown
        dashTimeColddown = 0f;

        //At start sets the players wall jumping velocity
        wallJumpVelocity = 2 * apexWallHeight / apexWallTime;

        //At start sets the players spring-loaded platform velocity
        springLoadedJumpVelocity = 2 * apexSpringHeight / apexSpringTime;

        //At start sets the players rigidbody
        playerRB = GetComponent<Rigidbody2D>();
        //At start sets the player box collider
        boxCollider = GetComponent<BoxCollider2D>();
    }
    // Update is called once per frame
    void Update()
    {
        //Previous state is equal to current state
        previousState = currentState;

        //Id Isdead is true 
        if (IsDead())
        {
            //transition to die state
            currentState = CharacterState.die;
        }

        //Switch the current state
        switch (currentState)
        {
            case CharacterState.idle:
                //If IsWalking is ture
                if (IsWalking())
                {
                    //transition to walk state
                    currentState = CharacterState.walk;
                }
                //If IsGrounded is false
                if (!IsGrounded())
                {
                    //transition to jump state
                    currentState = CharacterState.jump;
                    
                    //If IsOnWall is ture
                    if (IsOnWall())
                    {
                        //transition to on wall state
                        currentState = CharacterState.onWall;
                    }
                }
                //If IsDashing is ture
                if (IsDashing())
                {
                    //transition to dash state
                    currentState = CharacterState.dash;
                }
                break;
            case CharacterState.walk:
                //If IsWalking is false
                if (!IsWalking())
                {
                    //transition to the idle state
                    currentState = CharacterState.idle;
                }
                //If IsGrounded is false
                if (!IsGrounded())
                {
                    //transition to jump state
                    currentState = CharacterState.jump;

                    //If IsOnWall is ture
                    if (IsOnWall())
                    {
                        //transition to on wall state
                        currentState = CharacterState.onWall;
                    }
                }
                //If IsDashing is ture
                if (IsDashing())
                {
                    //transition to dash state
                    currentState = CharacterState.dash;
                }
                break;
            case CharacterState.jump:
                //If IsGrounded is ture
                if (IsGrounded())
                {
                    //If IsWalking is true
                    if (IsWalking())
                    {
                        //transition to walk state
                        currentState = CharacterState.walk;
                    }
                    //else IsWalking is false
                    else
                    {
                        //transition to idle state
                        currentState = CharacterState.idle;
                    }
                }
                //If IsDashing is ture
                if (IsDashing())
                {
                    //transition to dash state
                    currentState = CharacterState.dash;
                }
                //If IsOnWall is ture
                if (IsOnWall())
                {
                    //transition to on wall state
                    currentState = CharacterState.onWall;
                }
                break;
            case CharacterState.dash:
                //If IsDashing is false
                if (!IsDashing())
                {
                    //If IsWalking is true
                    if (IsWalking())
                    {
                        //transition to walk state
                        currentState = CharacterState.walk;
                    }
                    //else IsWalking is false
                    else
                    {
                        //transition to idle state
                        currentState = CharacterState.idle;
                    }
                    //If IsGrounded is false
                    if (!IsGrounded())
                    {
                        //transition to walk state
                        currentState = CharacterState.jump;

                        //If IsOnWall is ture
                        if (IsOnWall())
                        {
                            //transition to on wall state
                            currentState = CharacterState.onWall;
                        }
                    }
                }
                break;
            case CharacterState.onWall:
                //If IsOnWall is false
                if (!IsOnWall())
                {
                    //If IsWalking is true
                    if (IsWalking())
                    {
                        //transition to walk state
                        currentState = CharacterState.walk;
                    }
                    //else IsWalking is false
                    else
                    {
                        //transition to idle state
                        currentState = CharacterState.idle;
                    }
                    //If IsGrounded is false
                    if (!IsGrounded())
                    {
                        //transition to jump state
                        currentState = CharacterState.jump;
                    }
                    //If IsDashing is ture
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
        //If left arrow key is held down
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            //Set isWalkingLeft to true
            isWalkingLeft = true;
        }
        //else left arrow key is not held down
        else
        {
            //Set isWalkingLeft to false
            isWalkingLeft = false;
        }
        //If right arrow key is held down
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //Set isWalkingRight to true
            isWalkingRight = true;
        }
        //else right arrow key is not held down
        else
        {
            //Set isWalkingRight to false
            isWalkingRight = false;
        }

        //If the Z key is pressed and IsGrounded is true or ungroundtime is greater than coyoteTime or IsOnWall is true
        if (Input.GetKeyDown(KeyCode.Z) && IsGrounded() | ungroundedTime > coyoteTime | IsOnWall())
        {
            //Sets didWeWallJump to true
            didWeWallJump = true;
            //Set didWeJump to true
            didWeJump = true;

            //Testing if we jumped
            //Debug.Log("Jump");
        }

        //If the X key is pressed and dashTimeColddoen is less than or equal to zero
        if (Input.GetKeyDown(KeyCode.X) && dashTimeColddown <= 0)
        {
            //Sets isDashing to true
            isDashing = true;
        }

        //Debug.Log(("Time off the ground: " + ungroundedTime));
        //Debug.Log(("Dash Timer: " + dashingTime));
    }

    void FixedUpdate()
    {
        //Updates the players input equal to a new vector2
        Vector2 playerInput = new Vector2();
        //Updates the players movement
        MovementUpdate(playerInput);

        //Runs the CheckForGround method
        CheckForGround();

        //Runs the CheckForWall method
        CheckForWall();

        //Runs the CheckSpringLoadedPlatform
        CheckSpringLoadedPlatform();
    }

    //Movemnt update method
    private void MovementUpdate(Vector2 playerInput)
    {
        //Sets vector2 velocity variable to the players rididbody velocity 
        Vector2 velocity = playerRB.velocity;

        //Debug.Log(velocity);

        //If isWalkingLeft is ture
        if (isWalkingLeft)
        {
            //Makes the player move left 
            velocity += Vector2.left * acceleration * Time.fixedDeltaTime;
            //Switches the facing direction of the player to the left
            direction = FacingDirection.left;

            //Debug.Log("Walking left: " + Vector2.left * acceleration * Time.fixedDeltaTime);
        }

        //If isWalkingRight is ture
        if (isWalkingRight)
        {
            //Makes the player move right
            velocity += Vector2.right * acceleration * Time.fixedDeltaTime;
            //Switches the facing direction of the player to the right
            direction = FacingDirection.right;

            //Debug.Log("Walking right: " + Vector2.right * acceleration * Time.fixedDeltaTime);
        }

        //If the velocity.x is Greater-than or equal to the maxSpeed
        if (velocity.x >= maxSpeed)
        {
            //Set the velocity.x to the maxSpeed
            velocity.x = maxSpeed;
        }

        //If the velocity.x is less-than or equal to the -maxSpeed
        if (velocity.x <= -maxSpeed)
        {
            //Set the velocity.x to the -maxSpeed
            velocity.x = -maxSpeed;
        }

        //If velocity.x is not zero and is not isWalkingLeft and is not isWalkingRight
        if (velocity.x != 0 && !isWalkingLeft && !isWalkingRight)
        {
            //If the velocity.x is Greater-than zero
            if (velocity.x > 0)
            {
                //Slows the player down
                velocity.x -= deceleration * Time.fixedDeltaTime;
            }

            //If the velocity.x is less-than zero
            if (velocity.x < 0)
            {
                //Slows the player down
                velocity.x += deceleration * Time.fixedDeltaTime;
            }

            //If the velocity.x is less-than deceleration times Time.fixedDeltalTime and velocity.x Greater-than minus deceleration times Time.fixedDeltalTime 
            if (velocity.x < deceleration * Time.fixedDeltaTime && velocity.x > -deceleration * Time.fixedDeltaTime)
            {
                //Set the velocity.x to zero
                velocity.x = 0;
            }
        }

        //Sets the acceleration to maxSpeed half timeToReachMaxSpeed
        acceleration = maxSpeed / timeToReachMaxSpeed;

        //If didWeJump is true and IsGrounded is true
        if (didWeJump && IsGrounded())
        {
            //Set the velocity.y to initialJumpVelocity
            velocity.y = initialJumpVelocity;
            //Set didWeJump to false
            didWeJump = false;

            //Debug.Log("Jumping");
        }

        //If IsGrounded is false
        if (!IsGrounded())
        {
            //Makes the player fall
            velocity += Vector2.up * gravity * Time.fixedDeltaTime;

            //If velocity.y less-than minus terminalSpeed 
            if (velocity.y < -terminalSpeed)
            {
                //Set velocity.y to minus terminalSpeed
                velocity.y = -terminalSpeed;
            }
        }

        //If IsDashing is true
        if (IsDashing())
        {
            //If direction is equal to FacingDirection.right
            if (direction == FacingDirection.right)
            {
                //Set velocity.x to dashingSpeed
                velocity.x = dashingSpeed;

                //Debug.Log("Dash Right: " + dashingSpeed);
            }
            //Else if direction is equal to FacingDirection.left
            else if (direction == FacingDirection.left)
            {
                //Set velocity.x to minus dashingSpeed
                velocity.x = -dashingSpeed;

                //Debug.Log("Dash Left: " + dashingSpeed);
            }

            //Makes the dashingTime count up
            dashingTime += Time.fixedDeltaTime;

            //If dashingTime is Greater-than or equal to dashTimer 
            if (dashingTime >= dashTimer)
            {
                //Set dashingTime to zero
                dashingTime = 0f;
                //Set dashTimeColddown to two
                dashTimeColddown = 2f;
                //Set isDashing to false
                isDashing = false;
            }
        }

        //If IsDashing is false
        if (!IsDashing())
        {
            //Counts down the dashTimeColddown
            dashTimeColddown -= Time.fixedDeltaTime;
        }

        //If velocity.x is Greater-than or equal dashingDistance
        if (velocity.x >= dashingDistance)
        {
            //Set velocity.x to dasingDistance
            velocity.x = dashingDistance;
        }

        //If velocity.x is less-than or equal minus dashingDistance
        if (velocity.x <= -dashingDistance)
        {
            //Set velocity.x to minus dasingDistance
            velocity.x = -dashingDistance;
        }

        //If IsWallJumping is true and IsGrounded is false and isWalkingleft or isWalkingRight is true
        if (IsWallJumping() && !IsGrounded() && (isWalkingLeft || isWalkingRight))
        {
            //Set velocity.y to zero
            velocity.y = 0f;

            //If firection is equal to FacingDirection.left
            if (direction == FacingDirection.left)
            {
                //If didWeWallJump is true
                if (didWeWallJump)
                {
                    //Makes the player jump off the wall
                    velocity = new Vector2(wallJumpSpeed, wallJumpVelocity);
                    //Set didWeJump to false
                    didWeJump = false;
                }
            }

            //If firection is equal to FacingDirection.right
            if (direction == FacingDirection.right)
            {
                //If didWeWallJump is true
                if (didWeWallJump)
                {
                    //Makes the player jump off the wall
                    velocity = new Vector2(-wallJumpSpeed, wallJumpVelocity);
                    //Set didWeJump to false
                    didWeJump = false;
                }
            }

            //Set onWall to true
            onWall = true;
        }

        //If IsWallJumping is false
        if (!IsWallJumping())
        {
            //Set didWeWallJump to false
            didWeWallJump = false;
            //Set onWall to false
            onWall = false;
        }

        //If isOnSpringLoadedPlatform is true
        if (isOnSpringLoadedPlatform)
        {
            //Set velocity.y to springLoadedJumpVelocity
            velocity.y = springLoadedJumpVelocity;
        }

        //Set playerBR.velocity to velocity
        playerRB.velocity = velocity;
    }

    //CheckForGround method
    public void CheckForGround()
    {
        //Castes a box from the players box collider downwards to detect the ground layer
        RaycastHit2D onGround = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x, 0.65f), 0f, directionGround, distance, groundLayer);

        Debug.DrawRay(boxCollider.bounds.center, directionGround * (distance), Color.red);

        //If onGround collider is not equal to null
        if (onGround.collider != null)
        {
            //Set isOnGround to true
            isOnGround = true;
            //Set ungroundedTime to zero
            ungroundedTime = 0f;
            //Debug.Log("is on ground");
        }
        //Else onGround collider is equal to null 
        else
        {
            //Set isOnGround to false
            isOnGround = false;
            //ungroundedTime counts up
            ungroundedTime += Time.fixedDeltaTime;
            //Debug.Log("not on ground");
        }
    }

    //CheckForWall method
    public void CheckForWall()
    {
        //If direction is equal to FacingDirection.left
        if (direction == FacingDirection.left)
        {
            //Castes a box from the players box collider to the left to detect the ground layer
            RaycastHit2D onWall = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(0.65f, 0.65f), 0f, Vector2.left, 0.05f, groundLayer);

            //If onWall collider is not equal to null
            if (onWall.collider != null)
            {
                //Set isWallJumping to true
                isWallJumping = true;
                //Set ungroundedTime to zero
                ungroundedTime = 0f;
                //Debug.Log("Left: Is on wall");
            }
            //Else onWall collider is equal to null 
            else
            {
                //Set isWallJumping to false
                isWallJumping = false;
                //Debug.Log("Left: Is not wall");
            }
        }

        //If direction is equal to FacingDirection.right
        if (direction == FacingDirection.right)
        {
            //Castes a box from the players box collider to the right to detect the ground layer
            RaycastHit2D onWall = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(0.65f, 0.65f), 0f, Vector2.right, 0.05f, groundLayer);

            //If onWall collider is not equal to null
            if (onWall.collider != null)
            {
                //Set isWallJumping to true
                isWallJumping = true;
                //Set ungroundedTime to zero
                ungroundedTime = 0f;
                //Debug.Log("Right: Is on wall");
            }
            //Else onWall collider is equal to null 
            else
            {
                //Set isWallJumping to false
                isWallJumping = false;
                //Debug.Log("Right: Is not wall");
            }
        }

        //Debug.DrawRay(boxCollider.bounds.center, Vector2.left * (1f), Color.red);
        //Debug.DrawRay(boxCollider.bounds.center, Vector2.right * (1f), Color.red);
    }

    //CheckSpringLoadedPlatform method
    public void CheckSpringLoadedPlatform()
    {
        //Castes a box from the players box collider downwards to detect the spring-loaded platform layer
        RaycastHit2D onSpringLoadedPlatform = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(0.4f, 1f), 0f, Vector2.down, 0.2f, springLoadedPlatform);

        //If onSpringLoadedPlatform collider is not equal to null
        if (onSpringLoadedPlatform.collider != null)
        {
            //Set onSpringLoadedPlatform to true
            isOnSpringLoadedPlatform = true;
            //Debug.Log("Is on spring-loaded platform");
        }
        //Else onSpringLoadedPlatform collider is equal to null 
        else
        {
            //Set onSpringLoadedPlatform to false
            isOnSpringLoadedPlatform = false;
            //Debug.Log("Is not on spring-loaded platform");
        }
    }

    //IsWalking Boolean method
    public bool IsWalking()
    {
        //If isWalkingLeft or isWalkingRight
        if (isWalkingLeft || isWalkingRight)
        {
            //Set IsWalking to true
            return true;
        }

        //Set IsWalking to false
        return false;
    }

    //IsGrounded Boolean method
    public bool IsGrounded()
    {
        //If isOnGround is true
        if (isOnGround)
        {
            //Set IsGrounded to true
            return true;
        }

        //Set IsGrounded to false
        return false;
    }

    //IsDead Boolean method
    public bool IsDead()
    {
        //Set IsDead to ture when currentHealth is less-than or equal to zero
        return currentHealth <= 0f;
    }

    //IsDashing Boolean method
    public bool IsDashing()
    {
        //If isDashing is true
        if (isDashing)
        {
            //Set IsDashing to true
            return true;
        }

        //Set IsDashing to false
        return false;
    }

    //IsWallJumping Boolean method
    public bool IsWallJumping()
    {
        //If isWallJumping is true
        if (isWallJumping)
        {
            //Set IsWallJumping to true
            return true;
        }

        //Set IsWallJumping to false
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

    //OnDeathAnimationComplete method
    public void OnDeathAnimationComplete()
    {
        //Set gameObject.SetActive to false //Turns the player character off
        gameObject.SetActive(false);
    }

    //FacingDirection method
    public FacingDirection GetFacingDirection()
    {
        //Sets the FacingDirection
        return direction;
    }
}
