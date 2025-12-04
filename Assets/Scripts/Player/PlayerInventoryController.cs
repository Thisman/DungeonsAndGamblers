using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class PlayerInventoryController: MonoBehaviour
{
    [SerializeField]
    private List<ItemDefinition> _defaultInventory;

    private List<ItemDefinition> _inventory;

    public List<ItemDefinition> Inventory => _inventory;

    private void Awake()
    {
        _inventory = new List<ItemDefinition>(_defaultInventory);
    }
}
