using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopPanel : UIPanel
{
    private VisualElement _root;
    private VisualElement _grid;
    private Button _sellTab;
    private Button _buyTab;
    private VisualElement _itemInfo;
    private VisualElement _itemIcon;
    private Label _itemName;
    private Label _itemDescription;
    private Label _itemPrice;
    private Button _actionButton;
    private List<VisualElement> _slots = new();

    private List<ItemDefinition> _playerStoredInventory;
    private List<ItemDefinition> _shopInventory;
    private PlayerResourcesController _playerResourcesController;

    private InventoryController.InventoryType _activeInventory = InventoryController.InventoryType.Stored;
    private int _selectedItemIndex = -1;

    override public void Show()
    {
        if (_root == null)
        {
            return;
        }

        _root.style.display = DisplayStyle.Flex;
        SetActiveTab(_activeInventory);
        RenderActiveInventory();
    }

    override public void Hide()
    {
        if (_root == null)
        {
            return;
        }

        _root.style.display = DisplayStyle.None;
        ClearItemInfo();
    }

    public void Render(List<ItemDefinition> storedInventory, List<ItemDefinition> shopInventory)
    {
        _playerStoredInventory = storedInventory;
        _shopInventory = shopInventory;

        _activeInventory = InventoryController.InventoryType.Stored;
        SetActiveTab(_activeInventory);

        RenderActiveInventory();
    }

    public void SetPlayerResourcesController(PlayerResourcesController playerResourcesController)
    {
        _playerResourcesController = playerResourcesController;
    }

    override protected void RegisterUIElements()
    {
        _root = _uiDocument.rootVisualElement.Q<VisualElement>(className: "shop");
        _grid = _root.Q<VisualElement>("shop-grid");
        _slots = _grid?.Query<VisualElement>(className: "shop__cell").ToList() ?? new List<VisualElement>();

        _sellTab = _root.Q<Button>("shop-tab__sell");
        _buyTab = _root.Q<Button>("shop-tab__buy");

        _itemInfo = _root.Q<VisualElement>("shop-item-info");
        _itemIcon = _itemInfo.Q<VisualElement>("shop-item-info__icon");
        _itemName = _itemInfo.Q<Label>("shop-item-info__name");
        _itemDescription = _itemInfo.Q<Label>("shop-item-info__description");
        _itemPrice = _itemInfo.Q<Label>("shop-item-info__price");
        _actionButton = _itemInfo.Q<Button>("shop-item-info__action");

        RegisterSlots();
        ClearItemInfo();
    }

    override protected void SubcribeToUIEvents()
    {
        if (_sellTab != null)
        {
            _sellTab.clicked += HandleSellTabClicked;
        }

        if (_buyTab != null)
        {
            _buyTab.clicked += HandleBuyTabClicked;
        }

        if (_actionButton != null)
        {
            _actionButton.clicked += HandleActionButtonClicked;
        }
    }

    override protected void UnsubscriveFromUIEvents()
    {
        if (_sellTab != null)
        {
            _sellTab.clicked -= HandleSellTabClicked;
        }

        if (_buyTab != null)
        {
            _buyTab.clicked -= HandleBuyTabClicked;
        }

        if (_actionButton != null)
        {
            _actionButton.clicked -= HandleActionButtonClicked;
        }
    }

    override protected void SubscriveToGameEvents() { }

    override protected void UnsubscribeFromGameEvents() { }

    private void RegisterSlots()
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            int index = i;
            _slots[i].RegisterCallback<PointerDownEvent>(_ => ShowItemInfo(index));
        }
    }

    private void HandleSellTabClicked()
    {
        SwitchTab(InventoryController.InventoryType.Stored);
    }

    private void HandleBuyTabClicked()
    {
        SwitchTab(InventoryController.InventoryType.Shop);
    }

    private void SwitchTab(InventoryController.InventoryType targetTab)
    {
        if (_activeInventory == targetTab)
        {
            return;
        }

        _activeInventory = targetTab;
        SetActiveTab(targetTab);
        RenderActiveInventory();
        ClearItemInfo();
    }

    private void SetActiveTab(InventoryController.InventoryType targetTab)
    {
        if (_sellTab == null || _buyTab == null)
        {
            return;
        }

        _sellTab.RemoveFromClassList("shop__tab--active");
        _buyTab.RemoveFromClassList("shop__tab--active");

        if (targetTab == InventoryController.InventoryType.Stored)
        {
            _sellTab.AddToClassList("shop__tab--active");
        }
        else if (targetTab == InventoryController.InventoryType.Shop)
        {
            _buyTab.AddToClassList("shop__tab--active");
        }
    }

    private void RenderActiveInventory()
    {
        if (_slots == null || _slots.Count == 0)
        {
            return;
        }

        List<ItemDefinition> inventory = GetInventory(_activeInventory);

        if (inventory == null)
        {
            return;
        }

        EnsureInventorySize(inventory, _slots.Count);

        for (int i = 0; i < _slots.Count; i++)
        {
            ItemDefinition item = i < inventory.Count ? inventory[i] : null;
            _slots[i].style.backgroundImage = item != null ? new StyleBackground(item.Icon) : new StyleBackground();
        }
    }

    private void ShowItemInfo(int index)
    {
        List<ItemDefinition> inventory = GetInventory(_activeInventory);

        if (inventory == null)
        {
            ClearItemInfo();
            return;
        }

        EnsureInventorySize(inventory, _slots.Count);

        ItemDefinition item = inventory[index];
        if (item == null)
        {
            ClearItemInfo();
            return;
        }

        _selectedItemIndex = index;

        if (_itemInfo == null || _itemIcon == null || _itemName == null || _itemDescription == null || _itemPrice == null || _actionButton == null)
        {
            return;
        }

        _itemIcon.style.backgroundImage = new StyleBackground(item.Icon);
        _itemName.text = $"Название: {item.Name}";
        _itemDescription.text = item.Description;
        _itemPrice.text = $"Цена: {item.Price}";
        _actionButton.text = GetActionButtonText(item);
        _itemInfo.style.display = DisplayStyle.Flex;
    }

    private string GetActionButtonText(ItemDefinition item)
    {
        if (_activeInventory == InventoryController.InventoryType.Shop &&
            _playerResourcesController != null &&
            _playerResourcesController.ResoucesCount < item.Price)
        {
            int difference = item.Price - _playerResourcesController.ResoucesCount;
            return $"Тебе не хватает {difference}";
        }

        return _activeInventory == InventoryController.InventoryType.Shop
            ? $"Купить {item.Price}"
            : $"Продать {item.Price}";
    }

    private List<ItemDefinition> GetInventory(InventoryController.InventoryType type)
    {
        return type switch
        {
            InventoryController.InventoryType.Stored => _playerStoredInventory,
            InventoryController.InventoryType.Shop => _shopInventory,
            _ => null
        };
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

    private void ClearItemInfo()
    {
        if (_itemInfo == null)
        {
            return;
        }

        if (_itemIcon == null || _itemName == null || _itemDescription == null || _itemPrice == null || _actionButton == null)
        {
            return;
        }

        _itemIcon.style.backgroundImage = new StyleBackground();
        _itemName.text = string.Empty;
        _itemDescription.text = string.Empty;
        _itemPrice.text = string.Empty;
        _actionButton.text = "Нельзя купить или продать пустоту";
        _selectedItemIndex = -1;
    }

    private void HandleActionButtonClicked()
    {
        if (_selectedItemIndex < 0)
        {
            return;
        }

        List<ItemDefinition> inventory = GetInventory(_activeInventory);
        if (inventory == null)
        {
            return;
        }

        EnsureInventorySize(inventory, _slots.Count);

        ItemDefinition item = inventory[_selectedItemIndex];
        if (item == null)
        {
            return;
        }

        if (_activeInventory == InventoryController.InventoryType.Shop)
        {
            TryBuyItem(item);
        }
        else
        {
            SellItem(item);
        }
    }

    private void TryBuyItem(ItemDefinition item)
    {
        if (_playerResourcesController == null)
        {
            return;
        }

        if (_playerResourcesController.ResoucesCount < item.Price)
        {
            _actionButton.text = GetActionButtonText(item);
            return;
        }

        _playerResourcesController.DecreaseResources(item.Price);
        RemoveItemFromInventory(_shopInventory, _selectedItemIndex);
        AddItemToInventory(_playerStoredInventory, item);

        RenderActiveInventory();
        ClearItemInfo();
    }

    private void SellItem(ItemDefinition item)
    {
        if (_playerResourcesController == null)
        {
            return;
        }

        _playerResourcesController.IncreaseResources(item.Price);
        RemoveItemFromInventory(_playerStoredInventory, _selectedItemIndex);
        AddItemToInventory(_shopInventory, item);

        RenderActiveInventory();
        ClearItemInfo();
    }

    private void AddItemToInventory(List<ItemDefinition> inventory, ItemDefinition item)
    {
        if (inventory == null || item == null)
        {
            return;
        }

        EnsureInventorySize(inventory, _slots.Count);

        int emptyIndex = inventory.FindIndex(i => i == null);
        if (emptyIndex >= 0)
        {
            inventory[emptyIndex] = item;
        }
        else
        {
            inventory.Add(item);
        }
    }

    private void RemoveItemFromInventory(List<ItemDefinition> inventory, int index)
    {
        if (inventory == null)
        {
            return;
        }

        EnsureInventorySize(inventory, _slots.Count);

        if (index >= 0 && index < inventory.Count)
        {
            inventory[index] = null;
        }
    }
}
