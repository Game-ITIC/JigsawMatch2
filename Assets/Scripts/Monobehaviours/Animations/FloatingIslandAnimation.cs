using UnityEngine;

public class FloatingIslandAnimation : MonoBehaviour
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

    [Header("Space")]
    public bool useLocalSpace = true;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    private Vector3 startWorldPosition;
    private Quaternion startWorldRotation;

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;

        startWorldPosition = transform.position;
        startWorldRotation = transform.rotation;
    }

    private void Update()
    {
        float time = Time.time;

        // Floating up and down
        float yOffset = Mathf.Sin(time * floatSpeed) * floatHeight;
        float xOffset = Mathf.Sin(time * floatSpeed) * floatW;

        // Small back-and-forth rotation, not 360 spin
        float xRot = Mathf.Sin(time * rotationSpeed) * xRotationAmount;
        float yRot = Mathf.Sin(time * rotationSpeed * 0.6f) * yRotationAmount;
        float zRot = Mathf.Cos(time * rotationSpeed * 0.8f) * zRotationAmount;

        Quaternion swingRotation = Quaternion.Euler(xRot, yRot, zRot);

        if (useLocalSpace)
        {
            transform.localPosition = startLocalPosition + new Vector3(xOffset, yOffset, 0f);
            transform.localRotation = startLocalRotation * swingRotation;
        }
        else
        {
            transform.position = startWorldPosition + new Vector3(xOffset, yOffset, 0f);
            transform.rotation = startWorldRotation * swingRotation;
        }
    }
}