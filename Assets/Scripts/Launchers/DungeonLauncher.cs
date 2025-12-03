using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

public class DungeonLauncher: MonoBehaviour
{
    [SerializeField]
    private Transform _playerSpawn;

    [Inject]
    private readonly IObjectResolver _resolver;

    [Inject]
    private readonly GameEventBus _gameEventBus;

    [Inject]
    private readonly GameInputSystem _gameInputSystem;

    [Inject]
    private readonly InputActionAsset _actions;

    [Inject]
    private readonly PlayerInteractionController _playerInteractionController;

    private InputAction _leaveAction;
    private BattleStateMachine _battleStateMachine;
    private readonly List<IDisposable> _subsribtions = new();

    private void OnEnable()
    {
        _subsribtions.Add(_gameEventBus.Subscribe<StartDungeon>(HandleStartDungeon));

        var map = _actions.FindActionMap("Dungeon", throwIfNotFound: true);
        _leaveAction = map.FindAction("Leave", throwIfNotFound: true);
    }

    private void OnDisable()
    {
        UnsubscribeFromInputActions();
        _subsribtions.ForEach(a => a.Dispose());
        _subsribtions.Clear();
        _leaveAction = null;
    }

    private void HandleStartDungeon(StartDungeon evt)
    {
        _gameInputSystem.EnterDungeon();
        TeleportPlayerToDungeon();
        SubscribeToInputActions();
        StartBattle();
    }

    private void StartBattle()
    {
        _battleStateMachine = _resolver.Resolve<BattleStateMachine>();
        _battleStateMachine.Start();
    }

    private void TeleportPlayerToDungeon()
    {
        var player = _playerInteractionController.gameObject;
        if (player.TryGetComponent<CharacterController>(out var cc))
        {
            cc.enabled = false;
            player.transform.position = _playerSpawn.transform.position;
            cc.enabled = true;
        }
    }

    private void SubscribeToInputActions() {
        _leaveAction.performed += HandleLeaveDungeon;
    }

    private void UnsubscribeFromInputActions() {
        _leaveAction.performed -= HandleLeaveDungeon;
    }

    private void HandleLeaveDungeon(InputAction.CallbackContext ctx)
    {
        _battleStateMachine.Stop();
        _battleStateMachine = null;
        _gameEventBus.Publish(new LeaveDungeon());
    }
}