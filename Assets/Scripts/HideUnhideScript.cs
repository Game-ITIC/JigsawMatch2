using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HideUnhideScript : MonoBehaviour
{
    [SerializeField] private Button eyeButton;
    [SerializeField] private GameObject eyeHide, eyeUnhide;
    [SerializeField] private GameObject uiElement1, uiElement2, uiElement3;
    private bool isVisible = true;

    private void Start()
    {
        eyeButton.onClick.AddListener(OnEyeButtonClick);
    }
    private void OnEyeButtonClick()
    {
        if (isVisible == true)
        {
            uiElement1.SetActive(false);
            uiElement2.SetActive(false);
            uiElement3.SetActive(false);
            //uiElement4.SetActive(false);
            isVisible = false;
            eyeHide.SetActive(false);
            eyeUnhide.SetActive(true);
        }
        else
        {
            uiElement1.SetActive(true);
            uiElement2.SetActive(true);
            uiElement3.SetActive(true);
            //uiElement4.SetActive(true);
            isVisible = true;
            eyeHide.SetActive(true);
            eyeUnhide.SetActive(false);
        }
        
    }
}
