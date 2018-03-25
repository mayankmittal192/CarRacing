using UnityEngine;

public class WayPoint
{
    public Vector3 position { get; private set; }
    public Vector3 direction { get; private set; }


    public WayPoint(Transform node, Lane lane)
    {
        position = node.position + node.forward * lane.distance(lane.type);
        direction = node.right;
    }
}
