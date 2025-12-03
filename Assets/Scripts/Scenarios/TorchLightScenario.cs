using UnityEngine;

public class TorchLightScenario: MonoBehaviour, IScenario
{
    [SerializeField]
    private Light _light;

    [SerializeField]
    private GameObject _flame;

    public void Run(GameObject target)
    {
        _light.enabled = !_light.enabled;
        _flame.SetActive(!_flame.activeSelf);
    }
}