using UnityEngine;
using UnityEngine.AI;
using Automata.Utility;

public class Enemy : CharacterBehaviour
{
    [SerializeField] private Waypoint waypoint = null;
    private NavMeshAgent navMeshAgent = null;
    protected sealed override StateMachine stateMachine { get; set; }

    protected sealed override void Awake()
    {
        // set properties
        stateMachine = new StateMachine();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // don't do anything when not initialized properly
        if (stateMachine != null && waypoint && navMeshAgent)
        {
            stateMachine.SetState<PatrolState>(new PatrolState(this, waypoint, navMeshAgent));
        }
    }

    protected sealed override void Update()
    {
        // don't do anything when not initialized properly
        if (stateMachine != null)
        {
            stateMachine.OnUpdate();
        }

        // line of sight code here
            // if player is line of sight, switch state to attack state
    }
}