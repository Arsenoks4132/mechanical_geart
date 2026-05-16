using UnityEngine;

/// <summary>
/// Управляет системами частиц игрока:
/// - пыль при ходьбе по земле
/// - взрыв частиц при прыжке
/// - вспышка частиц при размещении блока
/// </summary>
public class PlayerParticles : MonoBehaviour
{
    [Header("Dust (Walk/Land)")]
    public ParticleSystem dustParticles;

    [Header("Jump Burst")]
    public ParticleSystem jumpParticles;

    [Header("Block Place")]
    public ParticleSystem blockPlaceParticles;

    private bool wasGrounded = true;
    private bool isGrounded = true;
    private float walkEmissionTimer = 0f;
    private const float WalkEmissionInterval = 0.18f;
    private Vector2 lastVelocity;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        EnsureParticleSystems();
    }

    private void EnsureParticleSystems()
    {
        if (dustParticles == null)
            dustParticles = CreateDustSystem();

        if (jumpParticles == null)
            jumpParticles = CreateJumpSystem();

        if (blockPlaceParticles == null)
            blockPlaceParticles = CreateBlockPlaceSystem();
    }

    void Update()
    {
        float speedX = Mathf.Abs(rb != null ? rb.linearVelocity.x : 0f);

        // Пыль при ходьбе
        if (isGrounded && speedX > 0.3f)
        {
            walkEmissionTimer -= Time.deltaTime;
            if (walkEmissionTimer <= 0f)
            {
                walkEmissionTimer = WalkEmissionInterval;
                EmitDust(2);
            }
        }

        // Определяем приземление
        if (!wasGrounded && isGrounded)
        {
            EmitDust(6);
        }

        wasGrounded = isGrounded;
    }

    private void EmitDust(int count)
    {
        if (dustParticles == null) return;
        dustParticles.transform.position = transform.position + Vector3.down * 0.45f;
        dustParticles.Emit(count);
    }

    public void OnJump()
    {
        if (jumpParticles == null) return;
        jumpParticles.transform.position = transform.position + Vector3.down * 0.45f;
        jumpParticles.Emit(8);
    }

    public void OnBlockPlaced(Vector3 position)
    {
        if (blockPlaceParticles == null) return;
        blockPlaceParticles.transform.position = position;
        blockPlaceParticles.Emit(10);
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    // ---- Процедурное создание систем частиц ----

    private ParticleSystem CreateDustSystem()
    {
        GameObject go = new GameObject("DustParticles");
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.4f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(0.7f, 0.6f, 0.4f, 0.8f);
        main.gravityModifier = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.enabled = false; // управляем вручную через Emit()

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.4f, 0.1f, 0.1f);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(new Color(0.7f, 0.6f, 0.4f, 1f));
        return ps;
    }

    private ParticleSystem CreateJumpSystem()
    {
        GameObject go = new GameObject("JumpParticles");
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor = new Color(0.6f, 0.8f, 1f, 0.9f);
        main.gravityModifier = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(new Color(0.4f, 0.7f, 1f, 1f));
        return ps;
    }

    private ParticleSystem CreateBlockPlaceSystem()
    {
        GameObject go = new GameObject("BlockPlaceParticles");
        go.transform.SetParent(null); // мировое пространство
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
        main.startColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        main.gravityModifier = 0.8f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f, 0.5f, 0.1f);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(new Color(0.4f, 0.4f, 0.4f, 1f));
        return ps;
    }

    private Material CreateParticleMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
}
