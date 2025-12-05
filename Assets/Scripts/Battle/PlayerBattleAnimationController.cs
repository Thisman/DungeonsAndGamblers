using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PlayerBattleAnimationController : MonoBehaviour, IBattleAnimationController
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Color _damageColor = Color.red;

    [SerializeField]
    private float _blinkDurationSeconds = 0.5f;

    [SerializeField]
    private float _blinkIntervalSeconds = 0.1f;

    private Color _originalColor;

    private void Awake()
    {
        if (_camera == null)
        {
            _camera = Camera.main;
        }

        if (_camera != null)
        {
            _originalColor = _camera.backgroundColor;
        }
    }

    public Task PlayAttackAsync()
    {
        return Task.CompletedTask;
    }

    public async Task PlayDamageAsync()
    {
        if (_camera == null)
        {
            return;
        }

        var interval = Mathf.Max(0.01f, _blinkIntervalSeconds);
        var loops = Mathf.Max(1, Mathf.CeilToInt(_blinkDurationSeconds / interval));
        var sequence = DOTween.Sequence();

        for (var i = 0; i < loops; i++)
        {
            sequence.Append(_camera.DOColor(_damageColor, interval / 2f));
            sequence.Append(_camera.DOColor(_originalColor, interval / 2f));
        }

        await sequence.Play().AsyncWaitForCompletion();
        _camera.backgroundColor = _originalColor;
    }
}
