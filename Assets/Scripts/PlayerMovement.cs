using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;      // Скорость движения
    public float jumpForce = 10f;      // Сила прыжка
    public float attackSpawnOffsetY = 1.1f; // Насколько ниже игрока спавнить куб
    public float attackCubeLifetime = 3f;   // Время жизни куба

    private Rigidbody2D rb;            // Ссылка на компонент физики
    private bool isGrounded;            // Проверка на земле ли мы
    private Vector2 moveInput;         // Ввод движения
    private static Sprite cachedSquareSprite; // Кеш спрайта, чтобы не создавать каждый раз
    private readonly HashSet<Collider2D> groundedColliders = new HashSet<Collider2D>();

    // Визуальные и звуковые компоненты
    private PlayerParticles playerParticles;
    private PlayerAnimator playerAnimator;
    private PlayerAudio playerAudio;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerParticles = GetComponent<PlayerParticles>();
        playerAnimator = GetComponent<PlayerAnimator>();
        playerAudio = GetComponent<PlayerAudio>();
    }

    void Update()
    {
        float moveX = moveInput.x;
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // Обновляем направление в аниматоре
        if (playerAnimator != null)
            playerAnimator.SetFacingDirection(moveX);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (isGrounded && value.isPressed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // Частицы и анимация прыжка
            if (playerParticles != null) playerParticles.OnJump();
            if (playerAnimator != null)  playerAnimator.OnJump();
            if (playerAudio != null)     playerAudio.PlayJump();
        }
    }

    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;

        Vector2 spawnPosition = (Vector2)transform.position + Vector2.down * attackSpawnOffsetY;
        GameObject attackSquare = new GameObject("AttackSquare");
        attackSquare.transform.position = spawnPosition;
        attackSquare.transform.localScale = Vector3.one;

        SpriteRenderer spriteRenderer = attackSquare.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSquareSprite();
        spriteRenderer.color = Color.black;
        attackSquare.AddComponent<BoxCollider2D>();

        Destroy(attackSquare, attackCubeLifetime);

        // Частицы и звук постановки блока
        if (playerParticles != null) playerParticles.OnBlockPlaced(spawnPosition);
        if (playerAudio != null)     playerAudio.PlayBlockPlace();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCubePlaced();
        }
    }

    private Sprite GetSquareSprite()
    {
        if (cachedSquareSprite != null) return cachedSquareSprite;

        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        cachedSquareSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return cachedSquareSprite;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (HasGroundContact(collision))
            groundedColliders.Add(collision.collider);

        UpdateGrounded();
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        groundedColliders.Remove(collision.collider);
        UpdateGrounded();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (HasGroundContact(collision))
            groundedColliders.Add(collision.collider);
        else
            groundedColliders.Remove(collision.collider);

        UpdateGrounded();
    }

    private void UpdateGrounded()
    {
        bool newGrounded = groundedColliders.Count > 0;
        isGrounded = newGrounded;

        // Синхронизируем состояние со вспомогательными компонентами
        if (playerParticles != null) playerParticles.SetGrounded(isGrounded);
        if (playerAnimator != null)  playerAnimator.SetGrounded(isGrounded);
        if (playerAudio != null)     playerAudio.SetGrounded(isGrounded);
    }

    private bool HasGroundContact(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) return true;
        }
        return false;
    }
}
