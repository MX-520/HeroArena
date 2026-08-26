using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;

    private Vector2 moveInput;

    public float speed = 5f;
    public float runSpeed = 8f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private bool isRunning;

    private bool isRolling;

    public float rollSpeed = 8f;

    public void OnRoll(InputValue value)
    {
        if (value.isPressed)
        {
            animator.SetTrigger("Roll");
        }
    }


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }


    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }


    public void OnRun(InputValue value)
    {
        isRunning = value.Get<float>() > 0;

        Debug.Log("Run: " + isRunning);
    }


    void FixedUpdate()
    {
        // 判断当前速度
        float currentSpeed = isRunning ? runSpeed : speed;

        // 移动
        rb.velocity = moveInput * currentSpeed;


        // 给Animator传速度
        animator.SetFloat("Speed", rb.velocity.magnitude);


        // 翻转角色
        if (moveInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }
}