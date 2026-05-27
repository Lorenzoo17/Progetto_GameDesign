using UnityEngine;
using System;
using System.Collections.Generic;

namespace stateMachine
{
    public abstract class StateRunner<T> : MonoBehaviour where T : MonoBehaviour
    {
        [SerializeField] private List<State<T>> states;
        private readonly Dictionary<Type, State<T>> stateDictionary = new Dictionary<Type, State<T>>();
        private State<T> currentState;

        protected virtual void Awake() {
            // Cloniamo gli stati per evitare interferenze tra istanze diverse dello stesso Boss
            foreach (var s in states) {
                State<T> stateInstance = Instantiate(s);
                stateInstance.Init(this as T);
                stateDictionary.Add(stateInstance.GetType(), stateInstance);
            }
            
            if (states.Count > 0)
                SetState(states[0].GetType());
        }

        public void SetState(Type newStateType) {
            if (currentState != null) currentState.Exit();

            if (stateDictionary.TryGetValue(newStateType, out State<T> newState)) {
                currentState = newState;
                currentState.Enter(); // Ora chiamiamo correttamente l'inizio dello stato
                Debug.Log($"[FSM] {gameObject.name} passato allo stato: {newStateType.Name}");
            } else {
                Debug.LogError("Stato " + newStateType + " non trovato in " + gameObject.name);
            }
        }

        protected virtual void Update() {
            if (currentState != null) {
                currentState.CaptureInput();
                currentState.Update();
                currentState.ChangeState();
            }
        }

        protected virtual void FixedUpdate() {
            if (currentState != null) currentState.FixedUpdate();
        }
    }
}