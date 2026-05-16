using UnityEngine;

/// <summary>
/// Анимация игрока через Animator + процедурный Squash & Stretch.
/// SpriteRenderer находится прямо на объекте Player.
/// Переворот спрайта делается через SpriteRenderer.flipX.
/// </summary>
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Squash & Stretch (процедурная анимация)")]
    public float squashAmount  = 0.7f;
    public float stretchAmount = 1.3f;
    public float recoverySpeed = 8f;

    private Animator       anim;
    private Rigidbody2D    rb;
    private SpriteRenderer sr;

    private Vector3 baseScale;
    private Vector3 currentTarget;

    private bool isGrounded  = true;
    private bool wasGrounded = true;

    private static readonly int SpeedHash      = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VelocityYHash  = Animator.StringToHash("VelocityY");
    private static readonly int IsJumpingHash  = Animator.StringToHash("IsJumping");

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr   = GetComponent<SpriteRenderer>();

        baseScale     = transform.localScale;
        currentTarget = baseScale;
    }

    void Update()
    {
        float speedX    = Mathf.Abs(rb.linearVelocity.x);
        float velocityY = rb.linearVelocity.y;

        // Параметры Animator
        anim.SetFloat(SpeedHash,     speedX);
        anim.SetBool(IsGroundedHash, isGrounded);
        anim.SetFloat(VelocityYHash, velocityY);
        anim.SetBool(IsJumpingHash,  !isGrounded && velocityY > 0.1f);

        // ---- Squash & Stretch ----
        if (!wasGrounded && isGrounded)
        {
            // Приземление — сплющивание
            currentTarget = new Vector3(
                baseScale.x * (2f - squashAmount),
                baseScale.y * squashAmount,
                baseScale.z);
        }
        else if (!isGrounded && velocityY > 1f)
        {
            // Взлёт — вытягивание
            currentTarget = new Vector3(
                baseScale.x * (2f - stretchAmount),
                baseScale.y * stretchAmount,
                baseScale.z);
        }
        else
        {
            currentTarget = baseScale;
        }

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            currentTarget,
            Time.deltaTime * recoverySpeed);

        wasGrounded = isGrounded;
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    public void OnJump()
    {
        currentTarget = new Vector3(
            baseScale.x * (2f - stretchAmount),
            baseScale.y * stretchAmount,
            baseScale.z);

        if (anim != null) anim.SetTrigger("Jump");
    }

    /// <summary>
    /// Переворачивает спрайт в нужную сторону через flipX.
    /// Не трогает localScale — физика не ломается.
    /// </summary>
    public void SetFacingDirection(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        sr.flipX = dirX < 0;
    }
}
