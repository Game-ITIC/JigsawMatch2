using UnityEngine;

namespace UI
{
    public sealed class CurvedUIPanelAnimationState : MonoBehaviour
    {
        private bool _captured;

        public Vector3 ShownScale { get; private set; }
        public CanvasGroup CanvasGroup { get; private set; }

        public void Capture()
        {
            if(!_captured)
            {
                ShownScale = transform.localScale;
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
