using UnityEngine;

public class PlayerCameraHighlighter : MonoBehaviour
{
    [Tooltip("Камера, которая 'наводится' на объекты. Если не задана – Camera.main.")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Маска слоёв, по которым можно наведение делать.")]
    [SerializeField] private LayerMask hittableLayers = ~0;

    [Tooltip("Максимальная дистанция луча.")]
    [SerializeField] private float maxDistance = 100f;

    private OutlineTarget _currentTarget;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void Update()
    {
        if (targetCamera == null) return;

        // Луч из центра экрана
        Ray ray = targetCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hitInfo, maxDistance, hittableLayers,
                QueryTriggerInteraction.Ignore))
        {
            var outline = hitInfo.collider.GetComponentInParent<OutlineTarget>();
            UpdateTarget(outline);
        }
        else
        {
            UpdateTarget(null);
        }
    }

    private void UpdateTarget(OutlineTarget newTarget)
    {
        if (_currentTarget == newTarget) return;

        // Снять подсветку с предыдущего
        if (_currentTarget != null)
            _currentTarget.SetHighlighted(false);

        _currentTarget = newTarget;

        // Включить подсветку на новом
        if (_currentTarget != null)
            _currentTarget.SetHighlighted(true);
    }
}
