using UnityEngine;

public class PlayerResourcesController : MonoBehaviour
{
    [SerializeField]
    private int _defaultResourcesCount;

    private int _resourcesCount;

    public int ResoucesCount => _resourcesCount;

    private void Awake()
    {
        _resourcesCount = _defaultResourcesCount;
    }

    public void IncreaseResources(int amount)
    {
        _resourcesCount += amount;
    }

    public void DecreaseResources(int amount)
    {
        _resourcesCount -= amount;
    }
}
