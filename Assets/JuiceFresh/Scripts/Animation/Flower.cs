using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls the visual "flower" effect that flies to items when creating special items
/// </summary>
public class Flower : MonoBehaviour
{
    [SerializeField] private float speedInGame = 15f;
    [SerializeField] private float speedInTheEnd = 50f;
    private const int MaxRetargetAttempts = 5;
    
    #region Private Fields
    private Item targetItem;
    private SpriteRenderer spriteRenderer;
    private ParticleSystem particleSystem;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        particleSystem = GetComponent<ParticleSystem>();
    }
    
    void Start() 
    {
        particleSystem.Stop();
    }

    void Update() 
    {
        if (!spriteRenderer.enabled)
            return;

        transform.Rotate(Vector3.back * Time.deltaTime * 1000);
        transform.position = new Vector3(transform.position.x, transform.position.y, -15f);
    }
    #endregion

    #region Public Methods
    public void StartFly(Vector3 startPosition, bool directFly = false) 
    {
        StopAllCoroutines();
        StartCoroutine(FlyCor(startPosition, directFly));
    }
    #endregion

    #region Private Methods
    private IEnumerator FlyCor(Vector3 startPosition, bool directFly = false) 
    {
        spriteRenderer.enabled = true;
        bool appliedEffect = false;

        try
        {
            yield return new WaitForFixedUpdate();
            transform.position = startPosition;

            float dragWaitDeadline = Time.time + 10f;
            while (LevelManager.THIS.DragBlocked)
            {
                if (Time.time >= dragWaitDeadline)
                    break;

                yield return new WaitForEndOfFrame();
            }

            Vector3 flightStart = startPosition;
            int retargetAttempts = 0;

            while (true)
            {
                FindTargetItem();

                if (targetItem == null)
                    yield break;

                Item trackedItem = targetItem;
                trackedItem.nextType = (ItemsTypes)Random.Range(1, 3);
                Vector3 targetPosition = trackedItem.transform.position;

                float startTime = Time.time;
                float distance = Vector3.Distance(flightStart, targetPosition);
                float speed = directFly ? speedInTheEnd : speedInGame;

                if (distance < 0.01f)
                {
                    ApplyFlowerEffect(trackedItem);
                    appliedEffect = true;
                    yield break;
                }

                if (!particleSystem.isPlaying)
                    particleSystem.Play();

                float fracJourney = 0;
                bool needsRetarget = false;

                while (fracJourney < 1)
                {
                    if (trackedItem == null || trackedItem.gameObject == null)
                        yield break;

                    if (!directFly && trackedItem.awaken)
                    {
                        trackedItem.nextType = ItemsTypes.NONE;
                        flightStart = transform.position;
                        needsRetarget = true;
                        break;
                    }

                    float distCovered = (Time.time - startTime) * speed;
                    fracJourney = distCovered / distance;

                    if (float.IsNaN(fracJourney) || float.IsInfinity(fracJourney))
                        fracJourney = 1f;

                    transform.position = Vector3.Lerp(flightStart, targetPosition, fracJourney);
                    yield return new WaitForFixedUpdate();
                }

                if (needsRetarget)
                {
                    retargetAttempts++;
                    if (retargetAttempts >= MaxRetargetAttempts)
                        yield break;

                    continue;
                }

                particleSystem.gravityModifier = 0;
                ApplyFlowerEffect(trackedItem);
                appliedEffect = true;
                yield break;
            }
        }
        finally
        {
            CleanupFlower();

            if (appliedEffect)
                LevelManager.THIS.DragBlocked = false;
        }
    }

    private void FindTargetItem()
    {
        targetItem = null;
        List<Item> items = LevelManager.THIS.GetRandomItems(1);
        foreach (Item item in items)
        {
            targetItem = item;
        }
    }

    private void CleanupFlower()
    {
        particleSystem.Stop();
        spriteRenderer.enabled = false;
    }

    private void ApplyFlowerEffect(Item item)
    {
        if (item != null)
            item.ChangeType();
    }
    #endregion
}
