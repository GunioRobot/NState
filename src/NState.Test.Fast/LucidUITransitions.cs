﻿namespace NState.Test.Fast
{
    using System;

    [Serializable]
    public class LucidUITransitions
    {
        [Serializable]
        public class Pause : StateTransition<LucidUI, LucidUIState, LucidUIState, StateMachineType>
        {
            public Pause(Func<LucidUI, LucidUIState, LucidUI> transitionFunction) : base(transitionFunction) {}

            public override LucidUIState StartState
            {
                get { return new LucidUIState.Active(); }
            }

            public override LucidUIState EndState
            {
                get { return new LucidUIState.Paused(); }
            }
        }

        [Serializable]
        public class Resume : StateTransition<LucidUI, LucidUIState, LucidUIState, StateMachineType>
        {
            public Resume(Func<LucidUI, LucidUIState, LucidUI> transitionFunction) : base(transitionFunction) {}

            public override LucidUIState StartState
            {
                get { return new LucidUIState.Paused(); }
            }

            public override LucidUIState EndState
            {
                get { return new LucidUIState.Active(); }
            }
        }
    }
}