using System.Threading.Tasks;
using UnityEngine;

public class EnemyBattleActionController : MonoBehaviour, IBattleActionController
{
    public async Task ResolveAction(UnitModel target)
    {
        var delay = Random.Range(1000, 3001);
        await Task.Delay(delay);
    }
}
