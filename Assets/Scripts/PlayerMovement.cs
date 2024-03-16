using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Components
    private Rigidbody2D Rb { get; set; }
    public Vector2 boxExtends;
    private bool IsFacingRight { get; set; }
    public bool IsJumping { get; private set; }
    private float LastOnGroundTime { get; set; }
    public Vector2 moveInput;
    private float LastPressedJumpTime { get; set; }
    [SerializeField] private LayerMask ground;
    
    // Running
    [Space] [Header("Running Variables")]
    [Range(0f, 1f)]   public float accelerationInAir; // Multipliers are applied to acceleration when air-born
    [Range(0f, 1f)]   public float decelerationInAir;
    public float moveSpeed;
    public float accelerationTime;
    public float decelerationTime;
    public float velocityPower;
    public float frictionAmount;
    [HideInInspector] public float acceleration;
    [HideInInspector] public float deceleration;
    
    // Jumping
    [Space] [Header("Jumping Variables")]
    public float jumpHeight;
    public float jumpTimeToTopOfJump;
    public float jumpCutGravMultiplier;
    public float jumpHangTimeThreshold;
    public float jumpHangAccelerationMultiplier;
    public float jumpHangMaxSpeedMultiplier;
    [HideInInspector] public float jumpForce;
    [Range(0f, 1f)]   public float jumpHangGravMultiplier;
    private bool _jumpCut;
    private bool _jumpFalling;
    
    // Gravity
    [Space] [Header("Gravity Variables")]
    public float fallingGravityMultiplier;
    public float maxFallSpeed;
    public float fastFallGravityMultiplier;
    public float maxFastFallSpeed;
    [HideInInspector] public float gravityStrength;
    [HideInInspector] public float gravityScale;
    
    // other
    [Space] [Header("Other Variables")] 
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime;
    [Range(0.01f, 0.5f)] public float coyoteTime;

    // Start is called before the first frame update
    private void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        boxExtends = GetComponent<BoxCollider2D>().bounds.extents;
        SetGravityScale(gravityScale);
        IsFacingRight = true;
    }

    private void FixedUpdate()
    {
        Run();
    }

    // Update is called once per frame
    private void Update()
    {
        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        float xSpeed = Mathf.Abs(Rb.velocity.x);
        float ySpeed = Mathf.Abs(Rb.velocity.y);
        
        if (moveInput.x != 0) CheckDirection(moveInput.x > 0);
        
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) OnJumpInput();
        
        //ground checks
        if (!IsJumping)
        {
            if (GroundCheck()) LastOnGroundTime = coyoteTime;
        }
        
        //friction
        if (LastOnGroundTime > 0 && Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            float amount = Mathf.Min(Mathf.Abs(Rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(Rb.velocity.x);
            
            Rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        
        //jump checks
        if (IsJumping && Rb.velocity.y < 0) IsJumping = false;
        
        if (LastOnGroundTime > 0 && !IsJumping)
        {
            if (!IsJumping) _jumpFalling = false;
            _jumpCut = false;
        }
        
        if (CanJump() && LastPressedJumpTime > 0)
        {
            IsJumping = true;
            _jumpCut = false;
            _jumpFalling = false;
            Jump();
        }
        
        //gravity checks for jumping
        if (Rb.velocity.y < 0)
        {
            SetGravityScale(gravityScale*jumpCutGravMultiplier);
            //caps maximum fall speed
            Rb.velocity = new Vector2(Rb.velocity.x, Mathf.Max(Rb.velocity.y, -maxFastFallSpeed));
        }
        else if (_jumpCut)
        {
            //higher gravity if jump button released
            SetGravityScale(gravityScale * fastFallGravityMultiplier);
            Rb.velocity = new Vector2(Rb.velocity.x, Mathf.Max(Rb.velocity.y, -maxFallSpeed));
        }
        else if ((IsJumping || _jumpFalling) && Mathf.Abs(Rb.velocity.y) < jumpHangTimeThreshold)
        {
            SetGravityScale(gravityScale * jumpHangGravMultiplier);
        }
        else
        {
            //default gravity
            SetGravityScale(gravityScale);
        }
    }

    private void Run()
    {
        float targetSpeed = moveInput.x * moveSpeed;
        targetSpeed = Mathf.Lerp(Rb.velocity.x, targetSpeed, 1);
        float accelerationRate = 0;
        
        if (LastOnGroundTime > 0)
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        }
        else
        {
            accelerationRate = (Mathf.Abs(targetSpeed) > 0.01f)
                ? acceleration * accelerationInAir
                : deceleration * decelerationInAir;
        }
        
        if ((IsJumping || _jumpFalling) && Mathf.Abs(Rb.velocity.y) < jumpHangTimeThreshold)
        {
            accelerationRate *= jumpHangAccelerationMultiplier;
            targetSpeed *= jumpHangMaxSpeedMultiplier;
        }

        float speedDifferance = targetSpeed - Rb.velocity.x;
        float movement = Mathf.Pow(Mathf.Abs(speedDifferance) * accelerationRate, velocityPower) *
                         Mathf.Sign(speedDifferance);

        Rb.AddForce(movement * Vector2.right);
    }

    private void Jump()
    {
        LastPressedJumpTime = 0f;
        LastOnGroundTime = 0f;
        float force = jumpForce;
        
        if (Rb.velocity.y < 0) force -= Rb.velocity.y;
        
        Rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }
    private void OnJumpInput()
    {
        LastPressedJumpTime = jumpInputBufferTime;
    }
    private void SetGravityScale(float scale)
    {
        Rb.gravityScale = scale;
    }
    private void Turn()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        IsFacingRight = !IsFacingRight;
    }

    public void CheckDirection(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight) Turn();
    }
    private bool CanJump() {
        return LastOnGroundTime > 0 && !IsJumping;
    }
    private bool CanJumpCut() {
        return IsJumping && Rb.velocity.y > 0;
    }
    private bool GroundCheck()
    {
        Vector3 position = transform.position;
        Vector2 bottom = new Vector2(position.x, position.y - boxExtends.y);
        Vector2 hitBoxSize = new Vector2(boxExtends.x * 2.0f, 0.05f);
        RaycastHit2D result = Physics2D.BoxCast(bottom, hitBoxSize, 0.0f, new Vector3(0.0f, -1.0f), 0.0f, ground);
        bool grounded = result.collider != null && result.normal.y > 0.9f;
        return grounded;
    }

    private void OnValidate()
    {
        //running acceleration
        acceleration = (50 * accelerationTime) / moveSpeed;
        deceleration = (50 * decelerationTime) / moveSpeed;
        accelerationTime = Mathf.Clamp(accelerationTime, 0.01f, moveSpeed);
        decelerationTime = Mathf.Clamp(decelerationTime, 0.01f, moveSpeed);
        //jump calculations
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToTopOfJump * jumpTimeToTopOfJump);
        gravityScale = gravityStrength / Physics2D.gravity.y;
        //jump force
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToTopOfJump;
    }
}
