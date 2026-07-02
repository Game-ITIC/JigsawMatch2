using System.Collections;
using System.Collections.Generic;
using JuiceFresh;
using Lofelt.NiceVibrations;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

/// <summary>
/// Central hub for soft Feel feedback: gentle pulses, flutter motion, restrained camera shake and haptics.
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
    [SerializeField] private bool enableFreezeFrames;
    [SerializeField] private bool enableItemPunch = true;

    [Header("Soft Feel Intensity")]
    [SerializeField, Range(0.25f, 1.5f)] private float globalIntensity = 0.9f;
    [SerializeField, Range(0f, 0.3f)] private float matchShakeStrength = 0.045f;
    [SerializeField, Range(0f, 0.3f)] private float boardShakeStrength = 0.06f;
    [SerializeField, Range(0f, 0.3f)] private float comboShakeStrength = 0.085f;
    [SerializeField, Range(0f, 0.4f)] private float winShakeStrength = 0.14f;
    [SerializeField, Range(0f, 0.2f)] private float itemPulseStrength = 0.095f;
    [SerializeField, Range(0f, 0.08f)] private float boardPulseStrength = 0.028f;

    [Header("Selection Glow")]
    [SerializeField, Range(0f, 0.6f)] private float selectionGlowAlpha = 0.26f;
    [SerializeField, Range(1f, 1.5f)] private float selectionGlowScale = 1.12f;
    [SerializeField, Range(0f, 0.1f)] private float selectionGlowBreathing = 0.035f;

    private MMF_Player _matchPlayer;
    private MMF_Player _comboPlayer;
    private MMF_Player _winPlayer;
    private Camera _gameCamera;
    private readonly Dictionary<int, ItemPulseState> _itemPulses = new Dictionary<int, ItemPulseState>();
    private readonly Dictionary<int, SelectionGlowState> _selectionGlows = new Dictionary<int, SelectionGlowState>();
    private readonly List<GameObject> _releasedGlowObjects = new List<GameObject>();
    private Coroutine _boardMotionCoroutine;
    private Coroutine _boardPulseCoroutine;
    private Vector3 _boardRestPosition;
    private Vector3 _boardRestScale;

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
        RestoreAnimatedTransforms();

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
        if(board == null || boardRoot == board)
        {
            return;
        }

        RestoreBoardTransform();
        boardRoot = board;
    }

    public void OnItemSelected(Transform item, int chainCount)
    {
        if(enableItemPunch && item != null)
        {
            float pulse = itemPulseStrength * globalIntensity + Mathf.Min(chainCount - 1, 9) * 0.005f;
            PlayItemPulse(item, pulse);
            AddSelectionGlow(item, chainCount);
        }

        bool milestone = chainCount == 3 || chainCount == 5 || chainCount == 8;
        if(milestone)
        {
            float milestoneStrength = boardPulseStrength * globalIntensity * (0.45f + chainCount * 0.035f);
            TriggerBoardPulse(0.18f, milestoneStrength);
            PlayHaptic(chainCount >= 8
                ? HapticPatterns.PresetType.SoftImpact
                : HapticPatterns.PresetType.LightImpact);
        }
        else
        {
            PlayHaptic(HapticPatterns.PresetType.Selection);
        }
    }

    public void OnItemDeselected(Transform removedItem, Transform remainingItem, int chainCount)
    {
        RemoveSelectionGlow(removedItem, false);

        if(enableItemPunch && remainingItem != null)
        {
            float pulse = itemPulseStrength * globalIntensity * (0.48f + Mathf.Min(chainCount, 8) * 0.015f);
            PlayItemPulse(remainingItem, pulse);
        }

        PlayHaptic(HapticPatterns.PresetType.Selection);
    }

    public void OnItemDestroyed(Vector3 position)
    {
        // Per-item camera shake was too weak and got lost in chain pops.
    }

    public void OnMatchReleased(int matchedCount, Vector3 centerPosition)
    {
        CompleteAllItemPulses();
        ReleaseAllSelectionGlows(true);
        float normalized = Mathf.Clamp01((matchedCount - 3f) / 7f);

        TriggerBoardFlutter(
            0.2f + normalized * 0.08f,
            boardShakeStrength * globalIntensity * (0.45f + normalized * 0.35f));
        TriggerBoardPulse(0.24f, boardPulseStrength * globalIntensity * (0.75f + normalized * 0.5f));

        if(enableCameraShake)
        {
            _matchPlayer?.PlayFeedbacks(centerPosition, 0.35f + normalized * 0.25f);
        }

        PlayHaptic(matchedCount >= 6
            ? HapticPatterns.PresetType.SoftImpact
            : HapticPatterns.PresetType.LightImpact);
    }

    public void OnCombo(int combo)
    {
        if(combo < 6)
        {
            return;
        }

        float normalized = Mathf.Clamp01((combo - 6f) / 8f);

        TriggerBoardFlutter(0.26f + normalized * 0.08f, boardShakeStrength * globalIntensity * 0.7f);
        TriggerBoardPulse(0.28f, boardPulseStrength * globalIntensity * (1f + normalized * 0.45f));
        if(enableCameraShake)
        {
            _comboPlayer?.PlayFeedbacks(Vector3.zero, 0.5f + normalized * 0.25f);
        }

        if(enableFreezeFrames && combo >= 12)
        {
            MMFreezeFrameEvent.Trigger(0.012f);
        }

        PlayHaptic(HapticPatterns.PresetType.SoftImpact);
    }

    public void OnBoostExplosion(Vector3 position)
    {
        TriggerBoardFlutter(0.32f, boardShakeStrength * globalIntensity * 1.15f);
        TriggerBoardPulse(0.3f, boardPulseStrength * globalIntensity * 1.35f);
        if(enableCameraShake)
        {
            _comboPlayer?.PlayFeedbacks(position, 0.9f);
        }

        if(enableFreezeFrames)
        {
            MMFreezeFrameEvent.Trigger(0.018f);
        }

        PlayHaptic(HapticPatterns.PresetType.MediumImpact);
    }

    public void OnInvalidMove()
    {
        CompleteAllItemPulses();
        ReleaseAllSelectionGlows(false);
        TriggerBoardFlutter(0.16f, boardShakeStrength * globalIntensity * 0.32f);
        PlayHaptic(HapticPatterns.PresetType.Selection);
    }

    public void OnPreWin()
    {
        TriggerBoardFlutter(0.3f, boardShakeStrength * globalIntensity * 0.55f);
        TriggerBoardPulse(0.34f, boardPulseStrength * globalIntensity * 1.1f);
        PlayHaptic(HapticPatterns.PresetType.SoftImpact);
    }

    public void OnWin()
    {
        TriggerBoardFlutter(0.42f, boardShakeStrength * globalIntensity * 0.8f);
        TriggerBoardPulse(0.45f, boardPulseStrength * globalIntensity * 1.4f);
        if(enableCameraShake)
        {
            _winPlayer?.PlayFeedbacks(Vector3.zero, 0.8f);
        }
        PlayHaptic(HapticPatterns.PresetType.Success);
    }

    public void OnLose()
    {
        TriggerBoardFlutter(0.34f, boardShakeStrength * globalIntensity * 0.45f);
        PlayHaptic(HapticPatterns.PresetType.Warning);
    }

    public void OnTargetCollected()
    {
        PlayHaptic(HapticPatterns.PresetType.Selection);
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
        ConfigureCameraShake(_matchPlayer, 0.2f, matchShakeStrength * globalIntensity, 12f);

        _comboPlayer = CreatePlayer("Feel_Combo");
        ConfigureCameraShake(_comboPlayer, 0.26f, comboShakeStrength * globalIntensity, 10f);

        _winPlayer = CreatePlayer("Feel_Win");
        ConfigureCameraShake(_winPlayer, 0.38f, winShakeStrength * globalIntensity, 8f);
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

    private void TriggerBoardFlutter(float duration, float magnitude)
    {
        if(!enableBoardShake || boardRoot == null || magnitude <= 0f)
        {
            return;
        }

        if(_boardMotionCoroutine != null)
        {
            StopCoroutine(_boardMotionCoroutine);
            boardRoot.localPosition = _boardRestPosition;
        }

        _boardRestPosition = boardRoot.localPosition;
        _boardMotionCoroutine = StartCoroutine(BoardFlutterCoroutine(duration, magnitude));
    }

    private IEnumerator BoardFlutterCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            if(boardRoot == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float envelope = Mathf.Sin(t * Mathf.PI);
            float x = Mathf.Sin(t * Mathf.PI * 3f) * magnitude * envelope;
            float y = Mathf.Sin(t * Mathf.PI * 2f) * magnitude * 0.22f * envelope;
            boardRoot.localPosition = _boardRestPosition + new Vector3(x, y, 0f);
            yield return null;
        }

        if(boardRoot != null)
        {
            boardRoot.localPosition = _boardRestPosition;
        }

        _boardMotionCoroutine = null;
    }

    private void TriggerBoardPulse(float duration, float strength)
    {
        if(boardRoot == null || strength <= 0f)
        {
            return;
        }

        if(_boardPulseCoroutine != null)
        {
            StopCoroutine(_boardPulseCoroutine);
            boardRoot.localScale = _boardRestScale;
        }

        _boardRestScale = boardRoot.localScale;
        _boardPulseCoroutine = StartCoroutine(BoardPulseCoroutine(duration, strength));
    }

    private IEnumerator BoardPulseCoroutine(float duration, float strength)
    {
        float elapsed = 0f;

        while(elapsed < duration)
        {
            if(boardRoot == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float pulse = Mathf.Sin(t * Mathf.PI) * strength;
            boardRoot.localScale = _boardRestScale * (1f + pulse);
            yield return null;
        }

        if(boardRoot != null)
        {
            boardRoot.localScale = _boardRestScale;
        }

        _boardPulseCoroutine = null;
    }

    private void AddSelectionGlow(Transform target, int chainCount)
    {
        if(target == null)
        {
            return;
        }

        int id = target.GetInstanceID();
        if(_selectionGlows.ContainsKey(id))
        {
            return;
        }

        Item item = target.GetComponent<Item>();
        SpriteRenderer source = item != null && item.sprRenderer != null
            ? item.sprRenderer
            : target.GetComponentInChildren<SpriteRenderer>();
        if(source == null || source.sprite == null)
        {
            return;
        }

        GameObject auraObject = new GameObject("Feel_SelectionGlow");
        auraObject.layer = source.gameObject.layer;
        auraObject.transform.SetParent(source.transform.parent, false);
        auraObject.transform.localPosition = source.transform.localPosition;
        auraObject.transform.localRotation = source.transform.localRotation;
        auraObject.transform.localScale = source.transform.localScale * selectionGlowScale;

        SpriteRenderer aura = auraObject.AddComponent<SpriteRenderer>();
        aura.sprite = source.sprite;
        aura.sharedMaterial = source.sharedMaterial;
        aura.sortingLayerID = source.sortingLayerID;
        aura.sortingOrder = source.sortingOrder + 2;
        aura.flipX = source.flipX;
        aura.flipY = source.flipY;

        float chainEnergy = Mathf.InverseLerp(1f, 10f, chainCount);
        Color glowColor = Color.Lerp(
            new Color(0.86f, 0.96f, 1f, selectionGlowAlpha),
            new Color(1f, 0.88f, 0.55f, selectionGlowAlpha + 0.08f),
            chainEnergy);
        aura.color = glowColor;

        SelectionGlowState state = new SelectionGlowState(
            target,
            source,
            auraObject,
            aura,
            source.transform.localScale,
            glowColor);
        state.Coroutine = StartCoroutine(SelectionGlowCoroutine(state, chainCount));
        _selectionGlows[id] = state;
    }

    private IEnumerator SelectionGlowCoroutine(SelectionGlowState state, int chainCount)
    {
        float elapsed = chainCount * 0.17f;

        while(state.Target != null && state.Source != null && state.AuraObject != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float wave = Mathf.Sin(elapsed * 5.5f) * 0.5f + 0.5f;
            float scale = selectionGlowScale + wave * selectionGlowBreathing;
            state.AuraObject.transform.localScale = state.RestScale * scale;
            state.Aura.sprite = state.Source.sprite;

            Color color = state.GlowColor;
            color.a *= 0.78f + wave * 0.22f;
            state.Aura.color = color;
            yield return null;
        }
    }

    private void RemoveSelectionGlow(Transform target, bool successfulRelease)
    {
        if(target == null)
        {
            return;
        }

        int id = target.GetInstanceID();
        if(!_selectionGlows.TryGetValue(id, out SelectionGlowState state))
        {
            return;
        }

        if(state.Coroutine != null)
        {
            StopCoroutine(state.Coroutine);
        }

        _selectionGlows.Remove(id);
        StartCoroutine(ReleaseSelectionGlowCoroutine(state, successfulRelease));
    }

    private void ReleaseAllSelectionGlows(bool successfulRelease)
    {
        if(_selectionGlows.Count == 0)
        {
            return;
        }

        List<SelectionGlowState> activeGlows = new List<SelectionGlowState>(_selectionGlows.Values);
        _selectionGlows.Clear();

        foreach(SelectionGlowState state in activeGlows)
        {
            if(state.Coroutine != null)
            {
                StopCoroutine(state.Coroutine);
            }

            StartCoroutine(ReleaseSelectionGlowCoroutine(state, successfulRelease));
        }
    }

    private IEnumerator ReleaseSelectionGlowCoroutine(SelectionGlowState state, bool successfulRelease)
    {
        if(state.AuraObject == null || state.Aura == null)
        {
            yield break;
        }

        state.AuraObject.transform.SetParent(null, true);
        _releasedGlowObjects.Add(state.AuraObject);

        Vector3 startScale = state.AuraObject.transform.localScale;
        Vector3 endScale = startScale * (successfulRelease ? 1.48f : 0.82f);
        Vector3 startPosition = state.AuraObject.transform.position;
        float startAlpha = state.Aura.color.a;
        float duration = successfulRelease ? 0.3f : 0.18f;
        float elapsed = 0f;

        while(elapsed < duration && state.AuraObject != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            state.AuraObject.transform.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
            state.AuraObject.transform.position = startPosition + Vector3.up * (0.12f * eased);

            Color color = state.Aura.color;
            color.a = Mathf.Lerp(startAlpha, 0f, t * t);
            state.Aura.color = color;
            yield return null;
        }

        _releasedGlowObjects.Remove(state.AuraObject);
        if(state.AuraObject != null)
        {
            Destroy(state.AuraObject);
        }
    }

    private void PlayHaptic(HapticPatterns.PresetType preset)
    {
        if(!enableHaptics || preset == HapticPatterns.PresetType.None)
        {
            return;
        }

        HapticPatterns.PlayPreset(preset);
    }

    private void PlayItemPulse(Transform target, float pulseAmount)
    {
        int id = target.GetInstanceID();
        if(_itemPulses.TryGetValue(id, out ItemPulseState activePulse))
        {
            if(activePulse.Coroutine != null)
            {
                StopCoroutine(activePulse.Coroutine);
            }

            if(activePulse.Target != null)
            {
                activePulse.Target.localScale = activePulse.RestScale;
            }
        }

        ItemPulseState pulse = new ItemPulseState(target, GetItemPulseRestScale(target));
        pulse.Coroutine = StartCoroutine(ItemPulseCoroutine(id, pulse, pulseAmount));
        _itemPulses[id] = pulse;
    }

    private static Vector3 GetItemPulseRestScale(Transform target)
    {
        if(target == null)
            return Vector3.one;

        if(target.GetComponent<Item>() == null)
            return target.localScale;

        const float expectedScale = 0.9f;
        Vector3 scale = target.localScale;
        if(scale.x < expectedScale * 0.75f || scale.y < expectedScale * 0.75f)
            return Vector3.one * expectedScale;

        return scale;
    }

    private IEnumerator ItemPulseCoroutine(int id, ItemPulseState pulse, float pulseAmount)
    {
        if(pulse.Target == null)
        {
            _itemPulses.Remove(id);
            yield break;
        }

        const float duration = 0.2f;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            if(pulse.Target == null)
            {
                _itemPulses.Remove(id);
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float envelope = Mathf.Sin(t * Mathf.PI);
            pulse.Target.localScale = pulse.RestScale * (1f + pulseAmount * envelope);
            yield return null;
        }

        if(pulse.Target != null)
        {
            pulse.Target.localScale = pulse.RestScale;
        }

        _itemPulses.Remove(id);
    }

    private void CompleteAllItemPulses()
    {
        if(_itemPulses.Count == 0)
        {
            return;
        }

        foreach(ItemPulseState pulse in _itemPulses.Values)
        {
            if(pulse.Coroutine != null)
            {
                StopCoroutine(pulse.Coroutine);
            }

            if(pulse.Target != null)
            {
                pulse.Target.localScale = pulse.RestScale;
            }
        }

        _itemPulses.Clear();
    }

    private void RestoreAnimatedTransforms()
    {
        CompleteAllItemPulses();

        foreach(SelectionGlowState glow in _selectionGlows.Values)
        {
            if(glow.AuraObject != null)
            {
                Destroy(glow.AuraObject);
            }
        }

        _selectionGlows.Clear();

        for(int i = _releasedGlowObjects.Count - 1; i >= 0; i--)
        {
            if(_releasedGlowObjects[i] != null)
            {
                Destroy(_releasedGlowObjects[i]);
            }
        }

        _releasedGlowObjects.Clear();
        RestoreBoardTransform();
    }

    private void RestoreBoardTransform()
    {
        if(boardRoot == null)
        {
            return;
        }

        if(_boardMotionCoroutine != null)
        {
            StopCoroutine(_boardMotionCoroutine);
            _boardMotionCoroutine = null;
            boardRoot.localPosition = _boardRestPosition;
        }

        if(_boardPulseCoroutine != null)
        {
            StopCoroutine(_boardPulseCoroutine);
            _boardPulseCoroutine = null;
            boardRoot.localScale = _boardRestScale;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        RestoreAnimatedTransforms();
    }

    private sealed class ItemPulseState
    {
        public ItemPulseState(Transform target, Vector3 restScale)
        {
            Target = target;
            RestScale = restScale;
        }

        public Transform Target { get; }
        public Vector3 RestScale { get; }
        public Coroutine Coroutine { get; set; }
    }

    private sealed class SelectionGlowState
    {
        public SelectionGlowState(
            Transform target,
            SpriteRenderer source,
            GameObject auraObject,
            SpriteRenderer aura,
            Vector3 restScale,
            Color glowColor)
        {
            Target = target;
            Source = source;
            AuraObject = auraObject;
            Aura = aura;
            RestScale = restScale;
            GlowColor = glowColor;
        }

        public Transform Target { get; }
        public SpriteRenderer Source { get; }
        public GameObject AuraObject { get; }
        public SpriteRenderer Aura { get; }
        public Vector3 RestScale { get; }
        public Color GlowColor { get; }
        public Coroutine Coroutine { get; set; }
    }
}
