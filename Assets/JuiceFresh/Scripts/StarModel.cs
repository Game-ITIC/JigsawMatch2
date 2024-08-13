using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StarModel : MonoBehaviour
{
    public Text totalStarsText; // Ссылка на текстовый элемент, который будет отображать количество звезд
    private int _totalStars; // Переменная для хранения общего количества звезд
    private HashSet<int> processedLevels = new HashSet<int>(); // Хранит номера уровней, которые уже были учтены

    public void AddStarsFromLevel(int levelNumber, int starsCount)
    {
        // Проверяем, был ли этот уровень уже обработан
        if (!processedLevels.Contains(levelNumber))
        {
            _totalStars += starsCount;
            processedLevels.Add(levelNumber); // Добавляем уровень в список обработанных
            UpdateStarsDisplay();
        }
        else
        {
            Debug.Log($"Stars for level {levelNumber} have already been added.");
        }
    }

    public void ResetStars()
    {
        _totalStars = 0;
        processedLevels.Clear(); // Очищаем список обработанных уровней
        UpdateStarsDisplay();
    }

    private void UpdateStarsDisplay()
    {
        if (totalStarsText != null)
        {
            totalStarsText.text = _totalStars.ToString();
        }
    }
}
