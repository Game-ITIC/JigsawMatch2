using UnityEngine;
using System.Collections;
using DG.Tweening;
using UI;
using UnityEngine.UI;

public class PrePlay : MonoBehaviour
{
    private const float IntroFallbackDelay = 3.2f;

    public GameObject ingrObject;
    public GameObject blocksObject;
    public GameObject scoreTargetObject;
    public GameObject cage;
    public GameObject bomb;
    public GameObject items;

    private Tween introFallback;
    private bool introCompleting;

    // Use this for initialization
    void OnEnable()
    {
        introCompleting = false;
        InitTargets();

        if(TryGetComponent(out UIPrePlayBannerTweenAnimation tweenAnimation))
        {
            tweenAnimation.Play();
        }

        KillIntroFallback();
        introFallback = DOVirtual.DelayedCall(IntroFallbackDelay, CompleteIntro, true)
            .SetTarget(this);
    }

    private void OnDisable()
    {
        KillIntroFallback();
    }

    void InitTargets()
    {
        blocksObject.SetActive(false);
        ingrObject.SetActive(false);
        cage.SetActive(false);
        items.SetActive(false);
        bomb.SetActive(false);
        scoreTargetObject.SetActive(false);
        GameObject ingr1 = ingrObject.transform.Find("Ingr1").gameObject;
        GameObject ingr2 = ingrObject.transform.Find("Ingr2").gameObject;

        ingr1.SetActive(true);
        ingr2.SetActive(true);
        ingr1.GetComponent<RectTransform>().localPosition = new Vector3(-74.37f, ingr1.GetComponent<RectTransform>().localPosition.y, ingr1.GetComponent<RectTransform>().localPosition.z);
        ingr2.GetComponent<RectTransform>().localPosition = new Vector3(50.1f, ingr2.GetComponent<RectTransform>().localPosition.y, ingr2.GetComponent<RectTransform>().localPosition.z);

        if (LevelManager.THIS.target == Target.COLLECT)
        {
            blocksObject.SetActive(false);
            ingrObject.SetActive(true);
            for (int i = 0; i < LevelManager.THIS.ingrTarget.Count; i++)
            {
                ingrObject.transform.Find("Ingr" + (i + 1)).GetComponent<Image>().sprite = LevelManager.THIS.ingrTarget[i].sprite;
            }
        }

        if (LevelManager.THIS.targetBlocks > 0)
        {
            blocksObject.SetActive(true);
        }
        else if (LevelManager.THIS.target == Target.CAGES)
        {
            cage.SetActive(true);
        }
        else if (LevelManager.THIS.target == Target.BOMBS)
        {
            bomb.SetActive(true);
        }
        else if (LevelManager.THIS.target == Target.COLLECT)
        {
            ingrObject.SetActive(true);
        }
        else if (LevelManager.THIS.target == Target.ITEMS)
        {
            items.SetActive(true);
        }
        else if (LevelManager.THIS.target == Target.SCORE)
        {
            ingrObject.SetActive(false);
            blocksObject.SetActive(false);
            scoreTargetObject.SetActive(true);
        }

        else if (LevelManager.THIS.ingrTarget[0].count == 0 && LevelManager.THIS.ingrTarget[1].count == 0)
        {
            ingrObject.SetActive(false);
            blocksObject.SetActive(false);
            scoreTargetObject.SetActive(true);
        }
    }

    public void CompleteIntro()
    {
        if(introCompleting)
        {
            return;
        }

        introCompleting = true;
        KillIntroFallback();

        try
        {
            if(TryGetComponent(out AnimationManager animationManager))
            {
                animationManager.CloseMenu();
            }
        }
        finally
        {
            if(gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }

            if(LevelManager.THIS != null)
            {
                LevelManager.THIS.gameStatus = GameState.WaitForPopup;
            }
        }
    }

    private void KillIntroFallback()
    {
        if(introFallback != null && introFallback.IsActive())
        {
            introFallback.Kill();
        }

        introFallback = null;
    }
}
