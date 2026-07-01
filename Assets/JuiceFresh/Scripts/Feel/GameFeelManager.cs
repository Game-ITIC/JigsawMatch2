using System.Collections;
using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

/// <summary>
/// Central hub for Feel (More Mountains) juice: board shake, camera shake, freeze frames, haptics.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(-500)]
public sealed class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance { get; private set; }

    [SerializeField] private Camera targetCamera;
    [SerializeField] private Transform boardRoot;
    [SerializeField] private bool persistAcrossScenes;

    [SerializeField] private bool enableHaptics = true;
    [SerializeField] private bool enableCameraShake = true;
    [SerializeField] private bool enableBoardShake = true;
    [SerializeField] private bool enableFreezeFrames = true;
    [SerializeField] private bool enableItemPunch = true;

    [Header("Intensity")]
    [SerializeField, Range(0.5f, 3f)] private float globalIntensity = 1.6f;
    [SerializeField, Range(0f, 2f)] private float matchShakeStrength = 0.45f;
    [SerializeField, Range(0f, 2f)] private float boardShakeStrength = 0.18f;
    [SerializeField, Range(0f, 2f)] private float comboShakeStrength = 0.75f;
    [SerializeField, Range(0f, 2f)] private float winShakeStrength = 1f;

    private MMF_Player _matchPlayer;
    private MMF_Player _comboPlayer;
    private MMF_Player _winPlayer;
    private Camera _gameCamera;
    private Coroutine _punchCoroutine;
    private Coroutine _boardShakeCoroutine;

    public static void EnsureInitialized(Camera gameCamera, Transform board = null)
    {
        if(Instance != null)
        {
            Instance.BindCamera(gameCamera);
            Instance.BindBoard(board);
            return;
        }

        GameObject host = new GameObject("GameFeelManager");
        GameFeelManager manager = host.AddComponent<GameFeelManager>();
        manager.persistAcrossScenes = true;
        manager.BindCamera(gameCamera);
        manager.BindBoard(board);
    }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if(persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }

        if(targetCamera != null)
        {
            BindCamera(targetCamera);
        }

        if(boardRoot != null)
        {
            BindBoard(boardRoot);
        }

        EnsureFeelInfrastructure();
        BuildFeedbackPlayers();
    }

    private void OnDestroy()
    {
        if(Instance == this)
        {
            Instance = null;
        }
    }

    public void BindCamera(Camera gameCamera)
    {
        if(gameCamera == null)
        {
            return;
        }

        _gameCamera = gameCamera;
        targetCamera = gameCamera;
        EnsureCameraShaker(gameCamera);
    }

    public void BindBoard(Transform board)
    {
        if(board == null)
        {
            return;
        }

        boardRoot = board;
    }

    public void OnItemSelected(Transform item, int chainIndex)
    {
        if(enableItemPunch && item != null)
        {
            float punch = 0.16f + chainIndex * 0.025f;
            PlayItemPunch(item, punch);
        }

        PlayHaptic(HapticPatterns.PresetType.Selection);
    }

    public void OnItemDestroyed(Vector3 position)
    {
        // Per-item camera shake was too weak and got lost in chain pops.
    }

    public void OnMatchReleased(int matchedCount, Vector3 centerPosition)
    {
        float normalized = Mathf.Clamp01((matchedCount - 3f) / 7f);
        float shakeMul = globalIntensity * (0.85f + normalized * 0.9f);

        TriggerCameraShake(0.18f + normalized * 0.1f, matchShakeStrength * shakeMul, 28f);
        TriggerBoardShake(0.22f + normalized * 0.12f, boardShakeStrength * shakeMul);
        _matchPlayer?.PlayFeedbacks(centerPosition, 0.8f + normalized * 0.4f);

        if(enableFreezeFrames && matchedCount >= 5)
        {
            MMFreezeFrameEvent.Trigger(0.035f + normalized * 0.025f);
        }

        PlayHaptic(matchedCount >= 6 ? HapticPatterns.PresetType.MediumImpact : HapticPatterns.PresetType.LightImpact);
    }

    public void OnCombo(int combo)
    {
        if(combo < 3)
        {
            return;
        }

        float normalized = Mathf.Clamp01((combo - 3f) / 9f);
        float shakeMul = globalIntensity * (0.9f + normalized);

        TriggerCameraShake(0.2f + normalized * 0.15f, comboShakeStrength * shakeMul, 24f);
        TriggerBoardShake(0.28f + normalized * 0.15f, boardShakeStrength * (1.2f + normalized));
        _comboPlayer?.PlayFeedbacks(Vector3.zero, 0.9f + normalized * 0.5f);

        if(enableFreezeFrames && combo >= 4)
        {
            MMFreezeFrameEvent.Trigger(0.04f + normalized * 0.035f);
        }

        if(combo >= 4)
        {
            PlayHaptic(HapticPatterns.PresetType.MediumImpact);
        }
    }

    public void OnBoostExplosion(Vector3 position)
    {
        TriggerCameraShake(0.22f, comboShakeStrength * globalIntensity, 22f);
        TriggerBoardShake(0.3f, boardShakeStrength * globalIntensity * 1.4f);
        _comboPlayer?.PlayFeedbacks(position, 1.2f);

        if(enableFreezeFrames)
        {
            MMFreezeFrameEvent.Trigger(0.05f);
        }

        PlayHaptic(HapticPatterns.PresetType.HeavyImpact);
    }

    public void OnInvalidMove()
    {
        TriggerBoardShake(0.12f, boardShakeStrength * globalIntensity * 0.5f);
        TriggerCameraShake(0.1f, matchShakeStrength * 0.4f * globalIntensity, 35f);
        PlayHaptic(HapticPatterns.PresetType.Warning);
    }

    public void OnPreWin()
    {
        TriggerCameraShake(0.2f, winShakeStrength * 0.7f * globalIntensity, 20f);
        TriggerBoardShake(0.25f, boardShakeStrength * globalIntensity);
        PlayHaptic(HapticPatterns.PresetType.SoftImpact);
    }

    public void OnWin()
    {
        TriggerCameraShake(0.28f, winShakeStrength * globalIntensity, 18f);
        TriggerBoardShake(0.35f, boardShakeStrength * globalIntensity * 1.5f);
        _winPlayer?.PlayFeedbacks(Vector3.zero, 1.3f);
        PlayHaptic(HapticPatterns.PresetType.Success);
    }

    public void OnLose()
    {
        TriggerCameraShake(0.2f, winShakeStrength * 0.55f * globalIntensity, 22f);
        TriggerBoardShake(0.2f, boardShakeStrength * globalIntensity * 0.7f);
        PlayHaptic(HapticPatterns.PresetType.Failure);
    }

    public void OnTargetCollected()
    {
        PlayHaptic(HapticPatterns.PresetType.LightImpact);
    }

    private void EnsureFeelInfrastructure()
    {
        if(MMTimeManager.Instance == null)
        {
            GameObject timeManager = new GameObject("MMTimeManager");
            timeManager.AddComponent<MMTimeManager>();
            DontDestroyOnLoad(timeManager);
        }

        Camera camera = _gameCamera != null ? _gameCamera : Camera.main;
        if(camera != null)
        {
            EnsureCameraShaker(camera);
        }
    }

    private static void EnsureCameraShaker(Camera camera)
    {
        MMCameraShaker shaker = camera.GetComponent<MMCameraShaker>();
        if(shaker == null)
        {
            if(camera.GetComponent<MMWiggle>() == null)
            {
                camera.gameObject.AddComponent<MMWiggle>();
            }

            shaker = camera.gameObject.AddComponent<MMCameraShaker>();
        }

        shaker.CooldownBetweenShakes = 0f;
    }

    private void BuildFeedbackPlayers()
    {
        _matchPlayer = CreatePlayer("Feel_Match");
        ConfigureCameraShake(_matchPlayer, 0.18f, matchShakeStrength, 28f);

        _comboPlayer = CreatePlayer("Feel_Combo");
        ConfigureCameraShake(_comboPlayer, 0.24f, comboShakeStrength, 22f);
        if(enableFreezeFrames)
        {
            MMF_FreezeFrame freeze = (MMF_FreezeFrame)_comboPlayer.AddFeedback(typeof(MMF_FreezeFrame));
            freeze.FreezeFrameDuration = 0.045f;
        }

        _winPlayer = CreatePlayer("Feel_Win");
        ConfigureCameraShake(_winPlayer, 0.3f, winShakeStrength, 18f);
        MMF_FreezeFrame winFreeze = (MMF_FreezeFrame)_winPlayer.AddFeedback(typeof(MMF_FreezeFrame));
        winFreeze.FreezeFrameDuration = 0.06f;
    }

    private MMF_Player CreatePlayer(string playerName)
    {
        GameObject playerObject = new GameObject(playerName);
        playerObject.transform.SetParent(transform);
        MMF_Player player = playerObject.AddComponent<MMF_Player>();
        player.Initialization();
        return player;
    }

    private static void ConfigureCameraShake(MMF_Player player, float duration, float amplitude, float frequency)
    {
        MMF_CameraShake shake = (MMF_CameraShake)player.AddFeedback(typeof(MMF_CameraShake));
        shake.CameraShakeProperties = new MMCameraShakeProperties(duration, amplitude, frequency);
    }

    private void TriggerCameraShake(float duration, float amplitude, float frequency)
    {
        if(!enableCameraShake || amplitude <= 0f)
        {
            return;
        }

        MMCameraShakeEvent.Trigger(duration, amplitude, frequency, 0f, amplitude * 1.2f, 0f);
    }

    private void TriggerBoardShake(float duration, float magnitude)
    {
        if(!enableBoardShake || boardRoot == null || magnitude <= 0f)
        {
            return;
        }

        if(_boardShakeCoroutine != null)
        {
            StopCoroutine(_boardShakeCoroutine);
        }

        _boardShakeCoroutine = StartCoroutine(BoardShakeCoroutine(duration, magnitude));
    }

    private IEnumerator BoardShakeCoroutine(float duration, float magnitude)
    {
        Vector3 restPosition = boardRoot.localPosition;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            if(boardRoot == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float falloff = 1f - elapsed / duration;
            Vector2 offset = Random.insideUnitCircle * magnitude * falloff;
            boardRoot.localPosition = restPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        if(boardRoot != null)
        {
            boardRoot.localPosition = restPosition;
        }

        _boardShakeCoroutine = null;
    }

    private void PlayHaptic(HapticPatterns.PresetType preset)
    {
        if(!enableHaptics || preset == HapticPatterns.PresetType.None)
        {
            return;
        }

        HapticPatterns.PlayPreset(preset);
    }

    private void PlayItemPunch(Transform target, float punchAmount)
    {
        if(_punchCoroutine != null)
        {
            StopCoroutine(_punchCoroutine);
        }

        _punchCoroutine = StartCoroutine(PunchScaleCoroutine(target, punchAmount));
    }

    private static IEnumerator PunchScaleCoroutine(Transform target, float punchAmount)
    {
        if(target == null)
        {
            yield break;
        }

        Vector3 originalScale = target.localScale;
        const float duration = 0.16f;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            if(target == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float envelope = Mathf.Sin(t * Mathf.PI);
            float overshoot = punchAmount * envelope * (1f + 0.35f * Mathf.Sin(t * Mathf.PI * 2f));
            target.localScale = originalScale * (1f + overshoot);
            yield return null;
        }

        if(target != null)
        {
            target.localScale = originalScale;
        }
    }
}
