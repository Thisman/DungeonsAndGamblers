using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
///  онтроллер движени€ игрока через новый Input System.
/// ƒвижение происходит в плоскости XZ относительно направлени€, куда смотрит камера.
/// »спользует CharacterController дл€ коллизий.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMoveController : MonoBehaviour
{
    [Header("Input")]
    [Tooltip("InputAction (Vector2) дл€ движени€, например 'Player/Move'.")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Movement")]
    [Tooltip("—корость движени€ по земле (м/с).")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("≈сли true Ч игрок будет поворачиватьс€ лицом по направлению движени€.")]
    [SerializeField] private bool alignRotationWithMovement = true;

    [Header("Camera")]
    [Tooltip(" амера, относительно которой считаетс€ направление движени€. " +
             "≈сли не задана, будет использована Camera.main.")]
    [SerializeField] private Transform cameraTransform;

    [Header("Gravity")]
    [Tooltip("¬ключить простую гравитацию дл€ CharacterController.")]
    [SerializeField] private bool useGravity = true;

    [Tooltip("—ила гравитации (м/с^2, обычно -9.81).")]
    [SerializeField] private float gravity = -9.81f;

    [Tooltip("Ќебольшое отрицательное значение дл€ прижати€ к земле, когда контроллер на земле.")]
    [SerializeField] private float groundedGravity = -2f;

    private CharacterController _characterController;
    private float _verticalVelocity; // скорость по Y (гравитаци€, прыжок при необходимости)

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
        {
            Debug.LogWarning($"{nameof(PlayerMoveController)}: CameraTransform не задан и Camera.main не найдена. " +
                             "ƒвижение будет использовать мировые оси.");
        }
    }

    private void OnEnable()
    {
        if (moveAction != null && moveAction.action != null && !moveAction.action.enabled)
            moveAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null && moveAction.action != null && moveAction.action.enabled)
            moveAction.action.Disable();
    }

    private void Update()
    {
        if (moveAction == null || moveAction.action == null || !moveAction.action.enabled)
            return;

        Vector2 input = moveAction.action.ReadValue<Vector2>();

        // Ќаправление движени€ в плоскости XZ
        Vector3 moveDirection = Vector3.zero;
        if (input.sqrMagnitude >= 0.0001f)
        {
            moveDirection = GetMoveDirectionRelativeToCamera(input);
        }

        // √оризонтальна€ скорость
        Vector3 horizontalVelocity = moveDirection * moveSpeed;

        // √равитаци€
        if (useGravity)
        {
            if (_characterController.isGrounded)
            {
                // ѕока на земле Ч держим небольшую отрицательную скорость,
                // чтобы контроллер уверенно "прижимало" вниз.
                if (_verticalVelocity < 0f)
                    _verticalVelocity = groundedGravity;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }
        else
        {
            _verticalVelocity = 0f;
        }

        // »тогова€ скорость
        Vector3 velocity = horizontalVelocity;
        velocity.y = _verticalVelocity;

        // ƒвижение с учЄтом коллизий
        _characterController.Move(velocity * Time.deltaTime);

        // ѕоворот персонажа по направлению движени€
        if (alignRotationWithMovement && moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = targetRotation;
        }
    }

    /// <summary>
    /// ¬озвращает направление движени€ в глобальных координатах,
    /// ориентиру€сь на forward/right камеры в плоскости XZ.
    /// </summary>
    private Vector3 GetMoveDirectionRelativeToCamera(Vector2 input)
    {
        Vector3 forward;
        Vector3 right;

        if (cameraTransform != null)
        {
            forward = cameraTransform.forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;

            right = cameraTransform.right;
            right.y = 0f;
            right = right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
        }
        else
        {
            forward = Vector3.forward;
            right = Vector3.right;
        }

        Vector3 move = forward * input.y + right * input.x;
        if (move.sqrMagnitude > 1f)
            move.Normalize();

        return move;
    }
}
