using UnityEngine;

namespace stateMachine
{

    public abstract class State<T> : ScriptableObject where T : MonoBehaviour
    {
        protected T _runner;

        public virtual void Init(T runner)
        {
            _runner = runner;
        }   

        public abstract void Enter();
        public abstract void CaptureInput();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void ChangeState();
        public abstract void Exit();         
    }   

}