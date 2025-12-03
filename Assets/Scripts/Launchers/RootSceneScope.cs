// Configures root scene-level services such as session and scene loading systems.
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

public class RootSceneScope : LifetimeScope
{
    [SerializeField]
    private InputActionAsset _inputActions;

    [SerializeField]
    private PlayerInteractionController _playerInteractionController;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<GameEventBus>(Lifetime.Singleton);
        builder.Register<GameInputSystem>(Lifetime.Singleton);
        builder.RegisterInstance(_inputActions).As<InputActionAsset>();
        builder.RegisterComponentInHierarchy<IScenario>();
        builder.RegisterInstance(_playerInteractionController).AsSelf();
        builder.Register<BattleStateMachine>(Lifetime.Transient);
        builder.Register<EnemyGenerator>(Lifetime.Transient);
    }
}
