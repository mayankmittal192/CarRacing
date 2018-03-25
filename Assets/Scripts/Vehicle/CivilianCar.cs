using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CivilianCar : Car
{
    public Path path;
    public Lane lane;
    private List<Segment> segments;
    private int currentSegment;
    private float t;


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        segments = path.getSegments(lane);
        currentSegment = 15;
        t = 0;

        Segment currSegment = segments[currentSegment];
        car.position = currSegment.startWayPoint.position;
        car.rotation = Quaternion.LookRotation(currSegment.startWayPoint.direction);
    }

    protected override void Update()
    {
        base.Update();
        float forwardSpeed = Vector3.Dot(body.velocity, car.forward);
        //Debug.Log("Speed: " + (int)(forwardSpeed * 2.237f) + " mph");
        Debug.Log("Speed: " + (int)(forwardSpeed * 3.6f) + " kmph");
    }

    protected override void FixedUpdate()
    {
        updateParameter();
        applyParameter();
    }

    private void updateParameter()
    {
        t += Time.fixedDeltaTime * 0.7f;

        if (t > 1)
        {
            proceedToNextSegment();
            t -= 1;
        }
    }

    private void proceedToNextSegment()
    {
        currentSegment++;

        if (currentSegment == segments.Count)
            currentSegment = 0;
    }

    private void applyParameter()
    {
        // Translation
        Vector3 desiredPosition = segments[currentSegment].getParametricPoint(t) + car.up * 0.8f;
        Vector3 desiredVelocity = desiredPosition - car.position;
        body.velocity = desiredVelocity;

        // Rotation
        Vector3 desiredForwardDir = Vector3.ProjectOnPlane(body.velocity, car.up);
        body.rotation = Quaternion.LookRotation(desiredForwardDir, car.up);
    }
}
