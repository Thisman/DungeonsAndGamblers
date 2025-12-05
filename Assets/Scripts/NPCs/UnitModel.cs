using UnityEngine;

public class UnitModel: MonoBehaviour
{
    [SerializeField]
    private int _health;

    [SerializeField]
    private int _damage;

    [SerializeField, TextArea]
    private string _description;

    private int _level;
    private int _experience;
    private int _currentHealth;

    private IBattleAnimationController _battleAnimationController;

    public int Health => _health;

    public int CurrentHealth => _currentHealth;

    public int Damage => _damage;

    public int Level => _level;

    public int Experience => _experience;

    public string Description => _description;

    private void Awake()
    {
        _currentHealth = _health;
    }

    public void SetHealth(int health)
    {
        _health = Mathf.Max(1, health);
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _health);
    }

    public void SetDamage(int damage)
    {
        _damage = Mathf.Max(1, damage);
    }

    public void ApplyDamage(int damage)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - Mathf.Max(0, damage));
    }

    public IBattleAnimationController GetBattleAnimationController()
    {
        if (_battleAnimationController == null)
        {
            _battleAnimationController = GetComponent<IBattleAnimationController>();
        }

        return _battleAnimationController;
    }
}