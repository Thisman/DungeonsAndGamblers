using System.Threading.Tasks;
using UnityEngine;

public class PlayerBattleActionController : MonoBehaviour, IBattleActionController
{
    private TaskCompletionSource<bool> _actionCompletionSource;
    private Camera _cachedCamera;

    private void Awake()
    {
        _cachedCamera = Camera.main;
    }

    public async Task ResolveAction(UnitModel target)
    {
        _actionCompletionSource?.TrySetCanceled();
        _actionCompletionSource = new TaskCompletionSource<bool>();

        await _actionCompletionSource.Task;
    }

    private void Update()
    {
        if (_actionCompletionSource == null || _actionCompletionSource.Task.IsCompleted)
        {
            return;
        }

        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        var cameraToUse = _cachedCamera != null ? _cachedCamera : Camera.main;

        if (cameraToUse == null)
        {
            _actionCompletionSource.TrySetResult(true);
            return;
        }

        var ray = cameraToUse.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hit) && hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            _actionCompletionSource.TrySetResult(true);
        }
    }

    private void OnDisable()
    {
        _actionCompletionSource?.TrySetCanceled();
    }
}
