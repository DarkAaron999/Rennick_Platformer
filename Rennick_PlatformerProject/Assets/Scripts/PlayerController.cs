using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

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
    private float terminalFallingSpeed;
    public float apexHeight;
    public float apexTime;
    public float terminalSpeed;
    public float coyoteTime;

    private bool didWeJump = false;
    private bool isOnGround = true;

    private Vector2 directionGround = Vector2.down;
    private float distance = 0.05f;
    private float ungroundedTime = 0f;

    public int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        gravity = -2 * apexHeight / Mathf.Pow(apexTime, 2);
        terminalFallingSpeed = apexHeight / terminalSpeed;


        acceleration = maxSpeed / timeToReachMaxSpeed;
        deceleration = maxSpeed / timeToReachDecelerate;

        playerRB = GetComponent<Rigidbody2D>();
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

        if (Input.GetKey(KeyCode.RightArrow))
        {
            isWalkingRight = true;
        }

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() | ungroundedTime > coyoteTime)
        {
            didWeJump = true;
            Debug.Log("Jump");
        }

        Debug.Log(("Time off the ground: " + ungroundedTime));
    }


    void FixedUpdate()
    {
        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);

        RaycastHit2D onGround = Physics2D.Raycast(transform.position, directionGround, distance, groundLayer);
        if (onGround.collider != null)
        {
            isOnGround = true;
            Debug.Log("is on ground");
        }
        else
        {
            isOnGround = false;
            Debug.Log("not on ground");
        }
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        Vector2 velocity = playerRB.velocity;

        if (isWalkingLeft)
        {
            velocity += Vector2.left * acceleration * Time.fixedDeltaTime;
            direction = FacingDirection.left;
            isWalkingLeft = false;
        }

        if (isWalkingRight)
        {
            velocity += Vector2.right * acceleration * Time.fixedDeltaTime;
            direction = FacingDirection.right;
            isWalkingRight = false;
        }

        if (velocity.magnitude >= maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        if (velocity != Vector2.zero && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            velocity -= velocity.normalized * maxSpeed / timeToReachDecelerate * Time.fixedDeltaTime;

            if (velocity.magnitude < deceleration)
            {
                velocity = Vector2.zero * Time.fixedDeltaTime;
            }
        }

        acceleration = maxSpeed / timeToReachMaxSpeed;

        if (didWeJump && IsGrounded())
        {
            playerRB.gravityScale = gravity;
            velocity += Vector2.up * gravity * Time.fixedDeltaTime; 
            didWeJump = false;
        }

        if (!IsGrounded())
        {
            ungroundedTime += 1 * Time.fixedDeltaTime;
            playerRB.gravityScale = terminalFallingSpeed;
            velocity += Vector2.down * terminalFallingSpeed * Time.fixedDeltaTime;

            if (velocity.y >= apexHeight)
            {
                velocity = velocity.normalized * apexHeight;
            }

            if (velocity.magnitude >= apexHeight)
            {
                velocity = velocity.normalized * apexHeight;
            }

            playerRB.gravityScale = 0f;

        }
        else
        {
            ungroundedTime = 0f;
        }

        playerRB.velocity = velocity;
    }

    public bool IsWalking()
    {
        if (playerRB.velocity == Vector2.zero)
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
        if (isOnGround == false)
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
