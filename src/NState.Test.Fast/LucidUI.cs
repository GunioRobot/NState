﻿namespace NState.Test.Fast
{
    public class LucidUI : IStateful<LucidUI, LucidUIState, LucidUI, LucidUIState, StateMachineType>
    {
        public IStateMachine<LucidUI, LucidUIState, LucidUI, LucidUIState, StateMachineType>
            GetStateMachine(IStateMachine<LucidUI, LucidUIState, LucidUI, LucidUIState, StateMachineType> stateMachine)
        {
            return stateMachine;
        }
    }
}