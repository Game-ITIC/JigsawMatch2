using System.Collections.Generic;
using UnityEngine;

public class FloatingIslandTouchRotate : MonoBehaviour
{
    [Header("Floating")]
    public float floatHeight = 0.5f;
    public float floatW = 0f;
    public float floatSpeed = 1.0f;

    [Header("Rotation Swing")]
    public float xRotationAmount = 2f;
    public float yRotationAmount = 4f;
    public float zRotationAmount = 2f;
    public float rotationSpeed = 0.7f;

    [Header("Touch Y Rotation")]
    public bool enableTouchRotation = true;
    public float touchRotationSpeed = 0.25f;
    public float smoothSpeed = 12f;

    [Header("Y Rotation Limits")]
    public float minYAngle = -60f;
    public float maxYAngle = 60f;

    [Header("Camera Zoom")]
    public bool enableCameraZoom = true;

    [Tooltip("Camera that should zoom. If empty, Camera.main will be used.")]
    public Transform zoomCamera;

    [Tooltip("Mouse wheel zoom speed.")]
    public float mouseZoomSpeed = 0.02f;

    [Tooltip("Touch pinch zoom speed.")]
    public float pinchZoomSpeed = 0.01f;

    [Tooltip("Smooth speed for camera zoom.")]
    public float zoomSmoothSpeed = 12f;

    [Tooltip("Minimum zoom offset along camera local Z axis.")]
    public float minZoom = -5f;

    [Tooltip("Maximum zoom offset along camera local Z axis.")]
    public float maxZoom = 5f;

    [Header("Options")]
    public bool invertDirection = false;
    public bool invertZoom = false;

    [Header("Physical Pivot Swing Objects")]
    public List<PhysicalPivotSwing> pivotSwingObjects = new List<PhysicalPivotSwing>();

    [System.Serializable]
    public class PhysicalPivotSwing
    {
        [Header("Object")]
        public Transform pivot;

        [Header("Local Swing Axis")]
        public Axis localAxis = Axis.Z;

        [Header("Physical Response")]
        public float velocityInfluence = 0.015f;
        public float accelerationInfluence = 0.0008f;

        [Header("Spring Physics")]
        public float springStrength = 45f;
        public float damping = 9f;
        public float maxAngle = 35f;
        public float maxAngularVelocity = 250f;

        [Header("Options")]
        public bool invertSwing = false;

        private Quaternion startLocalRotation;
        private float angle;
        private float angularVelocity;

        public enum Axis
        {
            X,
            Y,
            Z
        }

        public void Init()
        {
            if (pivot == null)
                return;

            startLocalRotation = pivot.localRotation;
            angle = 0f;
            angularVelocity = 0f;
        }

        public void UpdateSwing(float islandAngularVelocity, float islandAngularAcceleration, float deltaTime)
        {
            if (pivot == null || deltaTime <= 0f)
                return;

            float direction = invertSwing ? -1f : 1f;

            float inertiaForce =
                (-islandAngularVelocity * velocityInfluence -
                 islandAngularAcceleration * accelerationInfluence) * direction;

            float springForce = -angle * springStrength;
            float dampingForce = -angularVelocity * damping;

            float finalForce = inertiaForce + springForce + dampingForce;

            angularVelocity += finalForce * deltaTime;
            angularVelocity = Mathf.Clamp(angularVelocity, -maxAngularVelocity, maxAngularVelocity);

            angle += angularVelocity * deltaTime;
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);

            Vector3 axis = GetAxisVector();
            pivot.localRotation = startLocalRotation * Quaternion.AngleAxis(angle, axis);
        }

        private Vector3 GetAxisVector()
        {
            switch (localAxis)
            {
                case Axis.X:
                    return Vector3.right;

                case Axis.Y:
                    return Vector3.up;

                case Axis.Z:
                    return Vector3.forward;
            }

            return Vector3.forward;
        }
    }

    private Vector3 startPosition;
    private Quaternion startRotation;

    private Vector3 startCameraWorldPosition;
    private Vector3 startCameraForward;

    private float targetZoom;
    private float currentZoom;

    private float targetTouchY;
    private float currentTouchY;

    private float previousFinalY;
    private float previousAngularVelocity;

    private float islandAngularVelocity;
    private float islandAngularAcceleration;

    private Vector2 lastPointerPosition;
    private bool isDragging;

    private float lastPinchDistance;
    private bool isPinching;

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;

        if (zoomCamera == null && Camera.main != null)
            zoomCamera = Camera.main.transform;

        if (zoomCamera != null)
        {
            startCameraWorldPosition = zoomCamera.position;

            // This is the camera's own local Z direction in world space
            startCameraForward = zoomCamera.forward;
        }

        targetZoom = 0f;
        currentZoom = 0f;

        targetTouchY = 0f;
        currentTouchY = 0f;

        previousFinalY = 0f;
        previousAngularVelocity = 0f;

        for (int i = 0; i < pivotSwingObjects.Count; i++)
        {
            pivotSwingObjects[i].Init();
        }
    }

    private void Update()
    {
        if (enableCameraZoom)
            HandleCameraZoomInput();

        if (enableTouchRotation)
            HandlePointerInput();

        UpdateSmoothTouchRotation();
        UpdateCameraZoom();
        UpdateFloatingAnimation();
    }

    private void LateUpdate()
    {
        UpdatePhysicalPivotSwing();
    }

    private void HandlePointerInput()
    {
        // Prevent island rotation while 2-finger zooming
        if (IsTwoFingerTouchActive())
        {
            isDragging = false;
            return;
        }

        Vector2 pointerPosition = Vector2.zero;
        bool pointerPressed = false;
        bool pointerDownThisFrame = false;
        bool pointerUpThisFrame = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            pointerPosition = touch.position;
            pointerPressed = IsTouchActive(touch);
            pointerDownThisFrame = touch.phase == TouchPhase.Began;
            pointerUpThisFrame = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }
        else
        {
            pointerPosition = Input.mousePosition;
            pointerPressed = Input.GetMouseButton(0);
            pointerDownThisFrame = Input.GetMouseButtonDown(0);
            pointerUpThisFrame = Input.GetMouseButtonUp(0);
        }

        if (pointerDownThisFrame)
        {
            isDragging = true;
            lastPointerPosition = pointerPosition;
        }

        if (pointerPressed && isDragging)
        {
            Vector2 delta = pointerPosition - lastPointerPosition;
            RotateFromDelta(delta.x);
            lastPointerPosition = pointerPosition;
        }

        if (pointerUpThisFrame)
        {
            isDragging = false;
        }
    }

    private void HandleCameraZoomInput()
    {
        if (zoomCamera == null)
            return;

        HandleMouseWheelZoom();
        HandlePinchZoom();

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
    }

    private void HandleMouseWheelZoom()
    {
        float scrollY = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scrollY) < 0.01f)
            return;

        float direction = invertZoom ? -1f : 1f;

        targetZoom += scrollY * mouseZoomSpeed * direction;
    }

    private void HandlePinchZoom()
    {
        if (!IsTwoFingerTouchActive())
        {
            isPinching = false;
            return;
        }

        Vector2 touch0 = Input.GetTouch(0).position;
        Vector2 touch1 = Input.GetTouch(1).position;

        float currentPinchDistance = Vector2.Distance(touch0, touch1);

        if (!isPinching)
        {
            isPinching = true;
            lastPinchDistance = currentPinchDistance;
            return;
        }

        float pinchDelta = currentPinchDistance - lastPinchDistance;
        lastPinchDistance = currentPinchDistance;

        float direction = invertZoom ? -1f : 1f;

        targetZoom += pinchDelta * pinchZoomSpeed * direction;
    }

    private bool IsTwoFingerTouchActive()
    {
        if (Input.touchCount < 2)
            return false;

        return IsTouchActive(Input.GetTouch(0)) && IsTouchActive(Input.GetTouch(1));
    }

    private bool IsTouchActive(Touch touch)
    {
        return touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
    }

    private void UpdateCameraZoom()
    {
        if (zoomCamera == null)
            return;

        currentZoom = Mathf.Lerp(
            currentZoom,
            targetZoom,
            Time.deltaTime * zoomSmoothSpeed
        );

        // Move along camera's own local Z axis, not global Z
        zoomCamera.position = startCameraWorldPosition + startCameraForward * currentZoom;
    }

    private void RotateFromDelta(float deltaX)
    {
        float direction = invertDirection ? -1f : 1f;

        targetTouchY += deltaX * touchRotationSpeed * direction;
        targetTouchY = Mathf.Clamp(targetTouchY, minYAngle, maxYAngle);
    }

    private void UpdateSmoothTouchRotation()
    {
        currentTouchY = Mathf.Lerp(
            currentTouchY,
            targetTouchY,
            Time.deltaTime * smoothSpeed
        );
    }

    private void UpdateFloatingAnimation()
    {
        float time = Time.time;

        float yOffset = Mathf.Sin(time * floatSpeed) * floatHeight;
        float xOffset = Mathf.Sin(time * floatSpeed) * floatW;

        transform.position = startPosition + new Vector3(xOffset, yOffset, 0f);

        float xRot = Mathf.Sin(time * rotationSpeed) * xRotationAmount;
        float autoYRot = Mathf.Sin(time * rotationSpeed * 0.6f) * yRotationAmount;
        float zRot = Mathf.Cos(time * rotationSpeed * 0.8f) * zRotationAmount;

        float finalY = currentTouchY + autoYRot;

        CalculateIslandAngularMotion(finalY);

        Quaternion touchRotation = Quaternion.Euler(0f, currentTouchY, 0f);
        Quaternion swingRotation = Quaternion.Euler(xRot, autoYRot, zRot);

        transform.rotation = startRotation * touchRotation * swingRotation;
    }

    private void CalculateIslandAngularMotion(float finalY)
    {
        if (Time.deltaTime <= 0f)
            return;

        islandAngularVelocity = Mathf.DeltaAngle(previousFinalY, finalY) / Time.deltaTime;
        islandAngularAcceleration = (islandAngularVelocity - previousAngularVelocity) / Time.deltaTime;

        previousFinalY = finalY;
        previousAngularVelocity = islandAngularVelocity;
    }

    private void UpdatePhysicalPivotSwing()
    {
        for (int i = 0; i < pivotSwingObjects.Count; i++)
        {
            pivotSwingObjects[i].UpdateSwing(
                islandAngularVelocity,
                islandAngularAcceleration,
                Time.deltaTime
            );
        }
    }
}
