using UnityEngine;

public class Waypoint : MonoBehaviour
{
    // editor
    [SerializeField] private Waypoint next = null;
    [SerializeField] private Waypoint previous = null;
    [SerializeField] private float radius = 1.0f;

    // exposed properties
    public Waypoint Next { get; private set; }
    public Waypoint Previous { get; private set; }
    public float Radius { get; private set; }

    private void Awake()
    {
        // set exposed properties
        Next = next;
        Previous = previous;
        Radius = radius;
    }
}