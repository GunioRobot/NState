﻿//namespace NState.Test.Fast
//{
//    using System;

//    [Serializable]
//    public class AccountTabTransitions
//    {
//        [Serializable]
//        public class Collapse : StateTransition<AccountTab, AccountTabState>
//        {
//            public Collapse(Func<AccountTab, AccountTabState, dynamic, AccountTab> transitionFunction) : base(transitionFunction) {}

//            public override AccountTabState[] StartStates
//            {
//                get { return new[] { new AccountTabState.Expanded(), }; }
//            }

//            public override AccountTabState[] EndStates
//            {
//                get { return new[] { new AccountTabState.Collapsed(), }; }
//            }
//        }

//        [Serializable]
//        public class Expand : StateTransition<AccountTab, AccountTabState>
//        {
//            public Expand(Func<AccountTab, AccountTabState, dynamic, AccountTab> transitionFunction) : base(transitionFunction) {}

//            public override AccountTabState[] StartStates
//            {
//                get { return new[] { new AccountTabState.Collapsed(), }; }
//            }

//            public override AccountTabState[] EndStates
//            {
//                get { return new[] { new AccountTabState.Expanded(), }; }
//            }
//        }
//    }
//}