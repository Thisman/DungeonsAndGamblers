using System.Collections.Generic;
using System.Linq;

public class BattleQueue
{
    private readonly List<UnitModel> _units;
    private int _currentIndex;
    private int _roundNumber = 1;

    public BattleQueue(IEnumerable<UnitModel> units)
    {
        _units = units?
            .Where(unit => unit != null)
            .OrderByDescending(unit => unit.Health)
            .ToList() ?? new List<UnitModel>();
    }

    public BattleQueueResult GetNextUnit()
    {
        if (_units.Count == 0)
        {
            return BattleQueueResult.RoundEnded(_roundNumber);
        }

        if (_currentIndex >= _units.Count)
        {
            var finishedRound = _roundNumber;
            _roundNumber++;
            _currentIndex = 0;

            return BattleQueueResult.RoundEnded(finishedRound);
        }

        var unit = _units[_currentIndex];
        _currentIndex++;

        return new BattleQueueResult(unit, _roundNumber, false);
    }
}

public readonly struct BattleQueueResult
{
    public UnitModel Unit { get; }
    public int RoundNumber { get; }
    public bool IsRoundEnded { get; }

    public BattleQueueResult(UnitModel unit, int roundNumber, bool isRoundEnded)
    {
        Unit = unit;
        RoundNumber = roundNumber;
        IsRoundEnded = isRoundEnded;
    }

    public static BattleQueueResult RoundEnded(int roundNumber)
    {
        return new BattleQueueResult(null, roundNumber, true);
    }
}
