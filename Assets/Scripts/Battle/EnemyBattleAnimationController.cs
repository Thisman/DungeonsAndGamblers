using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class EnemyBattleAnimationController : MonoBehaviour, IBattleAnimationController
{
    [SerializeField]
    private Renderer _renderer;

    [SerializeField]
    private Color _damageColor = Color.red;

    [SerializeField]
    private float _blinkDurationSeconds = 0.5f;

    [SerializeField]
    private float _blinkIntervalSeconds = 0.1f;

    [SerializeField]
    private float _attackMoveDistance = 0.5f;

    [SerializeField]
    private float _attackMoveDuration = 0.3f;

    private Color _originalColor;
    private Vector3 _initialLocalPosition;

    private void Awake()
    {
        _initialLocalPosition = transform.localPosition;

        if (_renderer == null)
        {
            _renderer = GetComponentInChildren<Renderer>();
        }

        if (_renderer != null)
        {
            _originalColor = _renderer.material.color;
        }
    }

    public async Task PlayAttackAsync()
    {
        var sequence = DOTween.Sequence();
        var forwardTarget = _initialLocalPosition + transform.forward.normalized * _attackMoveDistance;

        sequence.Append(transform.DOLocalMove(forwardTarget, _attackMoveDuration / 2f).SetEase(Ease.OutQuad));
        sequence.Append(transform.DOLocalMove(_initialLocalPosition, _attackMoveDuration / 2f).SetEase(Ease.InQuad));

        await sequence.Play().AsyncWaitForCompletion();
        transform.localPosition = _initialLocalPosition;
    }

    public async Task PlayDamageAsync()
    {
        if (_renderer == null)
        {
            return;
        }

        var interval = Mathf.Max(0.01f, _blinkIntervalSeconds);
        var loops = Mathf.Max(1, Mathf.CeilToInt(_blinkDurationSeconds / interval));
        var sequence = DOTween.Sequence();

        for (var i = 0; i < loops; i++)
        {
            sequence.Append(_renderer.material.DOColor(_damageColor, interval / 2f));
            sequence.Append(_renderer.material.DOColor(_originalColor, interval / 2f));
        }

        await sequence.Play().AsyncWaitForCompletion();
        _renderer.material.color = _originalColor;
    }
}
