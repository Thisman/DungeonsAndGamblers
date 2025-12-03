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

    [SerializeField]
    private Transform _enemySpawn;

    [SerializeField]
    private List<GameObject> _enemyPrefabs;

    [SerializeField]
    private UIPanel _uiPanel;

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

    private EnemyGenerator _enemyGenerator;

    private InputAction _leaveAction;
    private BattleStateMachine _battleStateMachine;
    private readonly List<IDisposable> _subsribtions = new();
    private GameObject _currentEnemy;

    private void OnEnable()
    {
        _subsribtions.Add(_gameEventBus.Subscribe<StartDungeon>(HandleStartDungeon));

        var map = _actions.FindActionMap("Dungeon", throwIfNotFound: true);
        _leaveAction = map.FindAction("Leave", throwIfNotFound: true);

        _uiPanel.Hide();
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
        CreateEnemy();
        StartBattle();
        _uiPanel.Show();
    }

    private void CreateEnemy()
    {
        _enemyGenerator ??= _resolver.Resolve<EnemyGenerator>();
        _currentEnemy = _enemyGenerator.CreateEnemy(_enemyPrefabs, _enemySpawn);
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
        Destroy(_currentEnemy);
        _battleStateMachine.Stop();
        _battleStateMachine = null;
        _currentEnemy = null;
        _uiPanel.Hide();
        _gameEventBus.Publish(new LeaveDungeon());
    }
}