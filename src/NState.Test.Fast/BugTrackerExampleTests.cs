﻿// ReSharper disable InconsistentNaming
namespace NState.Test.Fast
{
    using System;
    using Newtonsoft.Json;
    using NUnit.Framework;

    //define states
    [Serializable]
    public abstract class BugState : State
    {
        public class Assigned : BugState {}

        public class Closed : BugState {}

        public class Deferred : BugState {}

        public class Open : BugState {}

        public class Resolved : BugState {}
    }

    //define stateful type
    [Serializable]
    public class Bug : Stateful<Bug, BugState>
    {
        public Bug(string title, IStateMachine<Bug, BugState> stateMachine) : base(stateMachine)
        {
            Title = title;
        }

        public string Title { get; set; }

        public string AssigneeEmail { get; set; }

        public string ClosedByName { get; set; }

        public void Assign(string assigneeEmail)
        {
            TransitionTo(new BugState.Assigned(), new {AssigneeEmail = assigneeEmail});
        }

        public void Defer()
        {
            TransitionTo(new BugState.Deferred());
        }

        public void Resolve()
        {
            TransitionTo(new BugState.Resolved());
        }

        public void Close(string closedByName)
        {
            TransitionTo(new BugState.Closed(), new {ClosedByName = closedByName});
        }
    }

    //define transitions
    [Serializable]
    public class BugTransition
    {
        [Serializable]
        public class Assign : StateTransition<Bug, BugState>
        {
            public Assign(Func<Bug, BugState, dynamic, Bug> transitionFunction) : base(transitionFunction) {}

            public override BugState[] StartStates
            {
                get { return new BugState[] {new BugState.Open(), new BugState.Assigned(),}; }
            }

            public override BugState[] EndStates
            {
                get { return new[] {new BugState.Assigned(),}; }
            }
        }

        [Serializable]
        public class Close : StateTransition<Bug, BugState>
        {
            public Close(Func<Bug, BugState, dynamic, Bug> transitionFunction) : base(transitionFunction) {}

            public override BugState[] StartStates
            {
                get { return new[] {new BugState.Resolved(),}; }
            }

            public override BugState[] EndStates
            {
                get { return new[] {new BugState.Closed(),}; }
            }
        }

        [Serializable]
        public class Defer : StateTransition<Bug, BugState>
        {
            public Defer(Func<Bug, BugState, dynamic, Bug> transitionFunction) : base(transitionFunction) {}

            public override BugState[] StartStates
            {
                get { return new BugState[] {new BugState.Open(), new BugState.Assigned(),}; }
            }

            public override BugState[] EndStates
            {
                get { return new[] {new BugState.Deferred(),}; }
            }
        }

        [Serializable]
        public class Open : StateTransition<Bug, BugState>
        {
            public override BugState[] StartStates
            {
                get { return new[] {new BugState.Closed(),}; }
            }

            public override BugState[] EndStates
            {
                get { return new[] {new BugState.Open(),}; }
            }
        }

        [Serializable]
        public class Resolve : StateTransition<Bug, BugState>
        {
            public Resolve(Func<Bug, BugState, dynamic, Bug> transitionFunction) : base(transitionFunction) {}

            public override BugState[] StartStates
            {
                get { return new[] {new BugState.Assigned(),}; }
            }

            public override BugState[] EndStates
            {
                get { return new[] {new BugState.Resolved(),}; }
            }
        }
    }

    //define transition functions (if any)
    public class BugHelper
    {
        public static Bug Assign(Bug bug, BugState state, dynamic args)
        {
            bug.AssigneeEmail = args.AssigneeEmail;

            return bug;
        }

        public static Bug Defer(Bug bug, BugState state, dynamic args)
        {
            bug.AssigneeEmail = String.Empty;

            return bug;
        }

        public static Bug Resolve(Bug bug, BugState state, dynamic args)
        {
            bug.AssigneeEmail = String.Empty;

            return bug;
        }

        public static Bug Close(Bug bug, BugState state, dynamic args)
        {
            bug.ClosedByName = args.ClosedByName;

            return bug;
        }
    }

    [TestFixture]
    public class BugTrackerExampleTests
    {
        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            var transitions = new IStateTransition<Bug, BugState>[]
                                  {
                                      new BugTransition.Open(),
                                      new BugTransition.Assign(BugHelper.Assign),
                                      new BugTransition.Defer(BugHelper.Defer),
                                      new BugTransition.Resolve(BugHelper.Resolve),
                                      new BugTransition.Close(BugHelper.Close),
                                  };

            _stateMachine = new StateMachine<Bug, BugState>(transitions, startState: new BugState.Open());
        }

        #endregion

        private StateMachine<Bug, BugState> _stateMachine;

        [Test]
        public void CurrentState_ImmediatelyAfterConstruction_IsSetToInitialState()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);

            //act/assert
            Assert.That(bug.CurrentState == new BugState.Open());
        }

        [Test]
        public void PerformTransition_InvalidTransition_ExceptionThrown()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);

            //act/assert
            Assert.Throws<InvalidStateTransitionException<BugState>>(
                () => bug.TransitionTo(new BugState.Resolved()));
        }

        [Test]
        public void PerformTransition_TwoSuccessiveValidTransitions_StateSetCorrectly()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);

            //act/assert
            Assert.DoesNotThrow(() => bug.TransitionTo(new BugState.Assigned(),
                                                       new {AssigneeEmail = "example@example.com"}).TransitionTo(
                                                           new BugState.Deferred()));
        }

        [Test]
        public void PerformTransition_TwoSuccessiveValidTransitions_NoExceptionThrown()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);

            //act/assert
            Assert.DoesNotThrow(() => bug.TransitionTo(new BugState.Assigned(),
                                                       new {AssigneeEmail = "example@example.com"})
                                         .TransitionTo(new BugState.Deferred()));
        }

        [Test]
        public void PerformTransition_IdentityTransition_NoExceptionThrown()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);

            //act/assert
            Assert.DoesNotThrow(() => bug.TransitionTo(new BugState.Open()));
        }

        [Test]
        public void PerformTransition_UnexpectedDtoSupplied_NoExceptionThrown() //not sure how I could detect this in the framework
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);

            //act/assert
            Assert.DoesNotThrow(() => bug.TransitionTo(new BugState.Open(), new { Blah = "blah", }));
        }

        [Test]
        public void PerformTransition_ValidTransitionWithArgument_ArgumentSetInTargetObjectCorrectly()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);
            const string assigneeEmail = "example@example.com";

            //act/assert
            bug = bug.TransitionTo(new BugState.Assigned(),
                                   new {AssigneeEmail = assigneeEmail});

            Assert.That(bug.AssigneeEmail == assigneeEmail);
        }

        [Test]
        public void PerformTransition_ValidTransition_NoExceptionThrown()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);

            //act/assert
            Assert.DoesNotThrow(() => bug.TransitionTo(new BugState.Assigned(),
                                                       new {AssigneeEmail = "example@example.com"}));
        }

        [Test]
        public void Serialize_ValidStateMachine_NoExceptionThrown()
        {
            //arrange
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            
            //act/assert
            Assert.DoesNotThrow(() => JsonConvert.SerializeObject(_stateMachine, Formatting.Indented, settings));
        }


        //these fail due to inability to deserialize the generic type constraint BugState (it would appear)
        [Test, Ignore]
        public void DeSerialize_ValidStateMachine_NoExceptionThrown()
        {
            //arrange
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var serializedStateMachine = JsonConvert.SerializeObject(_stateMachine, Formatting.Indented, settings);

            //act/assert
            Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<StateMachine<Bug, BugState>>(serializedStateMachine));
        }

        [Test, Ignore]
        public void Serialization_RoundTrip_StateMaintainedCorrectly()
        {
            //arrange
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var serializedStateMachine = JsonConvert.SerializeObject(_stateMachine, Formatting.Indented, settings);

            //act
            var stateMachine = JsonConvert.DeserializeObject<StateMachine<Bug, BugState>>(serializedStateMachine);

            //assert
            Assert.That(stateMachine.ChildStateMachines.Count == 0);
            Assert.That(stateMachine.CurrentState == new BugState.Open());
        }

        [Test, Ignore]
        public void Serialization_RoundTrip_StateMaintainedCorrectly2()
        {
            //arrange
            var bug = new Bug("bug1", _stateMachine);
            bug.TransitionTo(new BugState.Assigned(), new { AssigneeEmail = "example@example.com" });
            
            //assert
            Assert.That(_stateMachine.CurrentState == new BugState.Assigned());

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var serializedStateMachine = JsonConvert.SerializeObject(_stateMachine, Formatting.Indented, settings);

            //act
            var stateMachine = JsonConvert.DeserializeObject<StateMachine<Bug, BugState>>(serializedStateMachine);

            //assert
            Assert.That(stateMachine.ChildStateMachines.Count == 0);
            Assert.That(stateMachine.CurrentState == new BugState.Assigned());
        }

        [Test, Ignore]
        public void Serialization_RoundTrip_TransitionsMayStillBePerformedAfterDeserialization()
        {
            //arrange
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            var serializedStateMachine = JsonConvert.SerializeObject(_stateMachine, Formatting.Indented, settings);

            //act
            var stateMachine = JsonConvert.DeserializeObject<StateMachine<Bug, BugState>>(serializedStateMachine);
            var bug = new Bug("bug1", stateMachine);

            //assert
            Assert.That(bug.CurrentState ==  new BugState.Open());
            
            //act
            bug.TransitionTo(new BugState.Assigned(), new { AssigneeEmail = "example@example.com", });
            
            Assert.That(bug.CurrentState == new BugState.Assigned());
        }
    }
}
// ReSharper restore InconsistentNaming