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
    [SerializeField] private Button dailyButton;
    [SerializeField] private Button inAppButton;
    [SerializeField] private Button mapButton;
    public Button StartGame => startGame;
    public Button DailyButton => dailyButton;
    public Button InAppButton => inAppButton;
    public TextView StartGameText => startGameText;
    public Button MapButton => mapButton;

    public void Warmup()
    {
        shopOpenButton.onClick.RemoveAllListeners();
        shopOpenButton.onClick.AddListener(() => { buildingShopManager.gameObject.SetActive(true); });
    }
}