using UnityEngine;
using VContainer;

public class InventoryScenario : MonoBehaviour, IScenario
{
    [Inject]
    private readonly GameEventBus _gameEventBus;

    public void Run(GameObject target)
    {
        if (!TryGetComponent<InventoryController>(out var inventory))
        {
            Debug.LogWarning($"{nameof(InventoryScenario)} requires an {nameof(InventoryController)} on the same GameObject.");
            return;
        }

        _gameEventBus.Publish(new OpenInventory(inventory));
    }
}