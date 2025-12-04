using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField]
    private int _capacity = 12;

    [SerializeField]
    private List<ItemDefinition> _items = new();

    [SerializeField]
    private List<ItemDefinition> _defaultItems = new();

    public int Capacity => _capacity;

    public void EnsureCapacity(int requestedCapacity)
    {
        if (requestedCapacity > _capacity)
        {
            _capacity = requestedCapacity;
        }

        NormalizeInventory();
    }

    public ItemDefinition GetItem(int index)
    {
        return IsValidIndex(index) ? _items[index] : null;
    }

    public void SetItem(int index, ItemDefinition item)
    {
        if (!IsValidIndex(index))
            return;

        _items[index] = item;
    }

    private void Awake()
    {
        ApplyDefaultItems();
        NormalizeInventory();
    }

    private void OnValidate()
    {
        NormalizeInventory();
    }

    private void NormalizeInventory()
    {
        _capacity = Mathf.Max(_capacity, _items.Count, 1);

        if (_items == null)
        {
            _items = new List<ItemDefinition>();
        }

        if (_items.Count < _capacity)
        {
            for (int i = _items.Count; i < _capacity; i++)
            {
                _items.Add(null);
            }
        }
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < _capacity && _items != null && _items.Count > index;
    }

    private void ApplyDefaultItems()
    {
        if (_items == null)
        {
            _items = new List<ItemDefinition>();
        }

        if (_defaultItems == null || _defaultItems.Count == 0)
            return;

        _capacity = Mathf.Max(_capacity, _items.Count + _defaultItems.Count);

        foreach (var item in _defaultItems)
        {
            _items.Add(item);
        }
    }
}
