// PreWinAnimationsState.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JuiceFresh.States;

public class PreWinAnimationsState : GameStateBase
{
    private Coroutine animationCoroutine;

    public PreWinAnimationsState(LevelManager levelManager) : base(levelManager)
    {
    }

    public override void EnterState()
    {
        // Stop the game music
        MusicBase.Instance.GetComponent<AudioSource>().Stop();

        // Start the pre-win animations
        animationCoroutine = levelManager.StartCoroutine(PreWinAnimationsCor());
    }

    public override void UpdateState()
    {
        // No specific update logic for pre-win animations state
    }

    public override void ExitState()
    {
        if (animationCoroutine != null)
        {
            levelManager.StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

   private IEnumerator PreWinAnimationsCor()
    {
        // Log for debugging
        Debug.Log("Starting PreWinAnimationsCor");

        // Add life if configured to not lose life every game
        // if (!InitScript.Instance.losingLifeEveryGame)
        //     InitScript.Instance.AddLife(1);

        // Play completion sound
        SoundBase.Instance.PlaySound(SoundBase.Instance.complete[1]);
        
        // Find and show pre-complete banner
        GameObject preCompleteBanner = GameObject.Find("Level/Canvas").transform.Find("PreCompleteBanner").gameObject;
        preCompleteBanner.SetActive(true);
        Debug.Log("Pre-complete banner activated");
        
        yield return new WaitForSeconds(3);
        
        // Hide pre-complete banner
        preCompleteBanner.SetActive(false);
        
        // Get position for flower animations
        Vector3 limitPos = GameObject.Find("Limit").transform.position;
        Debug.Log("Limit position: " + limitPos);

        yield return new WaitForSeconds(1);
        
        // Play map music
        PlayMapMusic();

        // Determine flower count based on level type
        int countFlowers = levelManager.limitType == LIMIT.MOVES ? Mathf.Clamp(levelManager.Limit, 0, 8) : 3;
        List<Item> items = levelManager.GetRandomItems(levelManager.limitType == LIMIT.MOVES ? Mathf.Clamp(levelManager.Limit, 0, 8) : 3);
        Debug.Log("Flower count: " + countFlowers);
        
        // Create flower animations
        for (int i = 1; i <= countFlowers; i++)
        {
            Debug.Log("Creating flower " + i);
            if (levelManager.limitType == LIMIT.MOVES)
                levelManager.Limit--;
                
            GameObject flowerParticle = levelManager.GetFlowerFromPool();
        
            if (flowerParticle != null)
            {
                flowerParticle.GetComponent<SpriteRenderer>().sortingLayerName = "UI";
                flowerParticle.GetComponent<Flower>().StartFly(limitPos, true);
                Debug.Log("Flower started flying");
            }
            else
            {
                Debug.LogError("Failed to get flower from pool");
            }

            yield return new WaitForSeconds(0.5f);
        }
        
        // Set limit to 0 after animations
        levelManager.Limit = 0;

        // Wait for all flowers to finish flying
        Debug.Log("Waiting for flowers to complete flight");
        while (levelManager.CheckFlowerStillFly())
        {
            Debug.Log("Flowers still flying...");
            yield return new WaitForSeconds(0.3f);
        }
        Debug.Log("All flowers completed flight");

        // Destroy any remaining extra items
        while (levelManager.GetAllExtraItems().Count > 0)
        {
            Item item = levelManager.GetAllExtraItems()[0];
            item.DestroyItem(false, "", false, true);
            levelManager.DragBlocked = true;
        
            yield return new WaitForSeconds(0.1f);
        
            // Use the shared board mechanics service
            levelManager.ProcessMatchesAndFalling();
        
            yield return new WaitForSeconds(1f);
        
            // Add timeout for safety
            float timeout = Time.time + 10f;
            while (levelManager.DragBlocked && Time.time < timeout)
            {
                yield return new WaitForFixedUpdate();
            }
        
            if (Time.time >= timeout)
            {
                Debug.LogWarning("PreWinAnimations: DragBlocked stuck - forcing to false");
                levelManager.DragBlocked = false;
            }
        }

        yield return new WaitForSeconds(1f);
        
        while (levelManager.DragBlocked)
            yield return new WaitForSeconds(0.2f);

        // Save level progress and stars
        SaveLevelProgress();
        Debug.Log("Level progress saved");

        // Show map temporarily to update it
        levelManager.LevelsMap.SetActive(false);
        levelManager.LevelsMap.SetActive(true);

        // Transition to Win state
        Debug.Log("Transitioning to Win state");
        levelManager.gameStatus = GameState.Win;
    }


    private void PlayMapMusic()
    {
        var audioSource = MusicBase.Instance.GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = MusicBase.Instance.music[0];
        audioSource.Play();
    }


    private void SaveLevelProgress()
    {
        // Save stars if current stars are better than saved stars
        int currentLevel = levelManager.currentLevel;
        int stars = levelManager.stars;

        Debug.Log($"Current level: {currentLevel}, Stars: {stars}");

        if (PlayerPrefs.GetInt(string.Format("Level.{0:000}.StarsCount", currentLevel), 0) < stars)
        {
            PlayerPrefs.SetInt(string.Format("Level.{0:000}.StarsCount", currentLevel), stars);
            Debug.Log($"Stars saved: {stars} for level {currentLevel}");
        }

        // Save high score if current score is better
        if (LevelManager.Score > PlayerPrefs.GetInt("Score" + currentLevel))
        {
            PlayerPrefs.SetInt("Score" + currentLevel, LevelManager.Score);
        }
    }
}