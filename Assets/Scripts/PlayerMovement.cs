using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    // Components
    private Rigidbody2D Rb { get; set; }
    public Vector2 boxExtends;
    public bool IsFacingRight { get; private set; }
    public bool IsJumping { get; private set; }
    private float LastOnGroundTime { get; set; }
    public Vector2 moveInput;
    private float LastPressedJumpTime { get; set; }
    
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

    // Start is called before the first frame update
    private void Start()
    {
        Rb = GetComponent<Rigidbody2D>();
        boxExtends = GetComponent<BoxCollider2D>().bounds.extents;
        SetGravityScale(gravityScale);
        IsFacingRight = true;
        Run();
    }

    // Update is called once per frame
    private void Update()
    {
        LastOnGroundTime -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        
    }

    private static void Run()
    {
        float targetSpeed;
    }

    private void SetGravityScale(float scale)
    {
        Rb.gravityScale = scale;
    }
}
