using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
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

    private Rigidbody2D rb;
    private Vector2 velocity = Vector2.zero;
    private float acceleration;
    private float deceleration;
    public float maxSpeed;
    public float timeToReachMaxSpeed;
    public float timeToReachDecelerate;
    
    private float gravity;
    private float terminalFallingSpeed;
    private float ungroundedTime = 0f;

    public float apexHeight;
    public float apexTime;
    public float terminalSpeed;
    public float coyoteTime;

    public int currentHealth;

    private bool facingLeft = false;
    private bool didWeJump = false;
    private bool isOnGround = true;

    public LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        gravity = -2 * apexHeight / apexTime;
        terminalFallingSpeed = apexHeight / terminalSpeed;

        acceleration = maxSpeed / timeToReachMaxSpeed;
        deceleration = maxSpeed / timeToReachDecelerate; 

        rb = GetComponent<Rigidbody2D>();
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
        }

        if (Input.GetKeyDown(KeyCode.Space) && isOnGround | ungroundedTime >= coyoteTime)
        {
            didWeJump = true;
            Debug.Log("Jump");
        }

        Debug.Log("Players Velocity; " + velocity);
        Debug.Log("Gravity: " + rb.gravityScale);
        Debug.Log("jump time: " + ungroundedTime);
        Debug.Log(didWeJump);
    }


    void FixedUpdate()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.

        //Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"), 0);
        //MovementUpdate(playerInput);

        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);

        RaycastHit2D onGround = Physics2D.Raycast(rb.position, Vector2.down, 0.3f, groundLayer);
        if (onGround.collider != null)
        {
            isOnGround = true;
            ungroundedTime = 0f;
            Debug.Log("is on ground");
        }
        else
        {
            isOnGround = false;
            ungroundedTime += 1 * Time.fixedDeltaTime;
            Debug.Log("not on ground");
        }
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        //Vector2 movement = new Vector2(playerInput.x * maxSpeed, rb.velocity.y);
        //rb.velocity = movement;

        //Vector2 movment = new Vector2(playerInput.x * maxSpeed, rb.velocity.y);
        //rb.velocity = Vector2.Lerp(rb.velocity, movment, acceleration * Time.deltaTime);

        //Vector2 velocity = rb.velocity;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb.velocity += Vector2.left * acceleration * Time.fixedDeltaTime;
            facingLeft = true;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rb.velocity += Vector2.right * acceleration * Time.fixedDeltaTime;
            facingLeft = false;
        }

        if (rb.velocity.magnitude >= maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        if (rb.velocity != Vector2.zero && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            rb.velocity -= rb.velocity.normalized * deceleration * Time.fixedDeltaTime;
        }

        //rb.velocity = velocity;

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector2.Lerp.html
        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetAxis.html
        //https://stackoverflow.com/questions/32905191/c-sharp-2d-platformer-movement-code

        if (didWeJump && isOnGround)
        {
            rb.gravityScale = gravity;
            rb.velocity += Vector2.up * apexTime * Time.fixedDeltaTime;
            didWeJump = false;
        }

        if (!isOnGround)
        {
            rb.gravityScale = terminalFallingSpeed;
            rb.velocity += Vector2.down * terminalSpeed * Time.fixedDeltaTime;

            if (rb.velocity.magnitude >= terminalSpeed)
            {
                rb.velocity = rb.velocity.normalized * terminalSpeed;
                Debug.Log("normalized");
            }

            rb.gravityScale = 0f;
        }
    }

    public bool IsWalking()
    {
        if (rb.velocity.x != Vector2.zero.x)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool IsGrounded()
    {
        if (isOnGround == false)
        {
            return false;
        }

        return true;

        //https://kylewbanks.com/blog/unity-2d-checking-if-a-character-or-object-is-on-the-ground-using-raycasts#:~:text=Once%20the%20Raycast%20is%20complete%2C%20we%20check%20if,player%20is%20on%20the%20ground%2C%20and%20act%20accordingly.
        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Physics2D.Raycast.html
        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/LayerMask.NameToLayer.html
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
        if (facingLeft == true)
        {
            return FacingDirection.left;
        }

        return FacingDirection.right;
    }
}
