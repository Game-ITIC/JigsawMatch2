using UnityEngine;

namespace Providers
{
    public class CameraProvider : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public Camera Camera => _camera;
    }
}