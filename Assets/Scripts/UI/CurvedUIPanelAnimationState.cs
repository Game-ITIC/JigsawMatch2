using UnityEngine;

namespace UI
{
    public sealed class CurvedUIPanelAnimationState : MonoBehaviour
    {
        private bool _captured;

        public Vector3 ShownScale { get; private set; } = Vector3.one;
        public CanvasGroup CanvasGroup { get; private set; }

        public void Capture()
        {
            if(!_captured)
            {
                var currentScale = transform.localScale;
                if(currentScale.sqrMagnitude > 0.001f)
                {
                    ShownScale = currentScale;
                }
                else
                {
                    ShownScale = Vector3.one;
                }
                _captured = true;
            }

            if(CanvasGroup == null)
            {
                CanvasGroup = GetComponent<CanvasGroup>();
                if(CanvasGroup == null)
                {
                    CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
    }
}
