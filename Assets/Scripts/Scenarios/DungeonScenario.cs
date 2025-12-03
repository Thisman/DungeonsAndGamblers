using UnityEngine;
using VContainer;

public class DungeonScenario: MonoBehaviour, IScenario
{
    [Inject]
    private readonly GameEventBus _gameEventBus;

    public void Run(GameObject target)
    {
        _gameEventBus.Publish(new StartDungeon(target));
    }
}