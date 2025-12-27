using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 8f, 6f);

    [Header("Follow Smoothing")]
    [SerializeField] private float smoothTime = 0.08f;
    private Vector3 followVel;

    [Header("Angle Offset (Inspector)")]
    [SerializeField, Range(-180f, 180f)]
    private float baseYawDegrees = 0f; // horizontal angle you want (left/right), no zoom

    [Header("Look Settings (Input Yaw Offset)")]
    [SerializeField] private float lookSensitivity = 100f;
    [SerializeField] private float maxYawOffset = 25f;
    [SerializeField] private float returnSpeed = 2f;

    [Header("LookAt")]
    [SerializeField] private float lookAtHeight = 1.5f;

    private float currentYaw;
    private float yawInput;

    private void Awake()
    {
        if (!target)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player) target = player.transform;
        }
    }

    private void Update()
    {
        HandleLookInput();
    }

    private void FixedUpdate()
    {
        if (!target) return;

        float totalYaw = baseYawDegrees + currentYaw;

        Vector3 desiredPos = target.position + Quaternion.Euler(0f, totalYaw, 0f) * offset;

        // IMPORTANT: use fixedDeltaTime to match the physics step
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPos,
            ref followVel,
            smoothTime,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );
    }

    private void LateUpdate()
    {
        if (!target) return;
        transform.LookAt(target.position + Vector3.up * lookAtHeight);
    }

    private void HandleLookInput()
    {
        yawInput = 0f;

        if (Gamepad.current != null)
            yawInput += Gamepad.current.rightStick.x.ReadValue();

        if (Mouse.current != null)
            yawInput += Mouse.current.delta.x.ReadValue() / Screen.width;

        currentYaw += yawInput * lookSensitivity * Time.deltaTime;
        currentYaw = Mathf.Clamp(currentYaw, -maxYawOffset, maxYawOffset);

        if (Mathf.Abs(yawInput) < 0.01f)
            currentYaw = Mathf.Lerp(currentYaw, 0f, returnSpeed * Time.deltaTime);
    }
}
