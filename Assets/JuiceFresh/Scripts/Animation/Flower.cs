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
        // Ensure the particle system is stopped on initialization
        particleSystem.Stop();
    }

    void Update() 
    {
        // Keep the flower rotating and at proper depth
        transform.Rotate(Vector3.back * Time.deltaTime * 1000);
        transform.position = new Vector3(transform.position.x, transform.position.y, -15f);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Starts the flower animation flying from a given position
    /// </summary>
    /// <param name="startPosition">Starting position for the flower</param>
    /// <param name="directFly">If true, flies directly and faster</param>
    public void StartFly(Vector3 startPosition, bool directFly = false) 
    {
        spriteRenderer.enabled = true;
        StartCoroutine(FlyCor(startPosition, directFly));
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Coroutine to handle the flying animation
    /// </summary>
    private IEnumerator FlyCor(Vector3 startPosition, bool directFly = false) 
    {
        Vector3 targetPosition = Vector3.zero;
        
        yield return new WaitForFixedUpdate();

        // Set initial position
        transform.position = startPosition;
        
        // Wait until we're not drag-blocked
        while (LevelManager.THIS.DragBlocked) 
        {
            yield return new WaitForEndOfFrame();
        }
        
        // Find a random target item
        FindTargetItem();
        
        if (targetItem == null) 
        {
            CleanupFlower();
            yield break;
        }
        
        // Store the target position and item
        targetPosition = targetItem.transform.position;
        Item trackedItem = targetItem;
        
        // Set the target item to change to a special item
        trackedItem.nextType = (ItemsTypes)Random.Range(1, 3);
        
        // Calculate movement parameters
        float startTime = Time.time;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float fracJourney = 0;
        var speed = directFly ? speedInTheEnd : speedInGame;
        
        
        // Start particle effect
        particleSystem.Play();

        // Animate the flower flying to the target
        while (fracJourney < 1) 
        {
            // If the target item changed state, retarget
            if (trackedItem.awaken && trackedItem.gameObject != null) 
            {
                trackedItem.nextType = ItemsTypes.NONE;
                StartFly(transform.position, directFly);
                yield break;
            }
            
            // Calculate movement
            float distCovered = (Time.time - startTime) * speed;
            fracJourney = distCovered / distance;
            
            // Handle edge case for NaN
            if (float.IsNaN(fracJourney))
                fracJourney = 0;
                
            // Move the flower
            transform.position = Vector3.Lerp(startPosition, targetPosition, fracJourney);
            yield return new WaitForFixedUpdate();
        }

        // Complete the animation
        particleSystem.gravityModifier = 0;
        AnimationComplete();
    }

    /// <summary>
    /// Finds a random target item for the flower to fly to
    /// </summary>
    private void FindTargetItem()
    {
        List<Item> items = LevelManager.THIS.GetRandomItems(1);
        foreach (Item item in items) 
        {
            targetItem = item;
        }
    }

    /// <summary>
    /// Cleans up the flower when animation is complete or canceled
    /// </summary>
    private void CleanupFlower()
    {
        particleSystem.Stop();
        spriteRenderer.enabled = false;
    }

    /// <summary>
    /// Completes the animation and applies the effect to the target item
    /// </summary>
    private void AnimationComplete() 
    {
        CleanupFlower();
        
        // Change the target item's type
        targetItem.ChangeType();
        
        // Unblock dragging
        LevelManager.THIS.DragBlocked = false;
    }
    #endregion
}