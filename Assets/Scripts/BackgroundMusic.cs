using UnityEngine;

/// <summary>
/// Процедурно генерирует фоновую музыку и динамически настраивает её громкость.
/// Громкость нарастает при старте (fade-in) и снижается при приближении NPC к игроку.
/// </summary>
public class BackgroundMusic : MonoBehaviour
{
    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float normalVolume = 0.3f;
    [Range(0f, 1f)]
    public float nearNPCVolume = 0.15f;
    [Tooltip("Скорость изменения громкости")]
    public float volumeFadeSpeed = 2f;
    [Tooltip("Расстояние до NPC, при котором музыка приглушается")]
    public float npcMuteDistance = 3f;

    [Header("References")]
    public Transform playerTransform;
    public Transform npcTransform;

    private AudioSource audioSource;
    private float targetVolume;
    private bool fadingIn = true;
    private float fadeInTimer = 0f;
    private const float FadeInDuration = 2f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0f;
        targetVolume = normalVolume;

        // Найти игрока и NPC если не назначены
        if (playerTransform == null)
        {
            var pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) playerTransform = pm.transform;
        }
        if (npcTransform == null)
        {
            var npc = FindFirstObjectByType<EnemyWalk>();
            if (npc != null) npcTransform = npc.transform;
        }

        // Генерируем и запускаем музыку
        AudioClip bgClip = GenerateBackgroundMusic();
        audioSource.clip = bgClip;
        audioSource.Play();
    }

    void Update()
    {
        // Fade in при старте
        if (fadingIn)
        {
            fadeInTimer += Time.deltaTime;
            if (fadeInTimer >= FadeInDuration)
            {
                fadingIn = false;
                audioSource.volume = targetVolume;
            }
            else
            {
                audioSource.volume = Mathf.Lerp(0f, targetVolume, fadeInTimer / FadeInDuration);
            }
            return;
        }

        // Динамическая настройка громкости: приглушаем при радости NPC
        if (playerTransform != null && npcTransform != null)
        {
            float dist = Vector2.Distance(playerTransform.position, npcTransform.position);
            float t = Mathf.InverseLerp(npcMuteDistance, npcMuteDistance * 2f, dist);
            targetVolume = Mathf.Lerp(nearNPCVolume, normalVolume, t);
        }

        // Плавное изменение громкости
        audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, 
                                                Time.deltaTime * volumeFadeSpeed);
    }

    /// <summary>Позволяет игроку вручную настроить громкость фоновой музыки</summary>
    public void SetVolume(float volume)
    {
        normalVolume = Mathf.Clamp01(volume);
        targetVolume = normalVolume;
    }

    /// <summary>
    /// Процедурная генерация простой зацикленной мелодии.
    /// Синтезирует 4-нотную последовательность в виде синусоидальных тонов.
    /// </summary>
    private AudioClip GenerateBackgroundMusic()
    {
        int sampleRate = 44100;
        // 4-нотная фраза, каждая нота 0.5 сек, цикл = 2 сек
        float noteDuration = 0.5f;
        float[] notes = new float[] { 261.63f, 329.63f, 392.00f, 329.63f }; // C4 E4 G4 E4
        int totalSamples = Mathf.RoundToInt(sampleRate * noteDuration * notes.Length);
        float[] data = new float[totalSamples];

        int noteLen = Mathf.RoundToInt(sampleRate * noteDuration);
        for (int noteIdx = 0; noteIdx < notes.Length; noteIdx++)
        {
            float freq = notes[noteIdx];
            int offset = noteIdx * noteLen;
            for (int i = 0; i < noteLen; i++)
            {
                float t = (float)i / noteLen;
                // ADSR-огибающая: Attack(0.05) + Sustain + Release(0.15)
                float env;
                if (t < 0.05f)
                    env = t / 0.05f;
                else if (t > 0.85f)
                    env = (1f - t) / 0.15f;
                else
                    env = 1f;

                float sample = env * Mathf.Sin(2f * Mathf.PI * freq * t);
                // Добавляем обертон для более тёплого звука
                sample += env * 0.3f * Mathf.Sin(4f * Mathf.PI * freq * t);
                data[offset + i] = sample * 0.2f; // тихо, фоновая
            }
        }

        AudioClip clip = AudioClip.Create("BackgroundMusic", totalSamples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
