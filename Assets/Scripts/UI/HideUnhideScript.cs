using UnityEngine;
using UnityEngine.UI;

public class HideUnhideScript : MonoBehaviour
{
    [SerializeField] private Button eyeButton;
    [SerializeField] private GameObject eyeHide, eyeUnhide;
    [SerializeField] private GameObject uiElement1, uiElement2, uiElement3, uiElement4, uiElement5, uiElement6;
    private bool isVisible = true;

    private void Start()
    {
        if(eyeButton != null)
        {
            eyeButton.onClick.RemoveListener(OnEyeButtonClick);
            eyeButton.onClick.AddListener(OnEyeButtonClick);
        }
    }

    public void OnEyeButtonClick()
    {
        if(isVisible == true)
        {
            SetActiveIfPresent(uiElement1, false);
            SetActiveIfPresent(uiElement2, false);
            SetActiveIfPresent(uiElement3, false);
            SetActiveIfPresent(uiElement4, false);
            SetActiveIfPresent(uiElement5, false);
            SetActiveIfPresent(uiElement6, false);
            isVisible = false;
            SetActiveIfPresent(eyeHide, false);
            SetActiveIfPresent(eyeUnhide, true);
        }
        else
        {
            SetActiveIfPresent(uiElement1, true);
            SetActiveIfPresent(uiElement2, true);
            SetActiveIfPresent(uiElement3, true);
            SetActiveIfPresent(uiElement4, true);
            SetActiveIfPresent(uiElement5, true);
            SetActiveIfPresent(uiElement6, true);
            isVisible = true;
            SetActiveIfPresent(eyeHide, true);
            SetActiveIfPresent(eyeUnhide, false);
        }
    }

    private static void SetActiveIfPresent(GameObject target, bool active)
    {
        if(target != null)
        {
            target.SetActive(active);
        }
    }
}
