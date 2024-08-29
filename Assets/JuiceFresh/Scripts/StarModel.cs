using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarModel : MonoBehaviour
{
    public static StarModel instance;

    public Text totalStarsText; // Текстовый элемент для отображения общего количества звёзд
    public int _totalStars; // Переменная для хранения общего количества звёзд
    private Dictionary<int, int> levelStars = new Dictionary<int, int>(); // Хранит количество звёзд для каждого пройденного уровня

    private void Awake()
    {
        // Инициализация Singleton
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        int spentStars = PlayerPrefs.GetInt("TotalSpentStars", 0); // Загружаем количество потраченных звезд
        _totalStars = PlayerPrefs.GetInt("TotalStars", 0) - spentStars; // Вычитаем потраченные звезды
        _totalStars = Mathf.Max(0, _totalStars); // Убедимся, что не уйдем в отрицательные значения
        UpdateStarsDisplay(); // Обновление текста при старте
    }

    // Метод для добавления звёзд после завершения уровня
    public void AddStarsFromLevel(int levelNumber, int starsCount)
    {
        // Проверяем, был ли уровень уже пройден
        if (levelStars.ContainsKey(levelNumber))
        {
            int existingStars = levelStars[levelNumber];
            if (starsCount > existingStars)
            {
                int additionalStars = starsCount - existingStars;
                _totalStars += additionalStars;
                levelStars[levelNumber] = starsCount;
            }
        }
        else
        {
            _totalStars += starsCount;
            levelStars[levelNumber] = starsCount;
        }

        SaveTotalStars();
        UpdateStarsDisplay();
    }

    private void UpdateStarsDisplay()
    {
        if (totalStarsText != null)
        {
            totalStarsText.text = _totalStars.ToString();
        }
    }

    // Метод для получения общего количества звёзд
    public int GetTotalStars()
    {
        return _totalStars;
    }

    // Метод для сброса звёзд (при необходимости)
    public void ResetStars()
    {
        _totalStars = 0;
        levelStars.Clear();
        UpdateStarsDisplay();
    }

    public void SaveTotalStars()
    {
        PlayerPrefs.SetInt("TotalStars", _totalStars);
        PlayerPrefs.Save();
        Debug.Log($"Total stars saved: {_totalStars}");
    }
}
