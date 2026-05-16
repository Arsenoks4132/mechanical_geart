using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Редакторский скрипт для создания AnimatorController'ов игрока и NPC.
/// Запустите через меню: Tools > Setup Animators
/// </summary>
public static class AnimatorSetup
{
    [MenuItem("Tools/Setup Animators")]
    public static void SetupAnimators()
    {
        SetupPlayerAnimator();
        SetupNPCAnimator();
        AssetDatabase.SaveAssets();
        Debug.Log("[AnimatorSetup] Animator controllers created and assigned!");
    }

    private static void SetupPlayerAnimator()
    {
        string path = "Assets/Animations/PlayerAnimator.controller";
        EnsureFolder("Assets/Animations");

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // --- Параметры ---
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
        controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);

        var root = controller.layers[0].stateMachine;

        // --- Состояния ---
        var idle  = root.AddState("Idle");
        var run   = root.AddState("Run");
        var jump  = root.AddState("Jump");
        var fall  = root.AddState("Fall");

        root.defaultState = idle;

        // --- Клипы (процедурные) ---
        idle.motion = CreatePlayerIdleClip();
        run.motion  = CreatePlayerRunClip();
        jump.motion = CreatePlayerJumpClip();
        fall.motion = CreatePlayerFallClip();

        // --- Переходы ---
        // Idle <-> Run
        var idleToRun = idle.AddTransition(run);
        idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToRun.hasExitTime = false;
        idleToRun.duration = 0.1f;

        var runToIdle = run.AddTransition(idle);
        runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        runToIdle.hasExitTime = false;
        runToIdle.duration = 0.1f;

        // Any -> Jump (по триггеру)
        var anyToJump = root.AddAnyStateTransition(jump);
        anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
        anyToJump.hasExitTime = false;
        anyToJump.duration = 0.05f;

        // Jump -> Fall
        var jumpToFall = jump.AddTransition(fall);
        jumpToFall.AddCondition(AnimatorConditionMode.Less, 0f, "VelocityY");
        jumpToFall.hasExitTime = false;
        jumpToFall.duration = 0.1f;

        // Fall -> Idle
        var fallToIdle = fall.AddTransition(idle);
        fallToIdle.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
        fallToIdle.hasExitTime = false;
        fallToIdle.duration = 0.05f;

        // Назначаем контроллер на игрока
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var anim = player.GetComponent<Animator>();
            if (anim == null) anim = player.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            Debug.Log("[AnimatorSetup] Player Animator assigned.");
        }
    }

    private static void SetupNPCAnimator()
    {
        string path = "Assets/Animations/NPCAnimator.controller";
        EnsureFolder("Assets/Animations");

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsHappy", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

        var root = controller.layers[0].stateMachine;

        var idle  = root.AddState("Idle");
        var walk  = root.AddState("Walk");
        var happy = root.AddState("Happy");

        root.defaultState = idle;

        idle.motion  = CreateNPCIdleClip();
        walk.motion  = CreateNPCWalkClip();
        happy.motion = CreateNPCHappyClip();

        // Idle <-> Walk
        var idleToWalk = idle.AddTransition(walk);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
        idleToWalk.hasExitTime = false;
        idleToWalk.duration = 0.1f;

        var walkToIdle = walk.AddTransition(idle);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
        walkToIdle.hasExitTime = false;
        walkToIdle.duration = 0.1f;

        // Any -> Happy
        var anyToHappy = root.AddAnyStateTransition(happy);
        anyToHappy.AddCondition(AnimatorConditionMode.If, 0, "IsHappy");
        anyToHappy.hasExitTime = false;
        anyToHappy.duration = 0.1f;

        // Happy -> Idle
        var happyToIdle = happy.AddTransition(idle);
        happyToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsHappy");
        happyToIdle.hasExitTime = false;
        happyToIdle.duration = 0.15f;

        // Назначаем на NPC
        var npc = GameObject.Find("Circle");
        if (npc != null)
        {
            var anim = npc.GetComponent<Animator>();
            if (anim == null) anim = npc.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            Debug.Log("[AnimatorSetup] NPC Animator assigned.");
        }
    }

    // ---- Создание ключевых анимационных клипов ----

    private static AnimationClip CreatePlayerIdleClip()
    {
        var clip = new AnimationClip { name = "PlayerIdle" };
        clip.frameRate = 12;
        // Лёгкое покачивание по Y (дыхание)
        var curve = AnimationCurve.EaseInOut(0f, 0f, 0.5f, 0.04f);
        curve.AddKey(new Keyframe(1f, 0f));
        clip.SetCurve("", typeof(Transform), "localPosition.y", curve);
        SaveClip(clip, "Assets/Animations/PlayerIdle.anim");
        return clip;
    }

    private static AnimationClip CreatePlayerRunClip()
    {
        var clip = new AnimationClip { name = "PlayerRun" };
        clip.frameRate = 12;
        // Подпрыгивание при беге
        var curveY = new AnimationCurve(
            new Keyframe(0f,   0f),
            new Keyframe(0.1f, 0.06f),
            new Keyframe(0.2f, 0f),
            new Keyframe(0.3f, 0.06f),
            new Keyframe(0.4f, 0f)
        );
        clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
        // Лёгкий наклон при беге (поворот по Z)
        var curveZ = new AnimationCurve(
            new Keyframe(0f,    5f),
            new Keyframe(0.1f, -5f),
            new Keyframe(0.2f,  5f),
            new Keyframe(0.3f, -5f),
            new Keyframe(0.4f,  5f)
        );
        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", curveZ);
        SetLooping(clip);
        SaveClip(clip, "Assets/Animations/PlayerRun.anim");
        return clip;
    }

    private static AnimationClip CreatePlayerJumpClip()
    {
        var clip = new AnimationClip { name = "PlayerJump" };
        clip.frameRate = 12;
        // Наклон вперёд при прыжке
        var curve = new AnimationCurve(
            new Keyframe(0f,    0f),
            new Keyframe(0.1f, 15f),
            new Keyframe(0.35f, 0f)
        );
        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", curve);
        SaveClip(clip, "Assets/Animations/PlayerJump.anim");
        return clip;
    }

    private static AnimationClip CreatePlayerFallClip()
    {
        var clip = new AnimationClip { name = "PlayerFall" };
        clip.frameRate = 12;
        var curve = new AnimationCurve(
            new Keyframe(0f,   0f),
            new Keyframe(0.2f, -10f)
        );
        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", curve);
        SaveClip(clip, "Assets/Animations/PlayerFall.anim");
        return clip;
    }

    private static AnimationClip CreateNPCIdleClip()
    {
        var clip = new AnimationClip { name = "NPCIdle" };
        clip.frameRate = 12;
        var curve = AnimationCurve.EaseInOut(0f, 0f, 0.6f, 0.03f);
        curve.AddKey(new Keyframe(1.2f, 0f));
        clip.SetCurve("", typeof(Transform), "localPosition.y", curve);
        SetLooping(clip);
        SaveClip(clip, "Assets/Animations/NPCIdle.anim");
        return clip;
    }

    private static AnimationClip CreateNPCWalkClip()
    {
        var clip = new AnimationClip { name = "NPCWalk" };
        clip.frameRate = 12;
        var curveY = new AnimationCurve(
            new Keyframe(0f,    0f),
            new Keyframe(0.15f, 0.05f),
            new Keyframe(0.3f,  0f),
            new Keyframe(0.45f, 0.05f),
            new Keyframe(0.6f,  0f)
        );
        clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
        SetLooping(clip);
        SaveClip(clip, "Assets/Animations/NPCWalk.anim");
        return clip;
    }

    private static AnimationClip CreateNPCHappyClip()
    {
        var clip = new AnimationClip { name = "NPCHappy" };
        clip.frameRate = 12;
        // Быстрое подпрыгивание
        var curveY = new AnimationCurve(
            new Keyframe(0f,    0f),
            new Keyframe(0.1f,  0.15f),
            new Keyframe(0.2f,  0f),
            new Keyframe(0.3f,  0.15f),
            new Keyframe(0.4f,  0f)
        );
        clip.SetCurve("", typeof(Transform), "localPosition.y", curveY);
        // Вращение
        var curveZ = new AnimationCurve(
            new Keyframe(0f,   0f),
            new Keyframe(0.1f, 20f),
            new Keyframe(0.2f, 0f),
            new Keyframe(0.3f, -20f),
            new Keyframe(0.4f, 0f)
        );
        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", curveZ);
        SetLooping(clip);
        SaveClip(clip, "Assets/Animations/NPCHappy.anim");
        return clip;
    }

    private static void SetLooping(AnimationClip clip)
    {
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
    }

    private static void SaveClip(AnimationClip clip, string path)
    {
        if (!AssetDatabase.Contains(clip))
            AssetDatabase.CreateAsset(clip, path);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            string folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
