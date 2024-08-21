using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarModel : MonoBehaviour
{
    public static StarModel instance;

    public Text totalStarsText; // Текстовый элемент для отображения общего количества звёзд
    private int _totalStars; // Переменная для хранения общего количества звёзд
    private Dictionary<int, int> levelStars = new Dictionary<int, int>(); // Хранит количество звёзд для каждого пройденного уровня

    private void Awake()
    {
        // Инициализация Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateStarsDisplay(); // Обновление текста при старте
    }

    // Метод для добавления звёзд после завершения уровня
    public void AddStarsFromLevel(int levelNumber, int starsCount)
    {
        // Проверяем, был ли уровень уже пройден
        if (levelStars.ContainsKey(levelNumber))
        {
            // Если уровень уже пройден, добавляем только разницу в звёздах, если новых больше
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
            // Если уровень пройден впервые, добавляем звёзды
            _totalStars += starsCount;
            levelStars[levelNumber] = starsCount;
        }

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
}
