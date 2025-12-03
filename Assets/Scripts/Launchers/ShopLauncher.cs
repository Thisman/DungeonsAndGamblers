using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;

public class ShopLauncher : MonoBehaviour
{
    [SerializeField]
    private UIPanel _uiPanel;

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
        _subscribtions.Add(_gameEventBus.Subscribe<OpenShop>(HandleOpenShop));

        var map = _actions.FindActionMap("Shop", throwIfNotFound: true);
        _leaveAction = map.FindAction("Leave", throwIfNotFound: true);

        _uiPanel.gameObject.SetActive(true);
        _uiPanel.Hide();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputActions();
        _subscribtions.ForEach(a => a.Dispose());
        _subscribtions.Clear();
        _leaveAction = null;
    }

    private void HandleOpenShop(OpenShop evt)  
    {
        _gameInputSystem.EnterShop();
        SubscribeToInputActions();
        _uiPanel.Show();
    }

    private void SubscribeToInputActions()
    {
        _leaveAction.performed += HandleLeaveShop;
    }

    private void UnsubscribeFromInputActions()
    {
        _leaveAction.performed -= HandleLeaveShop;
    }

    private void HandleLeaveShop(InputAction.CallbackContext ctx)
    {
        _uiPanel.Hide();
        _gameEventBus.Publish(new CloseShop());
    }
}