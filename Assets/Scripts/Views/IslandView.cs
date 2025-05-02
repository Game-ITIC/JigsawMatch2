using System;
using Configs;
using UnityEngine;

namespace Views
{
    public class IslandView : MonoBehaviour
    {
        public event Action<IslandView> OnClick;

        [SerializeField] private CountryConfig countryConfig;
        [SerializeField] private Transform targetPosition;
        public CountryConfig CountryConfig => countryConfig;

        public Transform TargetPosition => targetPosition;

        private void OnMouseDown()
        {
            
            // Debug.Log("Pressed");
            OnClick?.Invoke(this);
        }
        
        public void LocK(){}
        public void Unlock(){}
    }
}