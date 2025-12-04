using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryPanel : UIPanel
{
    private class InventoryCell
    {
        public VisualElement Element { get; init; }
        public int Index { get; init; }
        public bool IsPlayerInventory { get; init; }
    }

    private VisualElement _root;
    private VisualElement _dragGhost;

    private readonly List<InventoryCell> _playerCells = new();
    private readonly List<InventoryCell> _storedCells = new();
    private readonly Dictionary<VisualElement, InventoryCell> _cellLookup = new();

    private InventoryController _playerInventory;
    private InventoryController _storedInventory;
    private InventoryCell _dragSource;
    private InventoryItem _draggedItem;

    public void Render(InventoryController playerInventory, InventoryController storedInventory)
    {
        _playerInventory = playerInventory;
        _storedInventory = storedInventory;

        _playerInventory?.EnsureCapacity(_playerCells.Count);
        _storedInventory?.EnsureCapacity(_storedCells.Count);

        RefreshCells();
    }

    override public void Show()
    {
        _root.style.display = DisplayStyle.Flex;
    }

    override public void Hide()
    {
        CleanupDragState();
        _root.style.display = DisplayStyle.None;
    }

    override protected void RegisterUIElements()
    {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>(className: "inventory");

        var playerGrid = _uiDocument.rootVisualElement.Q<VisualElement>(className: "inventory__grid--left");
        var storedGrid = _uiDocument.rootVisualElement.Q<VisualElement>(className: "inventory__grid--right");

        _cellLookup.Clear();
        BuildCells(playerGrid, _playerCells, true);
        BuildCells(storedGrid, _storedCells, false);

        _dragGhost = new Label();
        _dragGhost.AddToClassList("inventory__drag");
        _dragGhost.style.display = DisplayStyle.None;
        _uiDocument.rootVisualElement.Add(_dragGhost);
    }

    override protected void SubcribeToUIEvents()
    {
        foreach (var cell in _playerCells.Concat(_storedCells))
        {
            cell.Element.RegisterCallback<PointerDownEvent, InventoryCell>(HandleCellPointerDown, cell);
        }

        _root.RegisterCallback<PointerMoveEvent>(HandlePointerMove);
        _root.RegisterCallback<PointerUpEvent>(HandlePointerUp);
    }

    override protected void UnsubscriveFromUIEvents()
    {
        foreach (var cell in _playerCells.Concat(_storedCells))
        {
            cell.Element.UnregisterCallback<PointerDownEvent, InventoryCell>(HandleCellPointerDown, cell);
        }

        _root.UnregisterCallback<PointerMoveEvent>(HandlePointerMove);
        _root.UnregisterCallback<PointerUpEvent>(HandlePointerUp);
    }

    override protected void SubscriveToGameEvents() { }

    override protected void UnsubscribeFromGameEvents() { }

    private void BuildCells(VisualElement grid, IList<InventoryCell> collection, bool isPlayerInventory)
    {
        collection.Clear();

        if (grid == null)
            return;

        var cells = grid.Query<VisualElement>(className: "inventory__cell").ToList();
        for (int i = 0; i < cells.Count; i++)
        {
            var cell = new InventoryCell
            {
                Element = cells[i],
                Index = i,
                IsPlayerInventory = isPlayerInventory
            };

            collection.Add(cell);
            _cellLookup[cell.Element] = cell;
        }
    }

    private void HandleCellPointerDown(PointerDownEvent evt, InventoryCell cell)
    {
        var inventory = GetInventory(cell);
        if (inventory == null)
            return;

        var item = inventory.GetItem(cell.Index);
        if (item == null)
            return;

        _dragSource = cell;
        _draggedItem = item;

        _dragGhost.style.display = DisplayStyle.Flex;
        _dragGhost.transform.position = new Vector3(evt.position.x, evt.position.y);
        _dragGhost.SetEnabled(false);
        if (_dragGhost is Label label)
        {
            label.text = item.DisplayName;
        }

        _root.CaptureMouse();
        cell.Element.AddToClassList("inventory__cell--dragging");
    }

    private void HandlePointerMove(PointerMoveEvent evt)
    {
        if (_draggedItem == null)
            return;

        _dragGhost.transform.position = new Vector3(evt.position.x, evt.position.y);
    }

    private void HandlePointerUp(PointerUpEvent evt)
    {
        if (_draggedItem == null)
            return;

        var targetCell = ResolveCellAt(evt.position);

        if (targetCell != null && _dragSource != null && targetCell.IsPlayerInventory != _dragSource.IsPlayerInventory)
        {
            MoveItemBetweenInventories(_dragSource, targetCell);
            RefreshCells();
        }

        CleanupDragState();
    }

    private InventoryCell ResolveCellAt(Vector2 position)
    {
        var pickedElement = _root.panel?.Pick(position);
        return GetCellFromElement(pickedElement);
    }

    private InventoryCell GetCellFromElement(VisualElement element)
    {
        while (element != null)
        {
            if (_cellLookup.TryGetValue(element, out var cell))
                return cell;

            element = element.parent;
        }

        return null;
    }

    private void MoveItemBetweenInventories(InventoryCell sourceCell, InventoryCell targetCell)
    {
        var sourceInventory = GetInventory(sourceCell);
        var targetInventory = GetInventory(targetCell);

        if (sourceInventory == null || targetInventory == null)
            return;

        var movingItem = sourceInventory.GetItem(sourceCell.Index);
        if (movingItem == null)
            return;

        var displacedItem = targetInventory.GetItem(targetCell.Index);

        targetInventory.SetItem(targetCell.Index, movingItem);
        sourceInventory.SetItem(sourceCell.Index, displacedItem);
    }

    private InventoryController GetInventory(InventoryCell cell)
    {
        if (cell == null)
            return null;

        return cell.IsPlayerInventory ? _playerInventory : _storedInventory;
    }

    private void RefreshCells()
    {
        UpdateCells(_playerCells, _playerInventory);
        UpdateCells(_storedCells, _storedInventory);
    }

    private void UpdateCells(IEnumerable<InventoryCell> cells, InventoryController inventory)
    {
        foreach (var cell in cells)
        {
            cell.Element.Clear();
            cell.Element.RemoveFromClassList("inventory__cell--dragging");

            if (inventory == null)
                continue;

            var item = inventory.GetItem(cell.Index);
            if (item == null)
                continue;

            var label = new Label(item.DisplayName);
            label.AddToClassList("inventory__item");
            cell.Element.Add(label);
        }
    }

    private void CleanupDragState()
    {
        if (_root.HasMouseCapture())
        {
            _root.ReleaseMouse();
        }

        _dragGhost.style.display = DisplayStyle.None;
        if (_dragSource != null)
        {
            _dragSource.Element.RemoveFromClassList("inventory__cell--dragging");
        }

        _dragSource = null;
        _draggedItem = null;
    }
}
