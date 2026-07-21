using Cysharp.Threading.Tasks;
using Interfaces;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class InAppView : MonoBehaviour, IPreload
    {
        private const string ShopScrollViewportName = "ShopScrollViewport";
        private const string ShopProductsContentName = "Shop Products Content";
        private const string ShopCloseButtonName = "Close Button";

        [SerializeField] private Button closeButton;
        [SerializeField] private Button noAdsButton;
        [SerializeField] private Button gemsButton;
        [SerializeField] private Button coinsButton;
        [SerializeField] private Transform buttonsParent;
        [SerializeField] private bool animatePanelTransitions = true;
        [SerializeField, Min(0f)] private float panelOpenDuration = 0.28f;
        [SerializeField, Min(0f)] private float panelCloseDuration = 0.18f;
        [SerializeField] private AnimationCurve panelOpenCurve = CurvedUIPanelAnimator.CreateDefaultOpenCurve();
        [SerializeField] private AnimationCurve panelCloseCurve = CurvedUIPanelAnimator.CreateDefaultCloseCurve();
        [SerializeField] private AnimationCurve panelFadeCurve = CurvedUIPanelAnimator.CreateDefaultFadeCurve();

        public Transform ButtonsParent => buttonsParent;
        public Button NoAdsButton => noAdsButton;
        public Button GemsButton => gemsButton;
        public Button CoinsButton => coinsButton;

        public void Show()
        {
            CurvedUIPanelAnimator.Show(
                gameObject,
                animatePanelTransitions ? panelOpenDuration : 0f,
                panelOpenCurve,
                panelFadeCurve);
        }

        public void Hide()
        {
            CurvedUIPanelAnimator.Hide(
                gameObject,
                animatePanelTransitions ? panelCloseDuration : 0f,
                panelCloseCurve,
                panelFadeCurve);
        }

        public async UniTask Warmup()
        {
            HideLegacyShopChildren();
            EnsureCloseButton();
            EnsureScrollableProductsGrid();

            if(closeButton != null)
            {
                closeButton.onClick.RemoveListener(Hide);
                closeButton.onClick.AddListener(Hide);
            }

            await UniTask.Yield();
        }

        private void EnsureScrollableProductsGrid()
        {
            if(buttonsParent == null)
            {
                buttonsParent = CreateProductsContent();
            }

            if(buttonsParent == null)
            {
                return;
            }

            var content = buttonsParent as RectTransform;
            if(content == null)
            {
                return;
            }

            ConfigureProductsGrid(content);

            if(content.GetComponent<ContentSizeFitter>() == null)
            {
                var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if(content.parent != null && content.parent.GetComponent<ScrollRect>() != null)
            {
                return;
            }

            var originalParent = content.parent;
            if(originalParent == null)
            {
                return;
            }

            var viewportObject = new GameObject(ShopScrollViewportName, typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            var viewport = viewportObject.GetComponent<RectTransform>();
            var siblingIndex = content.GetSiblingIndex();

            viewport.SetParent(originalParent, false);
            viewport.SetSiblingIndex(siblingIndex);
            viewport.anchorMin = content.anchorMin;
            viewport.anchorMax = content.anchorMax;
            viewport.anchoredPosition = content.anchoredPosition;
            viewport.sizeDelta = content.sizeDelta;
            viewport.pivot = content.pivot;

            var viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = Color.white;

            var mask = viewportObject.GetComponent<Mask>();
            mask.showMaskGraphic = false;

            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);

            var scrollRect = viewportObject.GetComponent<ScrollRect>();
            scrollRect.content = content;
            scrollRect.viewport = viewport;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 25f;
        }

        private RectTransform CreateProductsContent()
        {
            var contentObject = new GameObject(ShopProductsContentName, typeof(RectTransform));
            var content = contentObject.GetComponent<RectTransform>();

            content.SetParent(transform, false);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(1f, 1f);
            content.offsetMin = new Vector2(46f, 185f);
            content.offsetMax = new Vector2(-46f, -115f);
            content.pivot = new Vector2(0.5f, 0.5f);

            return content;
        }

        private void EnsureCloseButton()
        {
            if(closeButton != null && closeButton.gameObject.activeSelf)
            {
                return;
            }

            closeButton = null;

            var buttonObject = new GameObject(ShopCloseButtonName, typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonTransform = buttonObject.GetComponent<RectTransform>();
            buttonTransform.SetParent(transform, false);
            buttonTransform.anchorMin = new Vector2(1f, 1f);
            buttonTransform.anchorMax = new Vector2(1f, 1f);
            buttonTransform.pivot = new Vector2(1f, 1f);
            buttonTransform.anchoredPosition = new Vector2(-45f, -55f);
            buttonTransform.sizeDelta = new Vector2(76f, 76f);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.1f, 0.34f, 0.92f);

            closeButton = buttonObject.GetComponent<Button>();

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            var labelTransform = labelObject.GetComponent<RectTransform>();
            labelTransform.SetParent(buttonTransform, false);
            labelTransform.anchorMin = Vector2.zero;
            labelTransform.anchorMax = Vector2.one;
            labelTransform.offsetMin = Vector2.zero;
            labelTransform.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = "X";
            label.fontSize = 52f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(1f, 0.86f, 0.43f, 1f);
            label.raycastTarget = false;
        }

        private void HideLegacyShopChildren()
        {
            for(int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);

                if(child.name == ShopProductsContentName ||
                   child.name == ShopScrollViewportName ||
                   child.name == ShopCloseButtonName)
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private static void ConfigureProductsGrid(RectTransform content)
        {
            var grid = content.GetComponent<GridLayoutGroup>();
            if(grid == null)
            {
                grid = content.gameObject.AddComponent<GridLayoutGroup>();
            }

            grid.padding = new RectOffset(24, 24, 26, 120);
            grid.cellSize = new Vector2(292f, 176f);
            grid.spacing = new Vector2(18f, 18f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;
        }
    }
}
