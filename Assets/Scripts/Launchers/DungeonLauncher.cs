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
    private DungeonPanel _uiPanel;

    [Inject]
    private readonly IObjectResolver _resolver;

    [Inject]
    private readonly GameEventBus _gameEventBus;

    [Inject]
    private readonly GameInputSystem _gameInputSystem;

    [Inject]
    private readonly InputActionAsset _inputActions;

    [Inject]
    private readonly PlayerInteractionController _playerInteractionController;

    private EnemyGenerator _enemyGenerator;

    private InputAction _leaveAction;
    private UnitModel _currentEnemy;
    private UnitModel _currentPlayer;
    private BattleStateMachine _battleStateMachine;
    private readonly List<IDisposable> _subsribtions = new();

    private void OnEnable()
    {
        _subsribtions.Add(_gameEventBus.Subscribe<StartDungeon>(HandleStartDungeon));

        var map = _inputActions.FindActionMap("Dungeon", throwIfNotFound: true);
        _leaveAction = map.FindAction("Leave", throwIfNotFound: true);

        _uiPanel.gameObject.SetActive(true);
        _uiPanel.Hide();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputActions();
        _subsribtions.ForEach(a => a.Dispose());
        _subsribtions.Clear();
        _leaveAction = null;
    }

    private void Update()
    {
        if (_currentEnemy != null && _currentPlayer != null)
        {
            _uiPanel.Render(_currentPlayer, _currentEnemy);
        }
    }

    private void HandleStartDungeon(StartDungeon evt)
    {
        _gameInputSystem.EnterDungeon();
        TeleportPlayerToDungeon();
        SubscribeToInputActions();
        InitalizePlayer();
        CreateEnemy();
        StartBattle();
        _uiPanel.Show();
    }

    private void InitalizePlayer()
    {
        _currentPlayer = _playerInteractionController.GetComponent<UnitModel>();
    }

    private void CreateEnemy()
    {
        _enemyGenerator ??= _resolver.Resolve<EnemyGenerator>();
        _currentEnemy = _enemyGenerator.CreateEnemy(_enemyPrefabs, _enemySpawn)
            .GetComponent<UnitModel>();
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
            if (_enemySpawn != null)
            {
                Vector3 lookTarget = _enemySpawn.position;
                Vector3 playerLookTarget = new Vector3(lookTarget.x, player.transform.position.y, lookTarget.z);
                player.transform.LookAt(playerLookTarget);

                var cameraController = player.GetComponentInChildren<PlayerCameraController>();
                if (cameraController != null)
                {
                    cameraController.LookAt(lookTarget);
                }
            }
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
        Destroy(_currentEnemy.gameObject);
        _battleStateMachine.Stop();
        _battleStateMachine = null;
        _currentEnemy = null;
        _currentPlayer = null;
        _uiPanel.Hide();
        _gameEventBus.Publish(new LeaveDungeon());
    }
}