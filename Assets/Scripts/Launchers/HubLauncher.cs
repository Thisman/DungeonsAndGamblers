using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class HubLauncher: MonoBehaviour
{
    [SerializeField]
    private Transform _playerSpawn;

    [Inject]
    private readonly GameInputSystem _gameInputSystem;

    [Inject]
    private readonly GameEventBus _gameEventBus;

    [Inject]
    private readonly PlayerInteractionController _playerInteractionController;

    private readonly List<IDisposable> _subsribtions = new();

    private void OnEnable()
    {
        _subsribtions.Add(_gameEventBus.Subscribe<LeaveDungeon>(HandleStartHub));
        _subsribtions.Add(_gameEventBus.Subscribe<CloseInventory>(HandleStartHub));
        _subsribtions.Add(_gameEventBus.Subscribe<CloseShop>(HandleStartHub));
        _subsribtions.Add(_gameEventBus.Subscribe<EndGambling>(HandleStartHub));
        _gameInputSystem.EnterHub();
    }
    private void OnDisable()
    {
        _subsribtions.ForEach(a => a.Dispose());
        _subsribtions.Clear();
    }

    private void HandleStartHub(LeaveDungeon evt)
    {
        _gameInputSystem.EnterHub();
        TeleportPlayerToDungeon();
    }

    private void HandleStartHub(CloseInventory evt)
    {
        _gameInputSystem.EnterHub();
    }

    private void HandleStartHub(CloseShop evt)
    {
        _gameInputSystem.EnterHub();
    }

    private void HandleStartHub(EndGambling evt)
    {
        _gameInputSystem.EnterHub();
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
}