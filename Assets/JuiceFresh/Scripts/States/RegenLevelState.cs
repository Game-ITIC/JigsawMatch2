// RegenLevelState.cs

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JuiceFresh.States;

public class RegenLevelState : GameStateBase
{
    public RegenLevelState(LevelManager levelManager) : base(levelManager)
    {
    }

    public override void EnterState()
    {
        // Hide "No More Matches" message after a delay and regenerate the level
        levelManager.StartCoroutine(RegenLevelProcess());
    }

    public override void UpdateState()
    {
        // No specific update logic for regen level state
    }

    public override void ExitState()
    {
        // Hide the "No More Matches" message if it's visible
        GameObject noMoreMatchesMessage = GameObject.Find("Level/Canvas")?.transform.Find("NoMoreMatches")?.gameObject;
        if (noMoreMatchesMessage != null && noMoreMatchesMessage.activeSelf)
        {
            noMoreMatchesMessage.SetActive(false);
        }
    }

    private IEnumerator RegenLevelProcess()
    {
        // Wait for a moment to show the "No More Matches" message
        yield return new WaitForSeconds(1);

        // Regenerate the level
        ReGenLevel();

        // Return to playing state
        levelManager.gameStatus = GameState.Playing;
    }

    private void ReGenLevel()
    {
        // Reset items hidden flag
        levelManager.itemsHided = false;

        // Block dragging during regeneration
        levelManager.DragBlocked = true;

        // Destroy existing items
        DestroyItems(true);

        // Generate new items without falling animation
        levelManager.GenerateNewItems(false);

        // Initialize bombs
        levelManager.StartCoroutine(InitBombs());

        // Unblock dragging after regeneration
        levelManager.DragBlocked = false;

        // Trigger level loaded event
        LevelManager.TriggerOnLevelLoaded();
    }

    private void DestroyItems(bool withoutEffects = true)
    {
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            if (item != null)
            {
                Item itemComponent = item.GetComponent<Item>();
                if (itemComponent != null && itemComponent.currentType != ItemsTypes.INGREDIENT)
                {
                    if (!withoutEffects)
                        itemComponent.DestroyItem();
                    else
                        itemComponent.SmoothDestroy();
                }
            }
        }
    }

    private IEnumerator InitBombs()
    {
        // Wait until tip is closed
        yield return new WaitUntil(() => !TipsManager.THIS.gotTip);
        yield return new WaitForSeconds(1);

        // Count existing bombs
        int bombsOnField = 0;
        List<Item> items = levelManager.GetItems();
        foreach (Item item in items)
        {
            if (item.currentType == ItemsTypes.BOMB)
                bombsOnField++;
        }

        // Add more bombs if needed
        int bombsToAdd = levelManager.bombsCollect - bombsOnField - levelManager.TargetBombs;
        List<Item> itemsRand = levelManager.GetRandomItems(bombsToAdd);
        int i = 0;

        foreach (Item item in itemsRand)
        {
            item.nextType = ItemsTypes.BOMB;

            if (levelManager.bombTimers.Count > 0)
                item.bombTimer = levelManager.bombTimers[i];
            i++;
            item.ChangeType();
        }
    }
}