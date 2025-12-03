using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class EnemyGenerator
{
    private readonly IObjectResolver _resolver;

    [Inject]
    public EnemyGenerator(IObjectResolver resolver)
    {
        _resolver = resolver;
    }

    public GameObject CreateEnemy(IReadOnlyList<GameObject> prefabs, Transform spawnPoint)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning("No enemy prefabs provided.");
            return null;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("Spawn point is null.");
            return null;
        }

        var prefab = prefabs[Random.Range(0, prefabs.Count)];
        return _resolver.Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
    }
}
