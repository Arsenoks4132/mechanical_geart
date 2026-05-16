using UnityEngine;
using UnityEditor;

/// <summary>
/// Редакторский скрипт — добавляет все визуальные компоненты на объекты сцены.
/// Запустите через меню: Tools > Setup Visual Components
/// </summary>
public static class SceneVisualSetup
{
    [MenuItem("Tools/Setup Visual Components")]
    public static void SetupAll()
    {
        SetupPlayer();
        SetupNPC();
        SetupBackgroundMusic();

        // Сначала создаём аниматоры
        AnimatorSetup.SetupAnimators();

        EditorUtility.SetDirty(GameObject.Find("Player"));
        EditorUtility.SetDirty(GameObject.Find("Circle"));
        Debug.Log("[SceneVisualSetup] All visual components added!");
    }

    private static void SetupPlayer()
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogError("[SceneVisualSetup] Player not found!");
            return;
        }

        // Система частиц
        if (player.GetComponent<PlayerParticles>() == null)
            player.AddComponent<PlayerParticles>();

        // Анимация
        if (player.GetComponent<PlayerAnimator>() == null)
            player.AddComponent<PlayerAnimator>();

        // Звук
        if (player.GetComponent<PlayerAudio>() == null)
            player.AddComponent<PlayerAudio>();

        Debug.Log("[SceneVisualSetup] Player components added.");
    }

    private static void SetupNPC()
    {
        var npc = GameObject.Find("Circle");
        if (npc == null)
        {
            Debug.LogError("[SceneVisualSetup] NPC (Circle) not found!");
            return;
        }

        // Частицы NPC
        if (npc.GetComponent<NPCParticles>() == null)
            npc.AddComponent<NPCParticles>();

        // Анимация NPC
        if (npc.GetComponent<NPCAnimator>() == null)
            npc.AddComponent<NPCAnimator>();

        Debug.Log("[SceneVisualSetup] NPC components added.");
    }

    private static void SetupBackgroundMusic()
    {
        // Ищем GameManager для размещения фоновой музыки
        var gm = GameObject.Find("GameManager");
        if (gm == null)
        {
            gm = new GameObject("GameManager");
            Debug.LogWarning("[SceneVisualSetup] GameManager not found, created new.");
        }

        if (gm.GetComponent<BackgroundMusic>() == null)
            gm.AddComponent<BackgroundMusic>();

        Debug.Log("[SceneVisualSetup] BackgroundMusic added to GameManager.");
    }
}
