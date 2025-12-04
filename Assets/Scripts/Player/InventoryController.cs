using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Serializable]
    public enum InventoryType
    {
        Player,
        Stored,
        Shop,
    }

    [SerializeField]
    private InventoryType _inventoryType;

    [SerializeField]
    private List<ItemDefinition> _defaultInventory;

    private List<ItemDefinition> _inventory;

    public List<ItemDefinition> Inventory => _inventory;

    public InventoryType Type => _inventoryType;

    private void Awake()
    {
        _inventory = new List<ItemDefinition>(_defaultInventory);
    }
}
