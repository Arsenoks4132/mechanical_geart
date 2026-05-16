using UnityEngine;

public class EnemyWalk : MonoBehaviour
{
    public Transform target;
    public float speed = 2f;
    public float detectionRange = 10f;
    public float edgeCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public float stopDistance = 2f;
    public float bounceForce = 5f;
    public float bounceDelay = 0.3f;

    private Rigidbody2D rb;
    private bool facingRight = true;
    private float timeSinceLastBounce = 0f;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D не найден на " + gameObject.name);
        }

        if (target == null)
        {
            Debug.LogWarning("Target не установлен для " + gameObject.name);
        }
    }

    void Update()
    {
        if (target == null || rb == null)
        {
            return;
        }

        timeSinceLastBounce += Time.deltaTime;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > detectionRange)
        {
            return;
        }

        if (distanceToTarget < stopDistance)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (isGrounded && timeSinceLastBounce >= bounceDelay)
            {
                rb.linearVelocity = new Vector2(0, bounceForce);
                timeSinceLastBounce = 0f;
            }

            return;
        }

        float directionToTarget = target.position.x - transform.position.x;
        bool isEdgeAhead = CheckEdgeAhead(directionToTarget > 0);

        if (isEdgeAhead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            float moveDirection = Mathf.Sign(directionToTarget);
            rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);

            if ((moveDirection > 0 && !facingRight) || (moveDirection < 0 && facingRight))
            {
                Flip();
            }
        }
    }

    private bool CheckEdgeAhead(bool movingRight)
    {
        Vector2 checkPosition = (Vector2)transform.position + Vector2.right * (movingRight ? edgeCheckDistance : -edgeCheckDistance);
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, 1f, groundLayer);

        return !hit.collider;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateGroundedState();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        UpdateGroundedState();
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        UpdateGroundedState();
    }

    private void UpdateGroundedState()
    {
        Vector2 rayOrigin = (Vector2)transform.position + Vector2.down * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, 0.1f, groundLayer);
        isGrounded = hit.collider != null;
    }
}
