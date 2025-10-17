using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace IA
{
    public class StateMachine : MonoBehaviour
    {
        private slimeState _currentState;
        
        private float _waitingTimer = 5f;
        
        [SerializeField] NavMeshAgent _agent;
        
        enum slimeState
        {
            Idle,
            Walk
        }

        private void Start()
        {
            _currentState = slimeState.Idle;
            OnStateEnter(_currentState);
        }

        private void Update()
        {
            OnStateUpdate(_currentState);
        }
        
        public static Vector3 GetRandomPoint(Vector3 origin, float range)
        {
            Vector3 randomDirection = Random.insideUnitSphere * range;
            randomDirection += origin;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
            {
                return hit.position;
            }

            // Fallback: return the original position if no valid point found
            return origin;
        }

        private void OnStateEnter(slimeState  state)
        {
            switch (state)
            {
                case slimeState.Idle:
                    break;
                case slimeState.Walk:
                    _agent.SetDestination(GetRandomPoint(transform.position, 20));
                    break;
            }
            
        }

        private void OnStateUpdate(slimeState state) 
        {
            switch (state)
            {
                case slimeState.Idle:
                    _waitingTimer -= Time.deltaTime;
                    if (_waitingTimer <= 0)
                    {
                        _waitingTimer = Random.Range(2f, 5f);
                        SwitchState(slimeState.Walk);
                    }
                    break;
                case slimeState.Walk:
                    if (_agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        SwitchState(slimeState.Idle);
                    }
                    break;
            }
        }

        private void OnStateExit(slimeState  state)
        {
            switch (state)
            {
                case slimeState.Idle:
                    break;
                case slimeState.Walk:
                    break;
            }
        }

        private void SwitchState(slimeState state)
        {
            OnStateExit(_currentState);
            _currentState = state;
            OnStateEnter(state);
        }
    }
}
