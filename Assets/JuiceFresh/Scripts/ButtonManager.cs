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
        public Button upgradeButton; // Кнопка апгрейда, которая должна включаться после основного действия
    }

    [System.Serializable]
    public class UpgradeAction
    {
        public Button upgradeButton;
        public int starsToSpend;
        [HideInInspector]
        public string unlockID; // Уникальный ID для сохранения состояния разблокировки
        public GameObject[] objectsToUnlock; // Массив объектов для случайного разблокирования
        [HideInInspector]
        public string objectsStateID; // ID для сохранения состояния объектов
    }

    public StarAction[] starActions;
    public UpgradeAction[] upgradeActions;
    public StarDisplay starDisplay; // Ссылка на StarDisplay для обновления UI

    private int totalSpentStars;

    void Start()
    {
        totalSpentStars = PlayerPrefs.GetInt("TotalSpentStars", 0); // Загружаем количество потраченных звезд

        // Инициализация состояния основных кнопок и апгрейдов
        InitializeStarActions();
        InitializeUpgradeActions();

        UpdateStarsDisplay(); // Обновляем UI при старте
    }

    private void InitializeStarActions()
    {
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
                DisableButton(action.targetButton); // Полностью отключаем кнопку

                // Включаем кнопку апгрейда, если она есть
                if (action.upgradeButton != null)
                {
                    action.upgradeButton.gameObject.SetActive(true);
                }
            }

            if (action.targetButton != null)
            {
                action.targetButton.onClick.AddListener(() => HandleButtonClick(action));
            }
        }
    }

    private void InitializeUpgradeActions()
    {
        foreach (var action in upgradeActions)
        {
            // Автозаполнение unlockID на основе имени кнопки или объекта
            if (string.IsNullOrEmpty(action.unlockID))
            {
                action.unlockID = action.upgradeButton.name + "_upgrade_unlocked";
            }

            // Генерация уникального ID для хранения состояния объектов апгрейда
            if (string.IsNullOrEmpty(action.objectsStateID))
            {
                action.objectsStateID = action.upgradeButton.name + "_objects_state";
            }

            // Проверяем, было ли уже разблокировано
            bool isUpgradeUnlocked = PlayerPrefs.GetInt(action.unlockID, 0) == 1;

            if (isUpgradeUnlocked)
            {
                EnableAllObjects(action);
                DisableButton(action.upgradeButton); // Полностью отключаем кнопку
            }
            else
            {
                LoadObjectsState(action);
            }

            if (action.upgradeButton != null)
            {
                action.upgradeButton.onClick.AddListener(() => HandleUpgradeButtonClick(action));
            }
        }
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

            // Полностью отключаем кнопку после покупки
            DisableButton(action.targetButton);

            // Включаем кнопку апгрейда, если она есть
            if (action.upgradeButton != null)
            {
                action.upgradeButton.gameObject.SetActive(true);
            }

            // Обновляем UI после траты звёзд
            UpdateStarsDisplay();
            Debug.Log($"Spent {action.starsToSpend} stars. Total stars now: {totalStars}");
        }
        else
        {
            Debug.Log("Not enough stars to unlock the object.");
        }
    }

    private void HandleUpgradeButtonClick(UpgradeAction action)
    {
        int totalStars = PlayerPrefs.GetInt("TotalStars", 0);

        if (totalStars >= action.starsToSpend)
        {
            totalStars -= action.starsToSpend;
            totalSpentStars += action.starsToSpend;

            // Сохраняем новое значение в PlayerPrefs
            PlayerPrefs.SetInt("TotalStars", totalStars);
            PlayerPrefs.SetInt("TotalSpentStars", totalSpentStars);

            // Ищем неактивный объект и активируем его случайным образом
            GameObject[] inactiveObjects = System.Array.FindAll(action.objectsToUnlock, obj => !obj.activeSelf);
            if (inactiveObjects.Length > 0)
            {
                GameObject objectToUnlock = inactiveObjects[Random.Range(0, inactiveObjects.Length)];
                objectToUnlock.SetActive(true);

                SaveObjectsState(action);

                // Если все объекты активированы, сохраняем состояние и отключаем кнопку
                if (System.Array.FindAll(action.objectsToUnlock, obj => !obj.activeSelf).Length == 0)
                {
                    PlayerPrefs.SetInt(action.unlockID, 1);
                    DisableButton(action.upgradeButton);
                }
            }

            PlayerPrefs.Save();

            // Обновляем UI после траты звёзд
            UpdateStarsDisplay();
            Debug.Log($"Spent {action.starsToSpend} stars. Total stars now: {totalStars}");
        }
        else
        {
            Debug.Log("Not enough stars to unlock an upgrade.");
        }
    }

    private void SaveObjectsState(UpgradeAction action)
    {
        string state = "";
        foreach (var obj in action.objectsToUnlock)
        {
            state += obj.activeSelf ? "1" : "0";
        }
        PlayerPrefs.SetString(action.objectsStateID, state);
    }

    private void LoadObjectsState(UpgradeAction action)
    {
        string state = PlayerPrefs.GetString(action.objectsStateID, new string('0', action.objectsToUnlock.Length));
        for (int i = 0; i < action.objectsToUnlock.Length; i++)
        {
            action.objectsToUnlock[i].SetActive(state[i] == '1');
        }
    }

    private void EnableAllObjects(UpgradeAction action)
    {
        foreach (var obj in action.objectsToUnlock)
        {
            obj.SetActive(true);
        }
    }

    private void DisableButton(Button button)
    {
        button.interactable = false; // Отключаем взаимодействие
        button.gameObject.SetActive(false); // Отключаем объект кнопки
    }

    private void UpdateStarsDisplay()
    {
        if (starDisplay != null)
        {
            starDisplay.UpdateStarsDisplay(); // Обновляем UI через StarDisplay
        }
    }
}
