using UnityEngine;
using Automata.Utility;

public abstract class CharacterBehaviour : MonoBehaviour
{
    protected abstract StateMachine stateMachine { get; set; }

    protected abstract void Awake();

    protected abstract void Update();
}