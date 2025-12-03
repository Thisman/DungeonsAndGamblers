using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Transforms")]
    [Tooltip("“рансформ, который наклон€етс€ по вертикали (обычно Camera). " +
             "≈сли пусто Ч будет использован текущий Transform.")]
    [SerializeField] private Transform pitchTransform;

    [Header("Input")]
    [Tooltip("—сылка на InputAction (Vector2) дл€ обзора, например 'Player/Look'.")]
    [SerializeField] private InputActionReference lookAction;

    [Header("Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Header("Vertical Limits")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    [SerializeField] private bool invertY = false;

    [Header("Horizontal Limits")]
    [SerializeField] private bool limitHorizontalAngle = false;
    [SerializeField] private float minHorizontalAngle = -90f;
    [SerializeField] private float maxHorizontalAngle = 90f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorOnStart = true;

    private float _yaw;
    private float _pitch;
    private float _baseYaw;

    private bool _useSeparatePitchTransform;

    private void Awake()
    {
        if (pitchTransform == null)
            pitchTransform = transform;

        _useSeparatePitchTransform = pitchTransform != transform;

        Vector3 yawEuler = transform.localEulerAngles;
        Vector3 pitchEuler = pitchTransform.localEulerAngles;

        _yaw = yawEuler.y;
        _pitch = NormalizeAngle(pitchEuler.x);
        _baseYaw = _yaw;

        if (lockCursorOnStart)
            LockCursor(true);
    }

    private void OnEnable()
    {
        if (lookAction != null && lookAction.action != null && !lookAction.action.enabled)
            lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lookAction != null && lookAction.action != null && lookAction.action.enabled)
            lookAction.action.Disable();
    }

    private void Update()
    {
        if (lookAction == null || lookAction.action == null || !lookAction.action.enabled)
            return;

        Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
        if (lookDelta.sqrMagnitude < Mathf.Epsilon)
            return;

        float deltaX = lookDelta.x * mouseSensitivity;
        float deltaY = lookDelta.y * mouseSensitivity;

        if (invertY)
            deltaY = -deltaY;

        _yaw += deltaX;
        _pitch -= deltaY;
        _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);

        if (limitHorizontalAngle)
        {
            float angleOffset = NormalizeAngle(_yaw - _baseYaw);
            angleOffset = Mathf.Clamp(angleOffset, minHorizontalAngle, maxHorizontalAngle);
            _yaw = _baseYaw + angleOffset;
        }

        ApplyRotation();
    }

    private void ApplyRotation()
    {
        if (_useSeparatePitchTransform)
        {
            // ¬ариант с пивотом (рекомендуетс€):
            // - yaw на объекте с контроллером (обычно "PlayerRig")
            // - pitch на дочерней камере
            transform.localRotation = Quaternion.Euler(0f, _yaw, 0f);
            pitchTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
        else
        {
            // ¬ариант без пивота Ч всЄ на одном Transform
            transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
    }

    private void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private static float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }
}
