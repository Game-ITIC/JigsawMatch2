using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class LevelEffectsController : MonoBehaviour
{
    [SerializeField] private GameObject stripesEffectPrefab;
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField, Min(1)] private int poolSize = 20;
    [SerializeField, Min(0f)] private float explosionLifetime = 1f;
    [SerializeField, Min(0f)] private float doubleBombStepDelay = 0.3f;

    private LevelManager levelManager;
    private GameObject[] explosionPool;
    private GameObject[] flowerPool;

    public void Initialize(LevelManager owner)
    {
        if(levelManager == owner && explosionPool != null && flowerPool != null)
        {
            return;
        }

        levelManager = owner;
        CreatePools();
    }

    public void PlayRandomColorReveal()
    {
        StartCoroutine(RandomColorRevealCoroutine());
    }

    public void PlayDoubleBombWave(int column)
    {
        StartCoroutine(DoubleBombWaveForward(column));
        StartCoroutine(DoubleBombWaveBackward(column));
    }

    public void PlayStripedEffect(GameObject source, bool horizontal)
    {
        if(stripesEffectPrefab == null || source == null)
        {
            return;
        }

        GameObject effect = Instantiate(stripesEffectPrefab, source.transform.position, Quaternion.identity);
        if(!horizontal)
        {
            effect.transform.Rotate(Vector3.back, 90f);
        }

        Destroy(effect, 1f);
    }

    public GameObject GetExplosion()
    {
        if(explosionPool == null)
        {
            return null;
        }

        foreach(GameObject effect in explosionPool)
        {
            SpriteRenderer renderer = effect != null ? effect.GetComponent<SpriteRenderer>() : null;
            if(renderer == null || renderer.enabled)
            {
                continue;
            }

            renderer.enabled = true;
            StartCoroutine(HideExplosion(effect, renderer));
            return effect;
        }

        return null;
    }

    public GameObject GetFlower()
    {
        if(flowerPool == null)
        {
            return null;
        }

        foreach(GameObject flower in flowerPool)
        {
            SpriteRenderer renderer = flower != null ? flower.GetComponent<SpriteRenderer>() : null;
            if(renderer != null && !renderer.enabled)
            {
                return flower;
            }
        }

        return null;
    }

    public bool HasFlyingFlowers()
    {
        if(flowerPool == null)
        {
            return false;
        }

        foreach(GameObject flower in flowerPool)
        {
            SpriteRenderer renderer = flower != null ? flower.GetComponent<SpriteRenderer>() : null;
            if(renderer != null && renderer.enabled)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void CreatePools()
    {
        GameObject explosionPrefab = Resources.Load<GameObject>("Prefabs/Effects/ItemExpl");
        explosionPool = new GameObject[poolSize];
        flowerPool = new GameObject[poolSize];

        for(int i = 0; i < poolSize; i++)
        {
            explosionPool[i] = CreatePooledEffect(explosionPrefab);
            flowerPool[i] = CreatePooledEffect(flowerPrefab);
        }
    }

    private GameObject CreatePooledEffect(GameObject prefab)
    {
        if(prefab == null)
        {
            return null;
        }

        GameObject instance = Instantiate(prefab, transform.position, Quaternion.identity);
        SpriteRenderer renderer = instance.GetComponent<SpriteRenderer>();
        if(renderer != null)
        {
            renderer.enabled = false;
        }

        return instance;
    }

    private IEnumerator RandomColorRevealCoroutine()
    {
        if(levelManager == null)
        {
            yield break;
        }

        int color = Random.Range(0, levelManager.colorLimit);
        List<Item> items = levelManager.GetRandomItems(GameObject.FindGameObjectsWithTag("Item").Length / 3);

        foreach(Item item in items)
        {
            yield return new WaitForSeconds(0.01f);
            item.SetColor(color);
            item.PlayAppearAnimation();
        }
    }

    private IEnumerator DoubleBombWaveForward(int column)
    {
        for(int current = column; current < levelManager.maxCols; current++)
        {
            DestroyColumn(current);
            yield return new WaitForSeconds(doubleBombStepDelay);
        }

        if(column <= levelManager.maxCols - column - 1)
        {
            levelManager.FindMatches();
        }
    }

    private IEnumerator DoubleBombWaveBackward(int column)
    {
        for(int current = column - 1; current >= 0; current--)
        {
            DestroyColumn(current);
            yield return new WaitForSeconds(doubleBombStepDelay);
        }

        if(column > levelManager.maxCols - column - 1)
        {
            levelManager.FindMatches();
        }
    }

    private void DestroyColumn(int column)
    {
        foreach(Item item in levelManager.GetColumn(column))
        {
            if(item != null)
            {
                item.DestroyItem(true, "", true);
            }
        }
    }

    private IEnumerator HideExplosion(GameObject effect, SpriteRenderer renderer)
    {
        yield return new WaitForSeconds(explosionLifetime);

        Animator animator = effect.GetComponent<Animator>();
        if(animator != null)
        {
            animator.SetTrigger("stop");
            animator.SetInteger("color", 10);
        }

        renderer.enabled = false;
    }
}
