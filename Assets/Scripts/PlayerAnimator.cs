using UnityEngine;

/// <summary>
/// Процедурная + ключевая анимация игрока через Animator.
/// Squash & Stretch применяется к визуальному child-объекту,
/// чтобы не конфликтовать с Rigidbody2D на корневом transform.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Squash & Stretch (процедурная анимация)")]
    public float squashAmount = 0.7f;
    public float stretchAmount = 1.3f;
    public float recoverySpeed = 8f;

    [Header("Visual Child")]
    [Tooltip("Дочерний объект с SpriteRenderer. Если null — создаётся автоматически.")]
    public Transform visualChild;

    private Animator anim;
    private Rigidbody2D rb;
    private Vector3 baseScale;        // исходный scale визуального child
    private Vector3 currentTarget;
    private float facingSign = 1f;    // направление взгляда

    private bool isGrounded = true;
    private bool wasGrounded = true;

    private static readonly int SpeedHash      = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int VelocityYHash  = Animator.StringToHash("VelocityY");
    private static readonly int IsJumpingHash  = Animator.StringToHash("IsJumping");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        EnsureVisualChild();
        // Animator берём с child Visual, не с корня
        anim = visualChild != null ? visualChild.GetComponent<Animator>() : GetComponent<Animator>();
        baseScale     = visualChild != null ? visualChild.localScale : transform.localScale;
        currentTarget = baseScale;
    }

    private void EnsureVisualChild()
    {
        if (visualChild != null) return;

        // Ищем child "Visual" (создан заранее через Editor-скрипт)
        var found = transform.Find("Visual");
        if (found != null)
        {
            visualChild = found;
            return;
        }

        // Fallback: любой child с SpriteRenderer
        var sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && sr.gameObject != gameObject)
        {
            visualChild = sr.transform;
        }
    }

    void Update()
    {
        float speedX   = Mathf.Abs(rb.linearVelocity.x);
        float velocityY = rb.linearVelocity.y;

        // Обновляем параметры Animator
        anim.SetFloat(SpeedHash,      speedX);
        anim.SetBool(IsGroundedHash,  isGrounded);
        anim.SetFloat(VelocityYHash,  velocityY);
        anim.SetBool(IsJumpingHash,   !isGrounded && velocityY > 0.1f);

        // ---- Squash & Stretch (только к visual child) ----
        if (!wasGrounded && isGrounded)
        {
            // Приземление: сплющивание
            currentTarget = new Vector3(
                baseScale.x * (2f - squashAmount),
                baseScale.y * squashAmount,
                baseScale.z);
        }
        else if (!isGrounded && velocityY > 1f)
        {
            // Взлёт: вытягивание
            currentTarget = new Vector3(
                baseScale.x * (2f - stretchAmount),
                baseScale.y * stretchAmount,
                baseScale.z);
        }
        else
        {
            currentTarget = baseScale;
        }

        // Применяем к visual child с учётом направления
        if (visualChild != null)
        {
            Vector3 smoothed = Vector3.Lerp(
                visualChild.localScale,
                new Vector3(currentTarget.x * facingSign, currentTarget.y, currentTarget.z),
                Time.deltaTime * recoverySpeed);
            visualChild.localScale = smoothed;
        }

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

    /// <summary>Зеркалит спрайт через localScale.x child'а, не трогает корневой transform</summary>
    public void SetFacingDirection(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.01f) return;
        facingSign = dirX > 0 ? 1f : -1f;
    }
}
