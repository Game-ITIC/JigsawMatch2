using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleEffectGUI : MonoBehaviour
{
    [Header("Targets")]
    public List<ScaleAnimation> scaleEffects = new List<ScaleAnimation>();

    [Header("Sequence Settings")]
    public bool playOnStart = true;
    public float delayBetweenEffects = 2f;
    public bool loopSequence = false;

    [Header("GUI Settings")]
    public bool showGUI = true;
    public bool autoFindIfEmpty = true;

    public int x = 20;
    public int y = 20;
    public int buttonWidth = 220;
    public int buttonHeight = 40;
    public int gap = 10;

    private Coroutine sequenceCoroutine;

    private void Awake()
    {
        if (autoFindIfEmpty && scaleEffects.Count == 0)
        {
            ScaleAnimation[] foundEffects = FindObjectsOfType<ScaleAnimation>();
            scaleEffects.AddRange(foundEffects);
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartSequence();
        }
    }

    private void OnDisable()
    {
        StopSequence();
    }

    public void StartSequence()
    {
        StopSequence();
        sequenceCoroutine = StartCoroutine(PlaySequence());
    }

    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
    }

    private IEnumerator PlaySequence()
    {
        // Important:
        // Wait one frame so all ScaleAnimation Start/Awake logic is ready.
        yield return null;

        do
        {
            for (int i = 0; i < scaleEffects.Count; i++)
            {
                ScaleAnimation effect = scaleEffects[i];

                if (effect == null)
                    continue;

                Debug.Log("Starting scale effect: " + effect.gameObject.name);

                effect.StartScaleAnimation();

                yield return new WaitForSeconds(delayBetweenEffects);
            }

        } while (loopSequence);

        sequenceCoroutine = null;
    }

}