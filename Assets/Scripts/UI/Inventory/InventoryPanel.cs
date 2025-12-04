using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryPanel : UIPanel
{
    private VisualElement _root;
    private List<VisualElement> _playerInventorySlots = new();
    private List<VisualElement> _storedInventorySlots = new();
    private List<ItemDefinition> _playerInventory;
    private List<ItemDefinition> _storedInventory;
    private ItemDefinition _draggedItem;
    private InventoryType _draggedFrom;
    private int _draggedIndex;
    private readonly Dictionary<VisualElement, (InventoryType Type, int Index)> _slotLookup = new();

    private enum InventoryType
    {
        Player,
        Stored
    }

    override public void Show()
    {
        _root.style.display = DisplayStyle.Flex;
    }

    override public void Hide()
    {
        _root.style.display = DisplayStyle.None;
    }

    public void Render(List<ItemDefinition> playerItems, List<ItemDefinition> storedItems)
    {
        _playerInventory = playerItems;
        _storedInventory = storedItems;

        EnsureInventorySize(_playerInventory, _playerInventorySlots.Count);
        EnsureInventorySize(_storedInventory, _storedInventorySlots.Count);

        RenderInventorySlots(_playerInventorySlots, _playerInventory);
        RenderInventorySlots(_storedInventorySlots, _storedInventory);
    }

    override protected void RegisterUIElements()
    {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>(className: "inventory");
        _playerInventorySlots = _root.Q<VisualElement>("player__inventory")
            .Query<VisualElement>(className: "inventory__cell").ToList();

        _storedInventorySlots = _root.Q<VisualElement>("stored__inventory")
            .Query<VisualElement>(className: "inventory__cell").ToList();

        RegisterSlots(_playerInventorySlots, InventoryType.Player);
        RegisterSlots(_storedInventorySlots, InventoryType.Stored);
    }

    override protected void SubcribeToUIEvents() {
        _root.RegisterCallback<PointerUpEvent>(HandlePointerUpEvent);
    }

    override protected void UnsubscriveFromUIEvents() {
        _root.UnregisterCallback<PointerUpEvent>(HandlePointerUpEvent);
    }

    override protected void SubscriveToGameEvents() { }

    override protected void UnsubscribeFromGameEvents() { }

    private void RenderInventorySlots(List<VisualElement> slots, List<ItemDefinition> items)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            ItemDefinition item = i < items.Count ? items[i] : null;
            slots[i].style.backgroundImage = item != null ? new StyleBackground(item.Icon) : new StyleBackground();
        }
    }

    private void RegisterSlots(List<VisualElement> slots, InventoryType type)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            int index = i;
            VisualElement slot = slots[i];
            _slotLookup[slot] = (type, index);
            slot.RegisterCallback<PointerDownEvent>(_ => StartDrag(type, index));
        }
    }

    private void HandlePointerUpEvent(PointerUpEvent evt)
    {
        if (_draggedItem == null)
        {
            return;
        }

        if (TryGetSlot(evt.target as VisualElement, out InventoryType targetType, out int targetIndex))
        {
            MoveDraggedItem(targetType, targetIndex);
            Render(_playerInventory, _storedInventory);
        }
        else
        {
            Render(_playerInventory, _storedInventory);
        }

        ResetDrag();
    }

    private void StartDrag(InventoryType from, int index)
    {
        List<ItemDefinition> inventory = GetInventory(from);
        if (inventory == null)
        {
            return;
        }

        EnsureInventorySize(inventory, GetSlotCount(from));

        if (inventory[index] == null)
        {
            return;
        }

        _draggedItem = inventory[index];
        _draggedFrom = from;
        _draggedIndex = index;
    }

    private void MoveDraggedItem(InventoryType targetType, int targetIndex)
    {
        List<ItemDefinition> sourceInventory = GetInventory(_draggedFrom);
        List<ItemDefinition> targetInventory = GetInventory(targetType);

        if (sourceInventory == null || targetInventory == null)
        {
            return;
        }

        if (_draggedFrom == targetType)
        {
            return;
        }

        EnsureInventorySize(sourceInventory, GetSlotCount(_draggedFrom));
        EnsureInventorySize(targetInventory, GetSlotCount(targetType));

        ItemDefinition displacedItem = targetInventory[targetIndex];
        targetInventory[targetIndex] = _draggedItem;
        sourceInventory[_draggedIndex] = null;

        if (displacedItem != null)
        {
            PlaceItemInPlayerInventory(displacedItem);
        }
    }

    private List<ItemDefinition> GetInventory(InventoryType type)
    {
        return type switch
        {
            InventoryType.Player => _playerInventory,
            InventoryType.Stored => _storedInventory,
            _ => null
        };
    }

    private int GetSlotCount(InventoryType type)
    {
        return type switch
        {
            InventoryType.Player => _playerInventorySlots.Count,
            InventoryType.Stored => _storedInventorySlots.Count,
            _ => 0
        };
    }

    private void PlaceItemInPlayerInventory(ItemDefinition item)
    {
        if (item == null)
        {
            return;
        }

        EnsureInventorySize(_playerInventory, _playerInventorySlots.Count);

        if (_playerInventory == null)
        {
            return;
        }

        int emptyIndex = _playerInventory.FindIndex(i => i == null);
        if (emptyIndex >= 0)
        {
            _playerInventory[emptyIndex] = item;
        }
        else
        {
            _playerInventory.Add(item);
        }
    }

    private void EnsureInventorySize(List<ItemDefinition> inventory, int desiredSize)
    {
        if (inventory == null)
        {
            return;
        }

        while (inventory.Count < desiredSize)
        {
            inventory.Add(null);
        }
    }

    private bool TryGetSlot(VisualElement target, out InventoryType type, out int index)
    {
        VisualElement current = target;

        while (current != null)
        {
            if (_slotLookup.TryGetValue(current, out var data))
            {
                type = data.Type;
                index = data.Index;
                return true;
            }

            current = current.parent;
        }

        type = default;
        index = default;
        return false;
    }

    private void ResetDrag()
    {
        _draggedItem = null;
        _draggedIndex = -1;
    }
}
