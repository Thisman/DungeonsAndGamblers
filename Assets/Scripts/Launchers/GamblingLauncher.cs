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

    private PlayerResourcesController _playerResourcesController;

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
        _playerResourcesController = _playerInteractionController.GetComponent<PlayerResourcesController>();

        if (_uiPanel != null)
        {
            _uiPanel.SpinFinished += HandleSpinFinished;
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromInputActions();
        _subscriptions.ForEach(a => a.Dispose());
        _subscriptions.Clear();
        _leaveAction = null;

        if (_uiPanel != null)
        {
            _uiPanel.SpinFinished -= HandleSpinFinished;
        }
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

        if (_playerResourcesController != null)
        {
            bets.Add(new ResourcesGamblingBet(_playerResourcesController));
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

    private void HandleSpinFinished()
    {
        _uiPanel.Render(BuildBets());
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

    public string Description =>
        $"Ваша ставка {_item?.Name ?? "предмет"}, при победе у вас будет <color=#00ff00>{_item?.Name ?? "предмет"}</color>, при проигрыше <color=#ff0000>ничего</color>.";

    public bool CanUse => !_itemRemoved;

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

    public string Description
    {
        get
        {
            int current = _getter?.Invoke() ?? 0;
            int winValue = Mathf.Max(1, Mathf.FloorToInt(current * 1.5f));
            int loseValue = Mathf.Max(1, Mathf.FloorToInt(current / 2f));
            return
                $"Ваша ставка {current}, при победе у вас будет <color=#00ff00>{winValue}</color>, при проигрыше <color=#ff0000>{loseValue}</color>.";
        }
    }

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

public class ResourcesGamblingBet : IGamblingBet
{
    private readonly PlayerResourcesController _controller;

    public ResourcesGamblingBet(PlayerResourcesController controller)
    {
        _controller = controller;
    }

    public string Name => "Ресурсы";

    public string Description
    {
        get
        {
            int current = _controller?.ResoucesCount ?? 0;
            int winValue = Mathf.FloorToInt(current * 1.5f);
            int loseValue = Mathf.FloorToInt(current / 2f);
            return
                $"Ваша ставка {current}, при победе у вас будет <color=#00ff00>{winValue}</color>, при проигрыше <color=#ff0000>{loseValue}</color>.";
        }
    }

    public bool CanUse => _controller?.ResoucesCount > 0;

    public void ApplyWin()
    {
        if (_controller == null)
        {
            return;
        }

        int reward = Mathf.FloorToInt(_controller.ResoucesCount * 0.5f);
        _controller.IncreaseResources(reward);
    }

    public void ApplyLose()
    {
        if (_controller == null)
        {
            return;
        }

        int loss = Mathf.FloorToInt(_controller.ResoucesCount / 2f);
        _controller.DecreaseResources(loss);
    }
}
