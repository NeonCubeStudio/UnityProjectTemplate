using UnityEngine;
using UnityEngine.AI;
using Automata.Utility;

public class PatrolState : State
{
    private CharacterBehaviour characterBehaviour = null;
    private Waypoint waypoint = null;
    private NavMeshAgent navMeshAgent = null;

    public PatrolState(CharacterBehaviour newCharacterBehaviour, Waypoint newWaypoint, NavMeshAgent newNavMeshAgent)
    {
        // set properties
        characterBehaviour = newCharacterBehaviour;
        waypoint = newWaypoint;
        navMeshAgent = newNavMeshAgent;
    }

    public sealed override void OnUpdate()
    {
        // don't do anything when not initialized properly
        if (characterBehaviour && waypoint && navMeshAgent)
        {
            // go to the waypoint
            navMeshAgent.SetDestination(waypoint.transform.position);

            // set the next waypoint as target when inside the waypoint
            float distance = Vector3.Distance(waypoint.transform.position, characterBehaviour.transform.position);

            if (distance <= waypoint.Radius)
            {
                waypoint = waypoint.Next;
            }
        }
    }

    public sealed override void OnEnter()
    {
        // code here
    }

    public sealed override void OnExit()
    {
        // code here
    }
}