using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    // Variables
    public Color pathColor;
    public Color wayPointColor;
    public Color segmentHandleColor;
    public bool refresh;
    
    private List<Transform> nodes = new List<Transform>();
    private List<Segment> segments = new List<Segment>();


    // Draw gizmo lines when the object this script is attache on is selected
    public void OnDrawGizmosSelected()
    {
        List<Transform> childNodes = getNodes();

        for (int i = 0; i < childNodes.Count; i++)
        {
            Vector3 currentNode = childNodes[i].position;
            Vector3 previousNode = Vector3.zero;

            if (i == 0 && childNodes.Count > 1)
                previousNode = childNodes[childNodes.Count - 1].position;
            else
                previousNode = childNodes[i - 1].position;

            Gizmos.color = pathColor;
            Gizmos.DrawLine(previousNode, currentNode);
            Gizmos.DrawWireSphere(currentNode, 1.0f);
        }

        Lane lane = new Lane();
        lane.type = Lane.Type.NonPassing;
        List<Segment> pathSegments = getSegments(lane);

        for (int i = 0; i < pathSegments.Count; i++)
        {
            Vector3 startPoint = pathSegments[i].startWayPoint.position;
            Vector3 endPoint = pathSegments[i].endWayPoint.position;

            Gizmos.color = wayPointColor;
            Gizmos.DrawLine(startPoint, endPoint);
            Gizmos.DrawWireSphere(startPoint, 1.0f);

            Gizmos.color = segmentHandleColor;
            Vector3 segmentHandle = pathSegments[i].handlePoint;
            Gizmos.DrawWireSphere(segmentHandle, 1.0f);
        }
    }

    public List<Transform> getNodes()
    {
        if (nodes.Count == 0 || refresh)
        {
            nodes = new List<Transform>();

            Transform[] pathTransforms = GetComponentsInChildren<Transform>();

            foreach (Transform pathTransform in pathTransforms)
            {
                if (pathTransform != transform)
                    nodes.Add(pathTransform);
            }
        }

        return nodes;
    }

    public List<Segment> getSegments(Lane lane)
    {
        if (segments.Count == 0 || refresh)
        {
            segments = new List<Segment>();

            List<Transform> childNodes = getNodes();
            List<WayPoint> wayPoints = new List<WayPoint>();

            foreach (Transform node in childNodes)
            {
                WayPoint wayPoint = new WayPoint(node, lane);
                wayPoints.Add(wayPoint);
            }

            Segment segment;

            for (int i = 0; i < wayPoints.Count - 1; i++)
            {
                segment = new Segment(wayPoints[i], wayPoints[i + 1]);
                segments.Add(segment);
            }

            segment = new Segment(wayPoints[wayPoints.Count - 1], wayPoints[0]);
            segments.Add(segment);
        }

        return segments;
    }
}
