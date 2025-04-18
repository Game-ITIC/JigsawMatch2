using Interfaces;
using Monobehaviours.Buildings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Views;

public class MenuView : MonoBehaviour, IPreload
{
    [SerializeField] private Button startGame;
    [SerializeField] private TextView startGameText;
    [SerializeField] private Button shopOpenButton;
    [SerializeField] private BuildingShopManager buildingShopManager;

    public Button StartGame => startGame;
    public TextView StartGameText => startGameText;

    public void Warmup()
    {
        shopOpenButton.onClick.RemoveAllListeners();
        shopOpenButton.onClick.AddListener(() => { buildingShopManager.gameObject.SetActive(true); });
    }
}