using UnityEngine;

/// <summary>
/// Динамическое управление звуком игрока.
/// Генерирует процедурные звуки через AudioSource (без внешних файлов).
/// - Шаги: pitch/volume меняются от скорости
/// - Прыжок: высокочастотный свист
/// - Блок: глухой удар
/// </summary>
public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource footstepSource;
    public AudioSource jumpSource;
    public AudioSource blockPlaceSource;

    [Header("Footstep Settings")]
    [Tooltip("Интервал между шагами в секундах")]
    public float footstepInterval = 0.32f;
    [Tooltip("Питч меняется от скорости")]
    public float minPitch = 0.85f;
    public float maxPitch = 1.2f;
    [Tooltip("Громкость меняется динамически")]
    public float footstepVolume = 0.45f;

    [Header("Jump Settings")]
    public float jumpVolume = 0.6f;

    [Header("Block Place Settings")]
    public float blockVolume = 0.7f;

    private float stepTimer = 0f;
    private bool isGrounded = false;
    private float currentSpeed = 0f;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupAudioSources();
    }

    private void SetupAudioSources()
    {
        if (footstepSource == null)
        {
            footstepSource = gameObject.AddComponent<AudioSource>();
            footstepSource.playOnAwake = false;
            footstepSource.spatialBlend = 0f; // 2D звук
        }

        if (jumpSource == null)
        {
            jumpSource = gameObject.AddComponent<AudioSource>();
            jumpSource.playOnAwake = false;
            jumpSource.spatialBlend = 0f;
        }

        if (blockPlaceSource == null)
        {
            blockPlaceSource = gameObject.AddComponent<AudioSource>();
            blockPlaceSource.playOnAwake = false;
            blockPlaceSource.spatialBlend = 0f;
        }
    }

    void Update()
    {
        if (rb != null)
            currentSpeed = Mathf.Abs(rb.linearVelocity.x);

        // Шаги: только на земле и при движении
        if (isGrounded && currentSpeed > 0.3f)
        {
            // Динамически меняем интервал от скорости (быстрее = чаще шаги)
            float dynamicInterval = Mathf.Lerp(footstepInterval * 1.3f, footstepInterval * 0.7f, 
                                               currentSpeed / 8f);
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = dynamicInterval;
                PlayFootstep();
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        AudioClip clip = GenerateFootstepClip();
        // Pitch зависит от скорости
        float t = Mathf.Clamp01(currentSpeed / 6f);
        footstepSource.pitch = Mathf.Lerp(minPitch, maxPitch, t);
        // Громкость слегка рандомная для натуральности
        footstepSource.volume = footstepVolume * Random.Range(0.85f, 1.0f);
        footstepSource.PlayOneShot(clip);
    }

    public void PlayJump()
    {
        AudioClip clip = GenerateJumpClip();
        jumpSource.pitch = Random.Range(0.95f, 1.05f);
        jumpSource.volume = jumpVolume;
        jumpSource.PlayOneShot(clip);
    }

    public void PlayBlockPlace()
    {
        AudioClip clip = GenerateBlockPlaceClip();
        blockPlaceSource.pitch = Random.Range(0.9f, 1.1f);
        blockPlaceSource.volume = blockVolume;
        blockPlaceSource.PlayOneShot(clip);
    }

    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;
    }

    // ---- Процедурная генерация коротких звуков ----

    /// <summary>Глухой удар ноги о землю — короткий шум с быстрым затуханием</summary>
    private AudioClip GenerateFootstepClip()
    {
        int sampleRate = 44100;
        float duration = 0.06f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 30f); // быстрое затухание
            data[i] = envelope * Random.Range(-1f, 1f) * 0.6f;
        }

        AudioClip clip = AudioClip.Create("Footstep", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Свистящий звук прыжка — синус с нарастающей частотой</summary>
    private AudioClip GenerateJumpClip()
    {
        int sampleRate = 44100;
        float duration = 0.18f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(300f, 900f, t); // частота растёт
            float envelope = Mathf.Exp(-t * 6f);
            data[i] = envelope * Mathf.Sin(2f * Mathf.PI * freq * t) * 0.5f;
        }

        AudioClip clip = AudioClip.Create("Jump", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>Глухой удар при постановке блока — низкий тон с шумом</summary>
    private AudioClip GenerateBlockPlaceClip()
    {
        int sampleRate = 44100;
        float duration = 0.12f;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float envelope = Mathf.Exp(-t * 20f);
            float tone = Mathf.Sin(2f * Mathf.PI * 120f * t); // низкий тон
            float noise = Random.Range(-1f, 1f) * 0.4f;
            data[i] = envelope * (tone * 0.6f + noise) * 0.8f;
        }

        AudioClip clip = AudioClip.Create("BlockPlace", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// Динамически меняет громкость шагов в зависимости от скорости.
    /// Может вызываться извне (например, из UI или GameManager).
    /// </summary>
    public void SetFootstepVolume(float volume)
    {
        footstepVolume = Mathf.Clamp01(volume);
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }
}
