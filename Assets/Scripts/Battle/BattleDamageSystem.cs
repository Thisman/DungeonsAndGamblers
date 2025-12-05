using System.Threading.Tasks;
using UnityEngine;

public class BattleDamageSystem
{
    public async Task ResolveDamage(UnitModel actor, UnitModel target)
    {
        if (actor == null || target == null)
        {
            return;
        }

        var actorAnimationController = actor.GetBattleAnimationController();

        if (actorAnimationController != null)
        {
            await actorAnimationController.PlayAttackAsync();
        }

        target.ApplyDamage(actor.Damage);

        var targetAnimationController = target.GetBattleAnimationController();

        if (targetAnimationController != null)
        {
            await targetAnimationController.PlayDamageAsync();
        }
    }
}
