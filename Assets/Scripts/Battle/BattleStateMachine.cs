// Coordinates battle round and turn flow using Stateless for state transitions with entry/exit hooks for every state.
using Stateless;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

public class BattleStateMachine
{
    [Inject]
    private readonly GameEventBus _sceneEventBus;

    private BattleState _currentState;
    private StateMachine<BattleState, Trigger> _stateMachine;

    private readonly List<IDisposable> _subscribtions = new();

    public void Start()
    {
        _stateMachine = new StateMachine<BattleState, Trigger>(() => _currentState, state => _currentState = state);

        ConfigureTransitions();
        _stateMachine.OnTransitioned(transition =>
        {
            _sceneEventBus.Publish(new BattleStateChanged(transition.Source, transition.Destination));
            Debug.Log($"Change state from {transition.Source} to {transition.Destination}");
        });

        SubscribeToSceneEvents();

        Fire(Trigger.InitRound);
    }

    public void Stop()
    {
        _stateMachine = null;
        _currentState = BattleState.None;

        _subscribtions.Clear();
        UnsubscribeFromSceneEvents();
    }

    private void ConfigureTransitions()
    {
        ConfigureState(BattleState.None)
            .Permit(Trigger.InitRound, BattleState.RoundInit);

        ConfigureState(BattleState.RoundInit)
            .Permit(Trigger.StartRound, BattleState.RoundStart)
            .Permit(Trigger.Finish, BattleState.Finish);

        ConfigureState(BattleState.RoundStart)
            .Permit(Trigger.InitTurn, BattleState.TurnInit)
            .Permit(Trigger.Finish, BattleState.Finish);

        ConfigureState(BattleState.TurnInit)
            .Permit(Trigger.StartTurn, BattleState.TurnStart)
            .Permit(Trigger.EndRound, BattleState.RoundEnd)
            .Permit(Trigger.Finish, BattleState.Finish);

        ConfigureState(BattleState.TurnStart)
            .Permit(Trigger.ActionWait, BattleState.WaitForAction)
            .Permit(Trigger.Finish, BattleState.Finish);

        ConfigureState(BattleState.WaitForAction)
            .Permit(Trigger.EndTurn, BattleState.TurnEnd)
            .Permit(Trigger.Finish, BattleState.Finish);

        ConfigureState(BattleState.TurnEnd)
            .Permit(Trigger.InitTurn, BattleState.TurnInit)
            .Permit(Trigger.Finish, BattleState.Finish);

        ConfigureState(BattleState.RoundEnd)
            .Permit(Trigger.InitRound, BattleState.RoundInit)
            .Permit(Trigger.Finish, BattleState.Finish);

        ConfigureState(BattleState.Finish)
            .Ignore(Trigger.InitRound)
            .Ignore(Trigger.StartRound)
            .Ignore(Trigger.InitRound)
            .Ignore(Trigger.StartTurn)
            .Ignore(Trigger.ActionWait)
            .Ignore(Trigger.EndTurn)
            .Ignore(Trigger.EndRound);
    }

    private StateMachine<BattleState, Trigger>.StateConfiguration ConfigureState(BattleState state)
    {
        return _stateMachine.Configure(state)
            .OnEntry(() => EnterState(state))
            .OnExit(() => ExitState(state));
    }

    private void EnterState(BattleState state)
    {

        switch (state)
        {
            case BattleState.RoundInit:
                EnterRoundInit();
                break;
            case BattleState.RoundStart:
                EnterRoundStart();
                break;
            case BattleState.TurnInit:
                EnterTurnInit();
                break;
            case BattleState.TurnStart:
                EnterTurnStart();
                break;
            case BattleState.WaitForAction:
                EnterWaitForAction();
                break;
            case BattleState.TurnEnd:
                EnterTurnEnd();
                break;
            case BattleState.RoundEnd:
                EnterRoundEnd();
                break;
            case BattleState.Finish:
                EnterFinish();
                break;
        }
    }

    private void ExitState(BattleState state)
    {
        switch (state)
        {
            case BattleState.RoundInit:
                ExitRoundInit();
                break;
            case BattleState.RoundStart:
                ExitRoundStart();
                break;
            case BattleState.TurnInit:
                ExitTurnInit();
                break;
            case BattleState.TurnStart:
                ExitTurnStart();
                break;
            case BattleState.WaitForAction:
                ExitWaitForAction();
                break;
            case BattleState.TurnEnd:
                ExitTurnEnd();
                break;
            case BattleState.RoundEnd:
                ExitRoundEnd();
                break;
            case BattleState.Finish:
                ExitFinish();
                break;
        }
    }

    private void EnterRoundInit()
    {
        Fire(Trigger.StartRound);
    }

    private void ExitRoundInit() { }

    private void EnterRoundStart()
    {
        Fire(Trigger.InitTurn);
    }

    private void ExitRoundStart() { }

    private void EnterTurnInit()
    {
        Fire(Trigger.StartTurn);
    }

    private void ExitTurnInit() { }

    private void EnterTurnStart()
    {
        Fire(Trigger.ActionWait);
    }

    private void ExitTurnStart() { }

    private async void EnterWaitForAction()
    {
        await Task.Delay(3000);

        if (_currentState != BattleState.Finish && _currentState != BattleState.None)
            Fire(Trigger.EndTurn);
    }

    private void ExitWaitForAction() { }

    private void EnterTurnEnd()
    {
        Fire(Trigger.InitTurn);
    }

    private void ExitTurnEnd() { }

    private void EnterRoundEnd()
    {
        Fire(Trigger.InitRound);
    }

    private void ExitRoundEnd() { }

    private void EnterFinish()
    {
    }

    private void ExitFinish()
    {
        UnsubscribeFromSceneEvents();
    }

    private void Fire(Trigger trigger)
    {
        _stateMachine.Fire(trigger);
    }

    private void SubscribeToSceneEvents()
    {
    }

    private void UnsubscribeFromSceneEvents()
    {
        _subscribtions.ForEach(subscribtion => subscribtion.Dispose());
        _subscribtions.Clear();
    }

    private enum Trigger
    {
        InitRound,
        StartRound,
        InitTurn,
        StartTurn,
        ActionWait,
        EndTurn,
        EndRound,
        Finish
    }
}
