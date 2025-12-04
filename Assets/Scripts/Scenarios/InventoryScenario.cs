using UnityEngine;
using VContainer;

public class InventoryScenario : MonoBehaviour, IScenario
{
    [Inject]
    private readonly GameEventBus _gameEventBus;

    public void Run(GameObject target)
    {
        if (TryGetComponent<InventoryController>(out var inventoryController))
        {
            _gameEventBus.Publish(new OpenInventory(inventoryController));
        }
    }
}