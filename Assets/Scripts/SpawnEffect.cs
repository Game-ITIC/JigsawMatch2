using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpawnEffect : MonoBehaviour
{
    private float animationDuration = 0.5f;
    private float jumpPower = 2f;
    private float overshootScale = 1.5f;

    [SerializeField] private GameObject buildUI;


    void Start()
    {
        //PlaySpawnAnimation();
    }

    public void PlaySpawnAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 startScale = originalScale * 0.1f;
        Vector3 bigScale = originalScale * overshootScale;
        Vector3 startPosition = transform.position;

        transform.localScale = startScale;

        Sequence spawnSequence = DOTween.Sequence();
        spawnSequence.Append(transform.DOJump(startPosition, jumpPower, 1, animationDuration));
        spawnSequence.Join(transform.DOScale(bigScale, animationDuration));
        spawnSequence.Append(transform.DOScale(originalScale, animationDuration));

        //sound
        GetComponent<AudioSource>().Play();

        //hide UI
        buildUI.SetActive(false);
    }
}
