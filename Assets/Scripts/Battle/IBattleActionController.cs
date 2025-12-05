using System.Threading.Tasks;

public interface IBattleActionController
{
    Task ResolveAction(UnitModel target);
}
