using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    [SerializeField]
    private string _displayName = "Item";

    public string DisplayName => _displayName;
}

public class InventoryController : MonoBehaviour
{
    [SerializeField]
    private int _capacity = 12;

    [SerializeField]
    private List<InventoryItem> _items = new();

    public int Capacity => _capacity;

    public void EnsureCapacity(int requestedCapacity)
    {
        if (requestedCapacity > _capacity)
        {
            _capacity = requestedCapacity;
        }

        NormalizeInventory();
    }

    public InventoryItem GetItem(int index)
    {
        return IsValidIndex(index) ? _items[index] : null;
    }

    public void SetItem(int index, InventoryItem item)
    {
        if (!IsValidIndex(index))
            return;

        _items[index] = item;
    }

    private void Awake()
    {
        NormalizeInventory();
    }

    private void OnValidate()
    {
        NormalizeInventory();
    }

    private void NormalizeInventory()
    {
        if (_capacity < 1)
        {
            _capacity = 1;
        }

        if (_items == null)
        {
            _items = new List<InventoryItem>();
        }

        if (_items.Count < _capacity)
        {
            for (int i = _items.Count; i < _capacity; i++)
            {
                _items.Add(null);
            }
        }
        else if (_items.Count > _capacity)
        {
            _items.RemoveRange(_capacity, _items.Count - _capacity);
        }
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < _capacity && _items != null && _items.Count > index;
    }
}
