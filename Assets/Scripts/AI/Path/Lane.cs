using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a path AI managing component for defining and maintaining various lanes in a particular racing track level. 
/// This component lies on the middle tier in the path AI system hierarchy. Each object of this class would represent a 
/// lane of a particular path containing a list of lane segments which in turn contain a pair of way points needed to guide 
/// the AI vehicle trajectory.
/// </summary>
[System.Serializable]
public class Lane
{
    // Lane way point structure
    public struct WayPoint
    {
        public Vector3 position { get; private set; }
        public Vector3 direction { get; private set; }
        
        public WayPoint(Vector3 pos, Vector3 dir)
        {
            position = pos;
            direction = dir;
        }

        public WayPoint(Lane lane, Path.Node node)
        {
            if (lane.direction == Direction.Left)
                position = node.position - node.right * lane.distance;
            else
                position = node.position + node.right * lane.distance;

            if (lane.movement == Movement.Up)
                direction = node.forward;
            else
                direction = -node.forward;
        }
    }


    // Lane direction to determine which side of divider would this lane lie
    public enum Direction
    {
        Left,       // left side of divider
        Right       // right side of divider
    }

    // Lane movement to determine which way would vehicles move on this lane
    public enum Movement
    {
        Up,         // same way as the path nodes are structured
        Down        // opposite way of the path nodes structure
    }


    // Lane name variable
    public string name;
    
    // Lane direction variable
    public Direction direction;

    // Lane movement variable
    public Movement movement;

    // Distance variable of this lane from path nodes
    public float distance;

    // Whether or not to draw this lane in the editor
    public bool drawInEditor;

    // Lane segments list
    private List<Segment> segments = new List<Segment>();
    

    // Get the segments list of this lane
    public List<Segment> getSegments(List<Path.Node> pathNodes, bool applyChanges)
    {
        if (segments.Count == 0 || applyChanges)
        {
            // clear the segments list first
            segments.Clear();

            // construct a way points list for this lane
            List<WayPoint> wayPoints = new List<WayPoint>();

            for (int i = 0; i < pathNodes.Count; i++)
            {
                // select the nodes in reverse order if the movement of this lane is down
                int nodeIdx = (movement == Movement.Up) ? i : (pathNodes.Count - 1) - i;
                WayPoint wayPoint = new WayPoint(this, pathNodes[nodeIdx]);
                wayPoints.Add(wayPoint);
            }

            // add segments in the segments list
            for (int i = 0; i < wayPoints.Count; i++)
            {
                WayPoint startWayPoint = wayPoints[i];
                WayPoint endWayPoint = startWayPoint;

                if (i == wayPoints.Count - 1)           // for the very last way point as starting point of a segment
                    endWayPoint = wayPoints[0];         // choose the first way point as the ending point of that segment
                else
                    endWayPoint = wayPoints[i + 1];     // otherwise choose the next way point as the ending point of that segment

                Segment segment = new Segment(startWayPoint, endWayPoint);
                segments.Add(segment);
            }
        }

        return segments;
    }
}
