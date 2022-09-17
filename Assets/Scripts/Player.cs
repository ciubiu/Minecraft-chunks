using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float jumpForce = 8f;
    public float gravity = -9.8f;

    public float playerWidth = 0.6f; // as radious
    public float boundsTolerance = 0.1f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private float mouseSpeed = 3f;
    private Vector3 velocity;
    private float verticalMomentum = 0f;
    private bool jumpRequest;

    public Transform highlightBlock;
    public Transform placingBlock;
    public float checkIncrement = 0.1f;
    public float reach = 4f;

    public ushort selectedBlockType = 1;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("VoxelWorld").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        CalculateVelocity();

        if (jumpRequest)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal); // player object
        cam.transform.localRotation = Quaternion.Euler(-mouseVertical, 0f, 0f);   // camera object

        transform.Translate(velocity, Space.World);
    }


    private void Update()
    {
        GetPlayerInputs();
        PlaceCurcorBlocks();
    }


    void CalculateVelocity()
    {
        //velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.deltaTime * walkSpeed;
        //velocity += Vector3.up * gravity * Time.deltaTime;
        //velocity.y = checkDownSpeed(velocity.y);

        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // sprinting
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // falling and jumping
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);
    }

    void GetPlayerInputs()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        mouseHorizontal = Input.GetAxisRaw("Mouse X") * mouseSpeed;
        mouseVertical += Input.GetAxis("Mouse Y") * mouseSpeed;
        mouseVertical = Mathf.Clamp(mouseVertical, -80f, 80f);


        // Fire3 as Sprint aka Left Shift
        if (Input.GetButtonDown("Fire3"))
            isSprinting = true;
        if (Input.GetButtonUp("Fire3"))
            isSprinting = false;

        if (isGrounded && Input.GetButton("Jump"))
            jumpRequest = true;

        if (highlightBlock.gameObject.activeSelf)
        {
            // Destroy block
            if (Input.GetMouseButtonDown(1))
                world.GetChunkFromVector3(highlightBlock.position).EditBlock(highlightBlock.position, 0);
            // Destroy block
            if (Input.GetMouseButtonDown(0))
                world.GetChunkFromVector3(placingBlock.position).EditBlock(placingBlock.position, selectedBlockType);
        }
    }

    void PlaceCurcorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        var pos = new Vector3();

        while (step < reach)
        {
            pos = cam.position + (cam.forward * step);

            if (world.CheckForBlock(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x),
                                                    Mathf.FloorToInt(pos.y),
                                                    Mathf.FloorToInt(pos.z));
                placingBlock.position = lastPos;
                
                highlightBlock.gameObject.SetActive(true);
                placingBlock.gameObject.SetActive(true);

                return; // exit while
            }
            
            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;
        }
        
        // highlightBlock.gameObject.SetActive(false);
        // placingBlock.gameObject.SetActive(false);

    }
    
    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }

    private float checkDownSpeed(float downSpeed)
    {
        Vector3 tfpos = transform.position;
        // from the center of player check four corners
        //   world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
        if ((world.CheckForBlock(new Vector3(tfpos.x - playerWidth, tfpos.y + downSpeed, tfpos.z - playerWidth) )) ||
            (world.CheckForBlock(new Vector3(tfpos.x + playerWidth, tfpos.y + downSpeed, tfpos.z - playerWidth) )) ||
            (world.CheckForBlock(new Vector3(tfpos.x + playerWidth, tfpos.y + downSpeed, tfpos.z + playerWidth) )) ||
            (world.CheckForBlock(new Vector3(tfpos.x - playerWidth, tfpos.y + downSpeed, tfpos.z + playerWidth) ))
           )
        {
            verticalMomentum = 0;
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }

    private float checkUpSpeed(float upSpeed)
    {
        if ((world.CheckForBlock(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ) ||
            (world.CheckForBlock(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth) )) ||
            (world.CheckForBlock(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ) ||
            (world.CheckForBlock(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) )
           )
        {
            verticalMomentum = 0;  // set to 0 so the player falls when their head hits a block while jumping
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get {
            if (world.CheckForBlock(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForBlock(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth)))

                return true;
            else
                return false;
        }
    }

    public bool back
    {

        get {
            if (world.CheckForBlock(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForBlock(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth)))

                return true;
            else
                return false;
        }

    }
    public bool left
    {
        get {
            if (world.CheckForBlock(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForBlock(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z)))

                return true;
            else
                return false;
        }
    }

    public bool right
    {
        get {
            if (world.CheckForBlock(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForBlock(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z)))

                return true;
            else
                return false;
        }
    }

}
