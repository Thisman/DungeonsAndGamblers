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
    private VisualElement _dragVisual;
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
        _root.style.position = Position.Relative;
        _playerInventorySlots = _root.Q<VisualElement>("player__inventory")
            .Query<VisualElement>(className: "inventory__cell").ToList();

        _storedInventorySlots = _root.Q<VisualElement>("stored__inventory")
            .Query<VisualElement>(className: "inventory__cell").ToList();

        _dragVisual = _root.Q<VisualElement>("dragged-item-view");
        _dragVisual.pickingMode = PickingMode.Ignore;

        RegisterSlots(_playerInventorySlots, InventoryType.Player);
        RegisterSlots(_storedInventorySlots, InventoryType.Stored);
    }

    override protected void SubcribeToUIEvents() {
        _root.RegisterCallback<PointerMoveEvent>(HandlePointerMoveEvent);
        _root.RegisterCallback<PointerUpEvent>(HandlePointerUpEvent);
    }

    override protected void UnsubscriveFromUIEvents() {
        _root.UnregisterCallback<PointerMoveEvent>(HandlePointerMoveEvent);
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
            slot.RegisterCallback<PointerDownEvent>(evt => StartDrag(type, index, evt));
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
        }
        else
        {
            RestoreDraggedItemToOrigin();
        }

        Render(_playerInventory, _storedInventory);

        ResetDrag();
    }

    private void HandlePointerMoveEvent(PointerMoveEvent evt)
    {
        if (_draggedItem == null)
        {
            return;
        }

        UpdateDragVisualPosition(evt.position);
    }

    private void StartDrag(InventoryType from, int index, PointerDownEvent evt)
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

        inventory[index] = null;
        Render(_playerInventory, _storedInventory);

        EnsureDragVisual();
        _dragVisual.style.backgroundImage = new StyleBackground(_draggedItem.Icon);
        _dragVisual.style.display = DisplayStyle.Flex;
        UpdateDragVisualPosition(evt.position);
        _dragVisual.BringToFront();
        evt.StopPropagation();
    }

    private void MoveDraggedItem(InventoryType targetType, int targetIndex)
    {
        var sourceInventory = GetInventory(_draggedFrom);
        var targetInventory = GetInventory(targetType);

        if (sourceInventory == null || targetInventory == null)
            return;

        EnsureInventorySize(sourceInventory, GetSlotCount(_draggedFrom));
        EnsureInventorySize(targetInventory, GetSlotCount(targetType));

        if (_draggedFrom == targetType)
        {
            if (targetIndex == _draggedIndex)
            {
                sourceInventory[_draggedIndex] = _draggedItem;
                return;
            }

            var displacedItem = targetInventory[targetIndex];
            targetInventory[targetIndex] = _draggedItem;
            sourceInventory[_draggedIndex] = displacedItem;

            return;
        }

        var displaced = targetInventory[targetIndex];
        targetInventory[targetIndex] = _draggedItem;
        sourceInventory[_draggedIndex] = null;

        if (displaced != null)
            PlaceItemInPlayerInventory(displaced);
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

    private void RestoreDraggedItemToOrigin()
    {
        List<ItemDefinition> sourceInventory = GetInventory(_draggedFrom);
        if (sourceInventory == null)
        {
            return;
        }

        EnsureInventorySize(sourceInventory, GetSlotCount(_draggedFrom));

        if (sourceInventory[_draggedIndex] == null)
        {
            sourceInventory[_draggedIndex] = _draggedItem;
        }
    }

    private void EnsureDragVisual()
    {
        if (_dragVisual != null)
        {
            return;
        }

        _dragVisual = new VisualElement();
        _dragVisual.AddToClassList("inventory__drag");
        _dragVisual.pickingMode = PickingMode.Ignore;

        _root.Add(_dragVisual);
    }

    private void UpdateDragVisualPosition(Vector2 pointerPosition)
    {
        if (_dragVisual == null || _dragVisual.style.display == DisplayStyle.None)
        {
            return;
        }

        Vector2 localPosition = _root.WorldToLocal(pointerPosition);
        float width = _dragVisual.resolvedStyle.width;
        float height = _dragVisual.resolvedStyle.height;

        if (Mathf.Approximately(width, 0f))
        {
            width = 64f;
        }

        if (Mathf.Approximately(height, 0f))
        {
            height = 64f;
        }

        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        _dragVisual.style.left = localPosition.x - halfWidth;
        _dragVisual.style.top = localPosition.y - halfHeight;
    }

    private void ResetDrag()
    {
        _draggedItem = null;
        _draggedFrom = default;
        _draggedIndex = -1;

        if (_dragVisual != null)
        {
            _dragVisual.style.display = DisplayStyle.None;
        }
    }
}
