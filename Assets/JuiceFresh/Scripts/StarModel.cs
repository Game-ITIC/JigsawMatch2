using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarModel : MonoBehaviour
{
    public static StarModel instance;

    public Text totalStarsText; // Ссылка на текстовый элемент, который будет отображать количество звезд
    private int _totalStars; // Переменная для хранения общего количества звезд
    private HashSet<int> processedLevels = new HashSet<int>(); // Хранит номера уровней, которые уже были учтены

    private void Start()
    {
        instance = this;
        UpdateStarsDisplay(); // Обновление текста при старте
    }

    // Метод для добавления звёзд с уровня
    public void AddStarsFromLevel(int levelNumber, int starsCount)
    {
        // Проверяем, был ли этот уровень уже обработан
        if (!processedLevels.Contains(levelNumber))
        {
            _totalStars += starsCount;
            processedLevels.Add(levelNumber);
            UpdateStarsDisplay();
        }
        else
        {
            Debug.Log($"Stars for level {levelNumber} have already been added.");
        }
    }

    // Метод для получения общего количества звёзд
    public int GetTotalStars()
    {
        return _totalStars;
    }

    // Метод для установки и отображения конкретного значения звёзд
    public void SetTotalStars(int totalStars)
    {
        _totalStars = totalStars;
        UpdateStarsDisplay();
    }

    public void UpdateStarsDisplay()
    {
        if (totalStarsText != null)
        {
            totalStarsText.text = _totalStars.ToString();
        }
    }
}
