using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Monobehaviours.Animations
{
    public class CameraTransition : MonoBehaviour
    {
        [Header("Параметры перехода")]
        [Range(0.1f, 10f)]
        public float transitionDuration = 1.5f;
    
        [Range(0.1f, 10f)]
        public float rotationDuration = 1.2f;
    
        [Range(0f, 1f)]
        public float positionOvershoot = 0.1f;
    
        [Tooltip("Расстояние от камеры до объекта после перехода")]
        public float finalDistance = 5f;
    
        [Header("Настройки Ease")]
        public Ease positionEase = Ease.InOutQuad;
        public Ease rotationEase = Ease.InOutSine;
    
        [Header("Дополнительные эффекты")]
        [Range(0f, 90f)]
        public float fieldOfViewChange = 15f;
        public bool useFieldOfViewEffect = true;
    
        [Range(0f, 1f)]
        public float shakeMagnitude = 0;
        public float shakeDuration = 0.3f;
        public bool useShakeEffect = false;
    
        private Camera mainCamera;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float initialFOV;
    
        private Sequence transitionSequence;
    
        private void Awake()
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            initialFOV = mainCamera.fieldOfView;
            SaveInitialTransform();
        }
    
        public void SaveInitialTransform()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }
    
        /// <summary>
        /// Выполняет переход камеры к целевому объекту и возвращает UniTask для ожидания завершения
        /// </summary>
        public async UniTask TransitionToTargetAsync(Transform targetTransform)
        {
            // Останавливаем текущую анимацию, если она есть
            if (transitionSequence != null && transitionSequence.IsActive())
            {
                transitionSequence.Kill();
            }
        
            // Рассчитываем конечную позицию и поворот
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
            Vector3 finalPosition = targetTransform.position - directionToTarget * finalDistance;
            Quaternion finalRotation = Quaternion.LookRotation(targetTransform.position - finalPosition);
        
            // Создаем новую последовательность
            transitionSequence = DOTween.Sequence();
        
            // Добавляем твин позиции
            transitionSequence.Append(
                transform.DOMove(finalPosition, transitionDuration)
                    .SetEase(positionEase)
                
            );
        
            // Добавляем твин вращения (параллельно с движением)
            transitionSequence.Join(
                transform.DORotateQuaternion(finalRotation, rotationDuration)
                    .SetEase(rotationEase)
            );
        
            // Эффект изменения поля зрения
            if (useFieldOfViewEffect && mainCamera != null)
            {
                float midFOV = initialFOV - fieldOfViewChange;
                float finalFOV = initialFOV;
            
                transitionSequence.Join(
                    DOTween.Sequence()
                        .Append(DOTween.To(() => mainCamera.fieldOfView, x => mainCamera.fieldOfView = x, midFOV, transitionDuration * 0.5f))
                        .Append(DOTween.To(() => mainCamera.fieldOfView, x => mainCamera.fieldOfView = x, finalFOV, transitionDuration * 0.5f))
                );
            }
        
            // Эффект дрожания камеры в конце
            if (useShakeEffect && shakeMagnitude > 0)
            {
                transitionSequence.AppendCallback(() => {
                    transform.DOShakePosition(shakeDuration, shakeMagnitude);
                });
            }
        
            // Запускаем последовательность
            transitionSequence.Play();
        
            // Ожидаем завершения анимации
            float totalDuration = Mathf.Max(transitionDuration, rotationDuration);
            if (useShakeEffect && shakeMagnitude > 0)
                totalDuration += shakeDuration;
            
            await UniTask.Delay((int)(totalDuration * 1000));
        }
    }
}