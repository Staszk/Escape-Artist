using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    public abstract class State
    {
        public virtual void OnEnter(Enemy theEnemy) { }
        public virtual void OnEnter(Civilian theCiv) { }
        public virtual void OnUpdate() { }
        public virtual void OnExit() { }

        public List<Transition> transitions;
        public Enemy enemy;
        public Civilian civilian;
    }

    public abstract class Transition
    {
        public virtual bool CheckCondition() { return false; }

        public State nextState;
    }
}
