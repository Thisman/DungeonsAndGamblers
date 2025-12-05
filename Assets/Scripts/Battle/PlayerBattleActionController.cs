using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

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

        var mouse = Mouse.current;

        if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
        {
            return;
        }

        var cameraToUse = _cachedCamera != null ? _cachedCamera : Camera.main;

        if (cameraToUse == null)
        {
            _actionCompletionSource.TrySetResult(true);
            return;
        }

        var ray = cameraToUse.ScreenPointToRay(mouse.position.ReadValue());

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
