using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM<T> where T : class
{
    private BaseState<T> curState;
    
    public FSM(BaseState<T> _initState)
    {
        curState = _initState;
        ChangeState(curState);
    }

    public void ChangeState(BaseState<T> _nextState)
    {
        if (curState == _nextState) return;

        if (curState != null) curState.OnStateExit();

        curState = _nextState;
        curState.OnStateEnter();
    }

    public void UpdateState()
    {
        if (curState == null) return;

        curState.OnStateUpdate();
    }
}
