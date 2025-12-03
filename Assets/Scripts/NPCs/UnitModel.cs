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

    public int Health => _health;

    public int CurrentHealth => _currentHealth;

    public int Damage => _damage;

    public int Level => _level;

    public int Experience => _experience;

    public string Description => _description;
}