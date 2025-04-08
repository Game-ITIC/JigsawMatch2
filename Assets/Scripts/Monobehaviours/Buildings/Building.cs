using UnityEngine;

namespace Monobehaviours.Buildings
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private string buildingId;
        [SerializeField] private string buildingName;

        [HideInInspector] public bool isUnlocked = false;

        [SerializeField] private GameObject buildingVisual;

        public void Unlock()
        {
            isUnlocked = true;
            buildingVisual.SetActive(true);
        }

        public void Lock()
        {
            isUnlocked = false;
            buildingVisual.SetActive(false);
        }
    }
}