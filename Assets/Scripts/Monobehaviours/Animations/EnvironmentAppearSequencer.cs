using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnvironmentAppearSequencer : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private bool autoCollectDirectChildren = true;
    [SerializeField] private bool skipInactive = true;
    [SerializeField] private bool requireVisibleRenderer = true;
    [SerializeField] private List<Transform> targets = new List<Transform>();
    [SerializeField] private List<string> skipNames = new List<string> { "Street Lamps", "Terrain" };
    [SerializeField] private List<string> appearPriority = new List<string>
    {
        "Korean_Island_",
        "Pagoda",
        "Korean_building",
        "Korea House",
        "bridge",
        "fonar",
        "Tree 01",
        "tree (3)",
        "all_stones",
        "all_rocks"
    };

    [Header("Sequence")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float initialDelay = 0.2f;
    [SerializeField] private float delayBetweenEffects = 0.16f;

    [Header("Animation")]
    [SerializeField] private ScaleAnimation.ScalePreset preset = ScaleAnimation.ScalePreset.ElasticJelly;
    [SerializeField] private float duration = 0.55f;

    private readonly List<ScaleAnimation> scaleEffects = new List<ScaleAnimation>();
    private Coroutine sequenceCoroutine;

    private void Awake()
    {
        BuildEffects();
    }

    private void Start()
    {
        if (playOnStart)
            PlaySequence();
    }

    private void OnDisable()
    {
        StopSequence();
    }

    public void PlaySequence()
    {
        StopSequence();
        sequenceCoroutine = StartCoroutine(PlaySequenceRoutine());
    }

    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
    }

    private IEnumerator PlaySequenceRoutine()
    {
        // Wait until ScaleAnimation.Start/Init has finished on all targets.
        yield return null;
        yield return null;

        if (scaleEffects.Count == 0)
        {
            Debug.LogWarning($"{nameof(EnvironmentAppearSequencer)} found no appear targets on '{name}'.", this);
            sequenceCoroutine = null;
            yield break;
        }

        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < scaleEffects.Count; i++)
        {
            ScaleAnimation effect = scaleEffects[i];

            if (effect == null)
                continue;

            ApplySettings(effect);
            effect.StartScaleAnimation();

            if (i < scaleEffects.Count - 1 && delayBetweenEffects > 0f)
                yield return new WaitForSeconds(delayBetweenEffects);
        }

        sequenceCoroutine = null;
    }

    private void BuildEffects()
    {
        scaleEffects.Clear();

        List<Transform> resolvedTargets = ResolveTargets();
        SortTargets(resolvedTargets);

        for (int i = 0; i < resolvedTargets.Count; i++)
        {
            Transform targetTransform = resolvedTargets[i];
            if (targetTransform == null)
                continue;

            ScaleAnimation effect = targetTransform.GetComponent<ScaleAnimation>();
            if (effect == null)
                effect = targetTransform.gameObject.AddComponent<ScaleAnimation>();

            ApplySettings(effect);
            scaleEffects.Add(effect);
        }
    }

    private List<Transform> ResolveTargets()
    {
        if (!autoCollectDirectChildren)
            return new List<Transform>(targets);

        List<Transform> resolved = new List<Transform>();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (ShouldSkip(child))
                continue;

            resolved.Add(child);
        }

        return resolved;
    }

    private bool ShouldSkip(Transform child)
    {
        if (child == null)
            return true;

        if (skipInactive && !child.gameObject.activeInHierarchy)
            return true;

        for (int i = 0; i < skipNames.Count; i++)
        {
            if (string.Equals(child.name, skipNames[i], StringComparison.Ordinal))
                return true;
        }

        if (!requireVisibleRenderer)
            return false;

        return child.GetComponentInChildren<Renderer>(true) == null;
    }

    private void SortTargets(List<Transform> resolvedTargets)
    {
        resolvedTargets.Sort((a, b) =>
        {
            int aPriority = GetPriorityIndex(a.name);
            int bPriority = GetPriorityIndex(b.name);

            if (aPriority != bPriority)
                return aPriority.CompareTo(bPriority);

            return a.GetSiblingIndex().CompareTo(b.GetSiblingIndex());
        });
    }

    private int GetPriorityIndex(string objectName)
    {
        for (int i = 0; i < appearPriority.Count; i++)
        {
            if (string.Equals(objectName, appearPriority[i], StringComparison.Ordinal))
                return i;
        }

        return appearPriority.Count + 1;
    }

    private void ApplySettings(ScaleAnimation effect)
    {
        effect.startOnZero = true;
        effect.preset = preset;
        effect.duration = duration;
        effect.loop = false;
    }
}
