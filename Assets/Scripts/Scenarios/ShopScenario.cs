using UnityEngine;
using VContainer;

public class ShopScenario : MonoBehaviour, IScenario
{
    [Inject]
    private readonly GameEventBus _gameEventBus;

    public void Run(GameObject target)
    {
        _gameEventBus.Publish(new OpenShop());
    }
}