using UnityEngine;

/// <summary>
/// Управляет системой частиц NPC:
/// - звёздочки/сердечки "радости" когда игрок рядом
/// </summary>
public class NPCParticles : MonoBehaviour
{
    [Header("Happy Particles")]
    public ParticleSystem happyParticles;

    [Tooltip("Дистанция до игрока, при которой NPC радуется")]
    public float happyDistance = 2.5f;
    public Transform playerTransform;

    private bool isHappy = false;
    private float happyEmitTimer = 0f;
    private const float HappyEmitInterval = 0.3f;

    void Awake()
    {
        if (happyParticles == null)
            happyParticles = CreateHappySystem();

        if (playerTransform == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else
            {
                // fallback: ищем по скрипту
                var pm = FindFirstObjectByType<PlayerMovement>();
                if (pm != null) playerTransform = pm.transform;
            }
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        bool shouldBeHappy = dist <= happyDistance;

        if (shouldBeHappy != isHappy)
        {
            isHappy = shouldBeHappy;
            if (!isHappy)
            {
                // прекращаем испускать частицы
                happyEmitTimer = 0f;
            }
        }

        if (isHappy)
        {
            happyEmitTimer -= Time.deltaTime;
            if (happyEmitTimer <= 0f)
            {
                happyEmitTimer = HappyEmitInterval;
                EmitHappy(3);
            }
        }
    }

    private void EmitHappy(int count)
    {
        if (happyParticles == null) return;
        happyParticles.transform.position = transform.position + Vector3.up * 0.6f;
        happyParticles.Emit(count);
    }

    private ParticleSystem CreateHappySystem()
    {
        GameObject go = new GameObject("HappyParticles");
        go.transform.SetParent(transform, false);
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        // Случайный цвет: жёлтый или розовый
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.9f, 0.1f, 1f),
            new Color(1f, 0.4f, 0.7f, 1f)
        );
        main.gravityModifier = -0.3f; // летят вверх
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.25f;

        var colorOverLife = ps.colorOverLifetime;
        colorOverLife.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(Color.yellow, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLife.color = new ParticleSystem.MinMaxGradient(grad);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(Color.yellow);
        return ps;
    }

    private Material CreateParticleMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        return mat;
    }
}
