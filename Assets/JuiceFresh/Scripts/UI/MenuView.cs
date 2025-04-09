using System.Collections;
using System.Collections.Generic;
using Interfaces;
using Monobehaviours.Buildings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuView : MonoBehaviour, IPreload
{
    [SerializeField] private Button startGame;
    [SerializeField] private Button shopOpenButton;
    [SerializeField] private BuildingShopManager buildingShopManager;

    public Button StartGame => startGame;


    public void Warmup()
    {
        shopOpenButton.onClick.RemoveAllListeners();
        shopOpenButton.onClick.AddListener(() => { buildingShopManager.gameObject.SetActive(true); });
    }
}