using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public sealed class StarTargetCounter : MonoBehaviour
{
    TMP_Text _label;
    TargetGUI _targetGui;
    ProgressBarScript _progressBar;

    void Awake()
    {
        _label = GetComponent<TMP_Text>();
        _targetGui = GetComponentInParent<TargetGUI>();
    }

    void OnEnable()
    {
        StartCoroutine(BindWhenReady());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        Unbind();
    }

    IEnumerator BindWhenReady()
    {
        while (isActiveAndEnabled &&
               (ProgressBarScript.Instance == null || LevelManager.THIS == null))
        {
            yield return null;
        }

        if (!isActiveAndEnabled)
        {
            yield break;
        }

        _progressBar = ProgressBarScript.Instance;
        _progressBar.DisplayedStarsChanged += Refresh;
        Refresh(_progressBar.DisplayedStars);
    }

    void Unbind()
    {
        if (_progressBar == null)
        {
            return;
        }

        _progressBar.DisplayedStarsChanged -= Refresh;
        _progressBar = null;
    }

    void Refresh(int displayedStars)
    {
        LevelManager levelManager = LevelManager.THIS;
        if (levelManager == null)
        {
            return;
        }

        int requiredStars = levelManager.RequiredStars;
        int currentStars = Mathf.Min(displayedStars, requiredStars);
        _label.SetText("{0}/{1}", currentStars, requiredStars);

        if (currentStars >= requiredStars)
        {
            _targetGui?.Done();
        }
    }
}
