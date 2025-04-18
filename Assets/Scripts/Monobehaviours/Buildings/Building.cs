using UnityEngine;

namespace Monobehaviours.Buildings
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private string buildingId;
        [SerializeField] private string buildingName;

        [HideInInspector] public bool isUnlocked = false;

        [SerializeField] private GameObject[] buildingVisual;

        public void Unlock()
        {
            isUnlocked = true;
            foreach (var obj in buildingVisual)
            {
                obj.SetActive(true);
                
            }
            
        }

        public void Lock()
        {
            isUnlocked = false;
            foreach (var obj in buildingVisual)
            {
                obj.SetActive(false);
            }
        }

        public void Spawn()
        {
            gameObject.GetComponent<SpawnEffect>().PlaySpawnAnimation();
        }
    }
}