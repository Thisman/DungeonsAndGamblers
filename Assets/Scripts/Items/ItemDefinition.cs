using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "DungeonsAndGamblers/Item Definition")]
public class ItemDefinition: ScriptableObject
{
    [SerializeField]
    private string _name;

    [SerializeField, TextArea]
    private string _description;

    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    private int _price;

    public string Name => _name;

    public string Description => _description;

    public Sprite Icon => _icon;

    public int Price => _price;
}