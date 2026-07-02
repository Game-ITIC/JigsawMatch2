using Meta.Quests.Views;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Meta.Quests.Providers
{
    public class DailyQuestProvider : MonoBehaviour
    {
        [field: SerializeField] public DailyQuestView DailyQuestViewPrefab { get; private set; }
        [field: SerializeField] public Transform DailyQuestsParent { get; private set; }

        [SerializeField] private string questsParentObjectName = "Tasks List Content";

        private void Awake()
        {
            ResolveReferences();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif

        public void ResolveReferences()
        {
            if(DailyQuestsParent == null && !string.IsNullOrWhiteSpace(questsParentObjectName))
            {
                DailyQuestsParent = FindQuestsParentTransform();
            }
        }

        private Transform FindQuestsParentTransform()
        {
            for(var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);

                if(!scene.isLoaded)
                {
                    continue;
                }

                foreach (var root in scene.GetRootGameObjects())
                {
                    var match = FindInChildren(root.transform, questsParentObjectName);

                    if(match != null)
                    {
                        return match;
                    }
                }
            }

            return null;
        }

        private static Transform FindInChildren(Transform parent, string objectName)
        {
            if(parent.name == objectName)
            {
                return parent;
            }

            for(var i = 0; i < parent.childCount; i++)
            {
                var match = FindInChildren(parent.GetChild(i), objectName);

                if(match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}