using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class InventoryLauncher: MonoBehaviour
{
    [SerializeField]
    private InventoryPanel _inventoryPanel;

    [SerializeField]
    private InventoryController _playerInventory;

    [Inject]
    private readonly GameEventBus _gameEventBus;

    [Inject]
    private readonly GameInputSystem _gameInputSystem;

    [Inject]
    private readonly InputActionAsset _actions;

    private InputAction _leaveAction;
    private readonly List<IDisposable> _subscribtions = new();

    private void OnEnable()
    {
        _subscribtions.Add(_gameEventBus.Subscribe<OpenInventory>(HandleOpenInventory));

        var map = _actions.FindActionMap("Inventory", throwIfNotFound: true);
        _leaveAction = map.FindAction("Leave", throwIfNotFound: true);

        _inventoryPanel.gameObject.SetActive(true);
        _inventoryPanel.Hide();
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
        if (_playerInventory == null)
        {
            Debug.LogWarning("InventoryLauncher requires player inventory to be assigned.");
            return;
        }

        _gameInputSystem.EnterInventory();
        SubscribeToInputActions();
        _inventoryPanel.Render(_playerInventory, evt.SourceInventory);
        _inventoryPanel.Show();
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
        _inventoryPanel.Hide();
        _gameEventBus.Publish(new CloseInventory());
    }
}