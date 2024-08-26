using UnityEngine;
using UnityEngine.UI;

public class StarDisplay : MonoBehaviour
{
    public Text totalStarsText; // Ссылка на текстовый элемент для отображения общего количества звёзд

    void Start()
    {
        UpdateStarsDisplay();
    }



    public void UpdateStarsDisplay()
    {
        int totalStars = PlayerPrefs.GetInt("TotalStars", 0);
        totalStarsText.text = totalStars.ToString();
        Debug.Log($"Total stars updated: {totalStars}");
    }

}
