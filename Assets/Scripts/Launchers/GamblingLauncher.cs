using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class GamblingLauncher : MonoBehaviour
{
    [SerializeField]
    private GamblingPanel _uiPanel;

    [Inject]
    private readonly GameEventBus _gameEventBus;

    [Inject]
    private readonly GameInputSystem _gameInputSystem;

    [Inject]
    private readonly InputActionAsset _actions;

    [Inject]
    private readonly PlayerInteractionController _playerInteractionController;

    private InputAction _leaveAction;
    private InventoryController _playerInventoryController;
    private InventoryController _storedInventoryController;
    private UnitModel _playerUnitModel;
    private readonly List<IDisposable> _subscriptions = new();

    private void OnEnable()
    {
        _subscriptions.Add(_gameEventBus.Subscribe<StartGambling>(HandleStartGambling));
        _subscriptions.Add(_gameEventBus.Subscribe<EndGambling>(_ => HandleEndGambling()));

        var map = _actions.FindActionMap("Gambling", throwIfNotFound: true);
        _leaveAction = map.FindAction("Leave", throwIfNotFound: true);

        _uiPanel.gameObject.SetActive(true);
        _uiPanel.Hide();

        var inventories = _playerInteractionController.GetComponents<InventoryController>();
        _playerInventoryController = inventories.First(i => i.Type == InventoryController.InventoryType.Player);
        _storedInventoryController = inventories.First(i => i.Type == InventoryController.InventoryType.Stored);
        _playerUnitModel = _playerInteractionController.GetComponent<UnitModel>();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputActions();
        _subscriptions.ForEach(a => a.Dispose());
        _subscriptions.Clear();
        _leaveAction = null;
    }

    private void HandleStartGambling(StartGambling evt)
    {
        _gameInputSystem.EnterGambling();
        SubscribeToInputActions();
        _uiPanel.Render(BuildBets());
        _uiPanel.Show();
    }

    private void HandleEndGambling()
    {
        _uiPanel.Hide();
        UnsubscribeFromInputActions();
    }

    private void HandleLeaveGambling(InputAction.CallbackContext ctx)
    {
        _gameEventBus.Publish(new EndGambling());
    }

    private void SubscribeToInputActions()
    {
        _leaveAction.performed += HandleLeaveGambling;
    }

    private void UnsubscribeFromInputActions()
    {
        if (_leaveAction != null)
        {
            _leaveAction.performed -= HandleLeaveGambling;
        }
    }

    private List<IGamblingBet> BuildBets()
    {
        var bets = new List<IGamblingBet>();

        bets.AddRange(CreateItemBets(_playerInventoryController));
        bets.AddRange(CreateItemBets(_storedInventoryController));

        if (_playerUnitModel != null)
        {
            bets.Add(new StatGamblingBet("Здоровье", () => _playerUnitModel.Health, value => _playerUnitModel.SetHealth(value)));
            bets.Add(new StatGamblingBet("Урон", () => _playerUnitModel.Damage, value => _playerUnitModel.SetDamage(value)));
        }

        return bets;
    }

    private IEnumerable<IGamblingBet> CreateItemBets(InventoryController controller)
    {
        if (controller?.Inventory == null)
        {
            yield break;
        }

        foreach (var item in controller.Inventory)
        {
            if (item == null)
            {
                continue;
            }

            yield return new ItemGamblingBet(item, controller.Inventory);
        }
    }
}

public class ItemGamblingBet : IGamblingBet
{
    private readonly ItemDefinition _item;
    private readonly List<ItemDefinition> _inventory;
    private bool _itemRemoved;

    public ItemGamblingBet(ItemDefinition item, List<ItemDefinition> inventory)
    {
        _item = item;
        _inventory = inventory;
    }

    public string Name => _item?.Name ?? string.Empty;

    public bool CanUse => true;

    public void ApplyWin() { }

    public void ApplyLose()
    {
        if (_itemRemoved || _inventory == null)
        {
            return;
        }

        int index = _inventory.IndexOf(_item);
        if (index >= 0)
        {
            _inventory.RemoveAt(index);
        }

        _itemRemoved = true;
    }
}

public class StatGamblingBet : IGamblingBet
{
    private readonly string _name;
    private readonly Func<int> _getter;
    private readonly Action<int> _setter;

    public StatGamblingBet(string name, Func<int> getter, Action<int> setter)
    {
        _name = name;
        _getter = getter;
        _setter = setter;
    }

    public string Name => _name;

    public bool CanUse => _getter?.Invoke() > 1;

    public void ApplyWin()
    {
        if (_getter == null || _setter == null)
        {
            return;
        }

        int newValue = Mathf.FloorToInt(_getter() * 1.5f);
        _setter(Mathf.Max(1, newValue));
    }

    public void ApplyLose()
    {
        if (_getter == null || _setter == null)
        {
            return;
        }

        int newValue = Mathf.FloorToInt(_getter() / 2f);
        _setter(Mathf.Max(1, newValue));
    }
}
