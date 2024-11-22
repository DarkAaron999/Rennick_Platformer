using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left, right
    }

    private Rigidbody2D rb;
    public float acceleration;
    public float moveSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //The input from the player needs to be determined and then passed in the to the MovementUpdate which should
        //manage the actual movement of the character.
        Vector2 playerInput = new Vector2(Input.GetAxis("Horizontal"), 0);
        MovementUpdate(playerInput);
    }

    private void MovementUpdate(Vector2 playerInput)
    {
        //Vector2 movement = new Vector2 (playerInput.x * moveSpeed, rb.velocity.y);
        //rb.velocity = movement;

        Vector2 movment = new Vector2(playerInput.x * moveSpeed, rb.velocity.y);
        rb.velocity = Vector2.Lerp(rb.velocity, movment, acceleration * Time.deltaTime);

        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector2.Lerp.html
        //https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Input.GetAxis.html
        //https://stackoverflow.com/questions/32905191/c-sharp-2d-platformer-movement-code
    }

    public bool IsWalking()
    {
        return false;
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
