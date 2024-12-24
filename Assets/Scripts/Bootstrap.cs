using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class Bootstrap : MonoBehaviour
    {
        [SerializeField] private float maxWaitTime = 15f; // Максимальное время ожидания, секунд
        [SerializeField] private GameObject loadingFrame;

        private void Start()
        {
            StartCoroutine(nameof(WaitForIronSourceInit));
        }

        private IEnumerator WaitForIronSourceInit()
        {
            float timer = 0f;
            
            loadingFrame.SetActive(true);
            IronSourceManager.Instance.InitializeLevelPlay();
            
            while (timer < maxWaitTime)
            {
                if (IronSourceManager.Instance && IronSourceManager.Instance.AreAdsReady())
                {
                    Debug.Log("IronSource готов, переходим в MainMenu");
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }
            
            loadingFrame.SetActive(false);
            
            SceneManager.LoadScene(1);
        }
    }
}