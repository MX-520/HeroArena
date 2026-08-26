using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerMovement : MonoBehaviour
{
    // ======================
    // Movement Settings
    // ======================

    [SerializeField] private float speed = 5f;
    [SerializeField] private float runSpeed = 8f;


    // ======================
    // Roll Settings
    // ======================

    [SerializeField] private float rollDuration = 0.25f;
    [SerializeField] private float rollSpeed = 20f;


    // ======================
    // Components
    // ======================

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;


    // ======================
    // Runtime Data
    // ======================

    private Vector2 moveInput;
    private Vector2 lastMoveDirection = Vector2.right;

    private bool isRunning;
    private bool isRolling;



    // ======================
    // Unity Methods
    // ======================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }


    private void FixedUpdate()
    {
        if (isRolling)
        {
            return;
        }

        Move();
        UpdateAnimation();
        UpdateDirection();
        UpdateLastMoveDirection();
    }



    // ======================
    // Input
    // ======================

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }


    public void OnRun(InputValue value)
    {
        isRunning = value.Get<float>() > 0;
    }


    public void OnRoll(InputValue value)
    {
        if (value.isPressed && !isRolling)
        {
            StartCoroutine(RollCoroutine());
        }
    }



    // ======================
    // Movement
    // ======================

    private void Move()
    {
        float currentSpeed = isRunning ? runSpeed : speed;

        rb.velocity = moveInput * currentSpeed;
    }



    // ======================
    // Animation
    // ======================

    private void UpdateAnimation()
    {
        animator.SetFloat("Speed", rb.velocity.magnitude);
    }



    // ======================
    // Direction
    // ======================

    private void UpdateDirection()
    {
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }


    private void UpdateLastMoveDirection()
    {
        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput.normalized;
        }
    }



    // ======================
    // Roll
    // ======================

    private IEnumerator RollCoroutine()
    {
        isRolling = true;

        animator.SetTrigger("Roll");

        rb.velocity = lastMoveDirection * rollSpeed;

        yield return new WaitForSeconds(rollDuration);

        rb.velocity = Vector2.zero;

        isRolling = false;
    }
}