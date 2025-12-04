using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class InventoryLauncher: MonoBehaviour
{
    [SerializeField]
    private InventoryPanel _uiPanel;

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
    private readonly List<IDisposable> _subscribtions = new();

    private void OnEnable()
    {
        _subscribtions.Add(_gameEventBus.Subscribe<OpenInventory>(HandleOpenInventory));

        var map = _actions.FindActionMap("Inventory", throwIfNotFound: true);
        _leaveAction = map.FindAction("Leave", throwIfNotFound: true);

        _uiPanel.gameObject.SetActive(true);
        _uiPanel.Hide();

        var inventories = _playerInteractionController.GetComponents<InventoryController>();
        _playerInventoryController = inventories
            .First(i => i.Type == InventoryController.InventoryType.Player);
        _storedInventoryController = inventories
            .First(i => i.Type == InventoryController.InventoryType.Stored);
    }

    private void OnDisable()
    {
        UnsubscribeFromInputActions();
        _subscribtions.ForEach(a => a.Dispose());
        _subscribtions.Clear();
        _leaveAction = null;
    }

    private void HandleOpenInventory(OpenInventory evt)
    {
        _gameInputSystem.EnterInventory();
        SubscribeToInputActions();
        _uiPanel.Show();
        _uiPanel.Render(_playerInventoryController.Inventory, _storedInventoryController.Inventory);
    }

    private void SubscribeToInputActions()
    {
        _leaveAction.performed += HandleLeaveInventory;
    }

    private void UnsubscribeFromInputActions()
    {
        _leaveAction.performed -= HandleLeaveInventory;
    }

    private void HandleLeaveInventory(InputAction.CallbackContext ctx)
    {
        _uiPanel.Hide();
        _gameEventBus.Publish(new CloseInventory());
    }
}