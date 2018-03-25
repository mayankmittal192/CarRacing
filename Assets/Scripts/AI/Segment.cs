using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment
{
    private const float ANGLE_DIFFERENCE_THRESHOLD = 5.0f;

    public WayPoint startWayPoint { get; private set; }
    public WayPoint endWayPoint { get; private set; }
    public Vector3 handlePoint { get; set; }


    public Segment(WayPoint start, WayPoint end)
    {
        startWayPoint = start;
        endWayPoint = end;

        float angleDifference = Vector3.Angle(startWayPoint.direction, endWayPoint.direction);
        if (angleDifference > ANGLE_DIFFERENCE_THRESHOLD)
            handlePoint = GetIntersectingPoint(startWayPoint, endWayPoint);
        else
            handlePoint = (startWayPoint.position + endWayPoint.position) / 2;
    }

    public Vector3 getParametricPoint(float t)
    {
        Vector3 lerp1 = Vector3.Lerp(startWayPoint.position, handlePoint, t);
        Vector3 lerp2 = Vector3.Lerp(handlePoint, endWayPoint.position, t);
        return Vector3.Lerp(lerp1, lerp2, t);
    }

    private Vector3 GetIntersectingPoint(WayPoint p1, WayPoint p2)
    {
        float a1 = p1.direction.z;
        float b1 = -p1.direction.x;
        float c1 = -(a1 * p1.position.x + b1 * p1.position.z);

        float a2 = p2.direction.z;
        float b2 = -p2.direction.x;
        float c2 = -(a2 * p2.position.x + b2 * p2.position.z);

        float numerX = b1 * c2 - b2 * c1;
        float numerZ = a1 * c2 - a2 * c1;
        float denom = a1 * b2 - a2 * b1;

        Vector3 point = Vector3.zero;
        point.x = (numerX / denom);
        point.z = -(numerZ / denom);
        point.y = (p1.position.y + p2.position.y) / 2;

        return point;
    }
}
