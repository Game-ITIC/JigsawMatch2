using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [System.Serializable]
    public class StarAction
    {
        public Button targetButton;
        public int starsToSpend;
        public GameObject objectToUnlock;
        [HideInInspector]
        public string unlockID; // Уникальный ID для сохранения состояния разблокировки
    }

    public StarAction[] starActions;
    public StarDisplay starDisplay; // Ссылка на StarDisplay для обновления UI

    private int totalSpentStars;

    void Start()
    {
        totalSpentStars = PlayerPrefs.GetInt("TotalSpentStars", 0); // Загружаем количество потраченных звезд

        foreach (var action in starActions)
        {
            // Автозаполнение unlockID на основе имени кнопки или объекта
            if (string.IsNullOrEmpty(action.unlockID))
            {
                action.unlockID = action.targetButton.name + "_unlocked";
            }

            // Проверяем, было ли уже разблокировано
            bool isUnlocked = PlayerPrefs.GetInt(action.unlockID, 0) == 1;
            if (isUnlocked && action.objectToUnlock != null)
            {
                action.objectToUnlock.SetActive(true);
                action.targetButton.interactable = false; // Отключаем кнопку
            }

            if (action.targetButton != null)
            {
                action.targetButton.onClick.AddListener(() => HandleButtonClick(action));
            }
        }

        UpdateStarsDisplay(); // Обновляем UI при старте
    }

    private void HandleButtonClick(StarAction action)
    {
        int totalStars = PlayerPrefs.GetInt("TotalStars", 0);

        if (totalStars >= action.starsToSpend)
        {
            totalStars -= action.starsToSpend;
            totalSpentStars += action.starsToSpend;

            // Сохраняем новое значение в PlayerPrefs
            PlayerPrefs.SetInt("TotalStars", totalStars);
            PlayerPrefs.SetInt("TotalSpentStars", totalSpentStars);

            // Сохраняем состояние разблокировки
            PlayerPrefs.SetInt(action.unlockID, 1);

            PlayerPrefs.Save();

            if (action.objectToUnlock != null)
            {
                action.objectToUnlock.SetActive(true);
            }

            // Отключаем кнопку после покупки
            action.targetButton.interactable = false;

            // Обновляем UI после траты звёзд
            UpdateStarsDisplay();
            Debug.Log($"Spent {action.starsToSpend} stars. Total stars now: {totalStars}");
        }
        else
        {
            Debug.Log("Not enough stars to unlock the object.");
        }
    }

    private void UpdateStarsDisplay()
    {
        if (starDisplay != null)
        {
            starDisplay.UpdateStarsDisplay(); // Обновляем UI через StarDisplay
        }
    }
}
