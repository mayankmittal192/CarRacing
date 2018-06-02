using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a path AI managing component for defining and maintaining various track levels in a racing game/simulation. 
/// It has built in design for aiding in quick development and debugging of the path nodes. This is the top-most component 
/// in the path AI system hierarchy. Each object of this class would represent a different path. It defines how many and what 
/// kind of lanes are available for this path, which in turn, define all the way points lying on that lane corresponding to 
/// each node of this path. This script is attached to an empty game object having child game objects treated as nodes.
/// </summary>
public class Path : MonoBehaviour
{
    // Path node structure
    public struct Node
    {
        public Vector3 position { get; private set; }
        public Vector3 forward { get; private set; }
        public Vector3 right { get; private set; }
        public Vector3 up { get; private set; }

        public Node(Transform node)
        {
            position = node.position;
            forward = node.right;
            right = -node.forward;
            up = node.up;

            // Note: Wierd directions??? No. Because by mistake, path nodes were designed like this 
            // from the beginning and now altering them would consume a lot of time and energy.
        }
    }


    // Node color variables for easy debugging
    public Color pathColor;
    public Color laneColor;
    public Color handleColor;

    // ApplyChanges variable for telling the path to apply and render the new changes if any
    public bool applyChanges;

    // Whether or not to draw this path in the editor
    public bool drawInEditor;

    // Lane components list
    public List<Lane> lanes;

    // Path nodes list
    private List<Node> nodes = new List<Node>();


    // Draw path nodes for easy debugging
    public void OnDrawGizmos()
    {
        if (drawInEditor)
        {
            // draw path nodes by drawing a line connecting every consecutive node pairs
            List<Node> pathNodes = getNodes();

            for (int i = 0; i < pathNodes.Count; i++)
            {
                Vector3 currentNode = pathNodes[i].position;
                Vector3 previousNode = Vector3.zero;

                if (i == 0 && pathNodes.Count > 1)                          // for the very first node as current node
                    previousNode = pathNodes[pathNodes.Count - 1].position; // choose the last node as the previous node
                else
                    previousNode = pathNodes[i - 1].position;               // otherwise choose prior node as previous node

                Gizmos.color = pathColor;
                Gizmos.DrawLine(previousNode, currentNode);
                Gizmos.DrawWireSphere(currentNode, 1.0f);                   // draw a wireframe sphere of radius 1 unit at each node
            }
        }
    }


    // Draw path lanes when the object this script is attached to is selected
    public void OnDrawGizmosSelected()
    {
        // draw lane segments only if their drawInEditor variable is checked
        foreach (Lane lane in lanes)
        {
            if (lane.drawInEditor)
            {
                List<Segment> laneSegments = lane.getSegments(nodes, applyChanges);

                for (int i = 0; i < laneSegments.Count; i++)
                {
                    Vector3 startPoint = laneSegments[i].startWayPoint.position;
                    Vector3 endPoint = laneSegments[i].endWayPoint.position;

                    Gizmos.color = laneColor;
                    Gizmos.DrawLine(startPoint, endPoint);
                    Gizmos.DrawWireSphere(startPoint, 1.0f);        // draw a wireframe sphere of radius 1 unit at each way point

                    Gizmos.color = handleColor;
                    Vector3 segmentHandle = laneSegments[i].handlePoint;
                    Gizmos.DrawWireSphere(segmentHandle, 1.0f);     // draw a wireframe sphere of radius 1 unit at each handle point
                }
            }
        }
    }


    // Returns an array of string containing lane names
    public string[] getLaneOptions()
    {
        string[] laneNames = new string[lanes.Count];

        for (int i = 0; i < lanes.Count; i++)
            laneNames[i] = lanes[i].name;

        return laneNames;
    }


    // Returns the count of number of nodes
    public int getNodesCount()
    {
        nodes = getNodes();
        return nodes.Count;
    }


    // Returns a list of segment corresponding to a certain lane
    public List<Segment> getSegments(int laneIndex)
    {
        nodes = getNodes();
        List<Segment> segments = lanes[laneIndex].getSegments(nodes, false);
        return segments;
    }


    // Define and returns list of all the node children of this path
    private List<Node> getNodes()
    {
        if (nodes.Count == 0 || applyChanges)
        {
            nodes = new List<Node>();

            Transform[] pathTransforms = GetComponentsInChildren<Transform>();

            foreach (Transform pathTransform in pathTransforms)
            {
                if (pathTransform != transform)
                    nodes.Add(new Node(pathTransform));
            }
        }

        return nodes;
    }
}
