using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    private Rigidbody2D rb;
    private Vector2 velocity = Vector2.zero;
    private float acceleration;
    private float deceleration;
    public float maxSpeed;
    public float timeToReachMaxSpeed;
    public float timeToReachDecelerate;
    
    private float gravity;
    public float apexHeight;
    public float apexTime;
    private float terminalFallingSpeed;
    private Vector2 initialJumpVelocity = Vector2.zero;

    private bool didWeJump = false;

    // Start is called before the first frame update
    void Start()
    {
        gravity = apexHeight / apexTime;
        terminalFallingSpeed = apexHeight / apexTime;

        acceleration = maxSpeed / timeToReachMaxSpeed;
        deceleration = maxSpeed / timeToReachDecelerate; 

        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            didWeJump = true;
            Debug.Log("Jump");
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.

        //Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"), 0);
        //MovementUpdate(playerInput);

        Vector2 playerInput = new Vector2();
        MovementUpdate(playerInput);

        Debug.Log("Players Velocity; " + velocity);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        //Vector2 movement = new Vector2(playerInput.x * maxSpeed, rb.velocity.y);
        //rb.velocity = movement;

        //Vector2 movment = new Vector2(playerInput.x * maxSpeed, rb.velocity.y);
        //rb.velocity = Vector2.Lerp(rb.velocity, movment, acceleration * Time.deltaTime);

        //velocity = rb.velocity;
        initialJumpVelocity = rb.velocity;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb.velocity += Vector2.left * acceleration * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rb.velocity += Vector2.right * acceleration * Time.deltaTime;
        }

        if (rb.velocity.magnitude >= maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        if (rb.velocity != Vector2.zero && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
        {
            rb.velocity -= rb.velocity.normalized * deceleration * Time.deltaTime;
        }

        if (didWeJump)
        {
            gravity = -2 * apexHeight / apexTime;
            initialJumpVelocity += Vector2.up * gravity * Time.deltaTime;

            didWeJump = false;
        }

        //rb.velocity = velocity;


        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector2.Lerp.html
        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetAxis.html
        //https://stackoverflow.com/questions/32905191/c-sharp-2d-platformer-movement-code
    }

    public bool IsWalking()
    {
        if (rb.velocity == Vector2.zero)
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
        return true;
    }

    public FacingDirection GetFacingDirection()
    {
        return FacingDirection.left;
    }
}
