using UnityEngine;

/// <summary>
/// Анимация NPC: ходьба и "радость".
/// Squash & Stretch применяется к visual child,
/// не трогая корневой transform с Rigidbody2D.
/// </summary>
[RequireComponent(typeof(Animator))]
public class NPCAnimator : MonoBehaviour
{
    [Header("Happy Squash & Stretch")]
    public float happySquash   = 0.75f;
    public float happyStretch  = 1.35f;
    public float happyAnimSpeed = 10f;

    [Header("Detection")]
    public float happyDistance = 2.5f;
    public Transform playerTransform;

    private Animator anim;
    private Rigidbody2D rb;
    private Transform visualChild;
    private Vector3 baseScale;

    private bool isHappy = false;
    private float happyPhase = 0f;
    private float facingSign = 1f;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsHappyHash   = Animator.StringToHash("IsHappy");
    private static readonly int SpeedHash     = Animator.StringToHash("Speed");

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        EnsureVisualChild();
        // Animator берём с child Visual
        anim = visualChild != null ? visualChild.GetComponent<Animator>() : GetComponent<Animator>();
        baseScale = visualChild != null ? visualChild.localScale : Vector3.one;

        if (playerTransform == null)
        {
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) playerTransform = pm.transform;
        }
    }

    private void EnsureVisualChild()
    {
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
        float speedX = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;

        if (playerTransform != null)
        {
            float dist = Vector2.Distance(transform.position, playerTransform.position);
            isHappy = dist <= happyDistance;
        }

        anim.SetFloat(SpeedHash,     speedX);
        anim.SetBool(IsWalkingHash,  speedX > 0.1f && !isHappy);
        anim.SetBool(IsHappyHash,    isHappy);

        // Направление взгляда по скорости
        if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            facingSign = rb.linearVelocity.x > 0 ? 1f : -1f;

        // ---- Процедурный Bounce только на visual child ----
        if (visualChild == null) return;

        Vector3 targetScale;
        if (isHappy)
        {
            happyPhase += Time.deltaTime * happyAnimSpeed;
            float t = (Mathf.Sin(happyPhase) + 1f) * 0.5f;
            float sy = Mathf.Lerp(happySquash,         happyStretch,       t);
            float sx = Mathf.Lerp(2f - happySquash,    2f - happyStretch,  t);
            targetScale = new Vector3(baseScale.x * sx * facingSign, baseScale.y * sy, baseScale.z);
            visualChild.localScale = targetScale;
        }
        else
        {
            happyPhase = 0f;
            targetScale = new Vector3(baseScale.x * facingSign, baseScale.y, baseScale.z);
            visualChild.localScale = Vector3.Lerp(visualChild.localScale, targetScale, Time.deltaTime * 8f);
        }
    }
}
