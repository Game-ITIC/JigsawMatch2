using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuView : MonoBehaviour
{
    [SerializeField] private Button startGame;

    void Start()
    {
        startGame.onClick.AddListener(() =>
        {
            int level = PlayerPrefs.GetInt("OpenLevel", 0);
            
            InitScript.openLevel = level;
            if (InitScript.lifes > 0)
            {
                SceneManager.LoadScene("game");
            }
            
            // InitScript.Instance.OnLevelClicked(this, new LevelReachedEventArgs(1));
        });
    }
}