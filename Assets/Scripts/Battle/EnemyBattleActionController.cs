using System.Threading.Tasks;
using UnityEngine;

public class EnemyBattleActionController : MonoBehaviour, IBattleActionController
{
    private UnitModel _unitModel;
    private BattleDamageSystem _battleDamageSystem;

    private void Awake()
    {
        _unitModel = GetComponent<UnitModel>();
        _battleDamageSystem = new BattleDamageSystem();
    }

    public async Task ResolveAction(UnitModel target)
    {
        var delay = Random.Range(1000, 3001);
        await Task.Delay(delay);

        if (_unitModel != null && target != null)
        {
            await _battleDamageSystem.ResolveDamage(_unitModel, target);
        }
    }
}
