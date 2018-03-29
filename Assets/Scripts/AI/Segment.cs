using UnityEngine;

/// <summary>
/// This is a path AI managing component for defining and maintaining various segments in a particular lane of a racing track 
/// level. This is the bottom-most component in the path AI system hierarchy. Each object of this class would represent a 
/// segment of a particular lane containing a pair of way points which guide the AI vehicle trajectory.
/// </summary>
public class Segment
{
    // Handle point should not be calculated in the usual way if the angle between 
    // the starting point and ending point is less than this constant variable.
    private const float MINIMUM_ANGLE_DIFFERENCE = 5.0f;

    // Way points - starting and ending
    public Lane.WayPoint startWayPoint { get; private set; }
    public Lane.WayPoint endWayPoint { get; private set; }
    
    // Handle point of this segment
    public Vector3 handlePoint { get; private set; }


    // Constructor
    public Segment(Lane.WayPoint start, Lane.WayPoint end)
    {
        startWayPoint = start;
        endWayPoint = end;

        // calculate the handle point in its usual way only if the angle difference between the 
        // starting and ending way points is greater than the minimum allowed angle difference value.
        float angleDifference = Vector3.Angle(startWayPoint.direction, endWayPoint.direction);
        if (angleDifference > MINIMUM_ANGLE_DIFFERENCE)
            handlePoint = GetIntersectingPoint(startWayPoint, endWayPoint);
        else
            handlePoint = (startWayPoint.position + endWayPoint.position) / 2;
    }


    // Get a point on this segment's imaginary line segment (connecting the starting and ending way points) 
    // according to a parametric variable. Value of this parametric variable is in the range between 0 and 1, 
    // mapping 0 to the starting way point, 1 to the ending way point and any other intermediate value to its 
    // scaled counterpart on that imaginary line segment. (For instance 0.5 maps to the exact middle point of 
    // the line segment)
    public Vector3 getLinearParametricPoint(float t)
    {
        return Vector3.Lerp(startWayPoint.position, endWayPoint.position, t);
    }


    // Get a point on this segment's imaginary smooth curve according to a parametric variable. 
    // Value of this parametric variable is in the range between 0 and 1, mapping 0 to the 
    // starting way point, 1 to the ending way point and any other intermediate value to its 
    // scaled counterpart on that imaginary curve. (For instance 0.5 maps to the exact middle 
    // point of the curve)
    public Vector3 getBiLinearParametricPoint(float t)
    {
        Vector3 lerp1 = Vector3.Lerp(startWayPoint.position, handlePoint, t);
        Vector3 lerp2 = Vector3.Lerp(handlePoint, endWayPoint.position, t);
        return Vector3.Lerp(lerp1, lerp2, t);
    }


    // Calculate and returns the handle point of the segment defined as the 
    // intersecting point of the starting and ending way points on the x-z plane.
    private Vector3 GetIntersectingPoint(Lane.WayPoint p1, Lane.WayPoint p2)
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
