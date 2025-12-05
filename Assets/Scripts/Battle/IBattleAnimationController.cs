using System.Threading.Tasks;

public interface IBattleAnimationController
{
    Task PlayAttackAsync();
    Task PlayDamageAsync();
}
