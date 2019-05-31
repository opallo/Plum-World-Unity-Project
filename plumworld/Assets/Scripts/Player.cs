using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    // UI /////////////////////////////////////////
    public GameObject canvas;

    // CONTROLS /////////////////////////////////////////
    Controller2D controller;
    public float moveSpeed = 5;

    // PHYSICS /////////////////////////////////////////
    public Vector3 velocity;
    public float tApex = 1;
    float gravity;
    bool globalCollision = false;
    bool antiGravity;
    Vector3 startPosition;

    // MOVEMENT /////////////////////////////////////////
    public float regSpeed = 5;
    float targetVelocityX;
    float xSmoothing;
    float accGrounded = 0;
    int direction;
    int lastDirection;

    // JUMPING /////////////////////////////////////////
    public float jumpHeight = 5f;
    public float wallJumpHeight = 3f;
    public float wallBounceVelocity = 75f;
    float accAirborne = .1f;
    public float wallJumpTime = .2f;
    float wallJumpTimer = 0;
    public float jumpBuffer = .5f;
    bool canDoubleJump = true;


    void Start()
    {
        controller = GetComponent<Controller2D>();
        startPosition = transform.position;
        Cursor.visible = false;

    }

    void Update()
    {
        CalcVelocity();

        GlobalCollisions();

        ResetTimers();

        UpdateTimers();

        if (Input.anyKey) { DoInputs(); }

        controller.Move(velocity * Time.deltaTime);

        ResetJump();

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }
    }
    void DoInputs()
    {
        if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetMouseButtonDown(0))
        {
            if (controller.collisions.below)
            {
                Jump(jumpHeight, tApex, velocity.x);
                canDoubleJump = true;
            }
            else if (controller.collisions.above)
            {
                Jump(-jumpHeight, tApex, velocity.x);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                Jump(jumpHeight, tApex, velocity.x);
                canDoubleJump = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetMouseButtonDown(1))
        {
            if (wallJumpTimer > 0 && !controller.collisions.below)
            {

                Jump(wallJumpHeight, tApex, wallBounceVelocity);
                canDoubleJump = true;
            }
            //ceiling

            Debug.Log("Wall");
        }
    }
    void UpdateTimers()
    {

        if (wallJumpTimer > 0)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }
    void ResetTimers()
    {
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below)
        {
            wallJumpTimer = wallJumpTime;
        }
    }
    void CalcVelocity()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        Debug.Log(input);

        targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref xSmoothing, accGrounded); /*  (controller.collisions.below == false) ? accAirborne : accGrounded);*/

        gravity = (-2 * jumpHeight / Mathf.Pow(tApex, 2));

        if (antiGravity)
        {

            velocity.y -= gravity * Time.deltaTime;

        }
        else
        {

            velocity.y += gravity * Time.deltaTime;
        }

    }

    void Jump(float _jumpHeight, float _tApex, float _xPush)
    {
        if (controller.collisions.left)
        {
            velocity.x += _xPush;
        }
        else
        {
            velocity.x += -_xPush;
        }
        velocity.y = (2 * _jumpHeight) / _tApex;
        Debug.Log(canDoubleJump);
    }

    void GlobalCollisions()
    {

        if (controller.collisions.above || controller.collisions.below || controller.collisions.left || controller.collisions.right)
        {
            globalCollision = true;
            //antiGravity = true;
        }
        else
        {
            globalCollision = false;
        }
    }

    void ResetJump()
    {
        if (controller.collisions.below) { canDoubleJump = true; }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject != null && other.tag == "KillSurface")
        {
            transform.position = startPosition;
            Debug.Log("Kill");
        }
    }

}


