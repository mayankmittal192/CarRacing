using System.Collections.Generic;
using UnityEngine;

public class CivilianCar : Car
{
    // Vehicle Constants
    private const float VEHICLE_HALF_HEIGHT = 0.8f;     // half of car's height
    private const float SPEED_TURN_THRESHOLD = 0.1f;    // minimum speed required for car to be able to make turn
    private const float SPEED_PARAMETER_FACTOR = 130f;  // speed parameter

    // Wheel Motion Graphics Simulation Constants
    private const float TURN_RESISTANCE_FACTOR = 0.98f; // how fast should car react to steering forces; greater the value, 
                                                        // more will be the resistance; value in range 0 to 1; 0 being no 
                                                        // resistance and 1 is no steering at all
    private const float STEERING_FACTOR = 4.0f;         // steering angle multiplier for wheel steering graphics simulation

    // Path variable which it will use
    public Path path;

    // Current segment variable from which it will start its journey
    public int currentSegment;

    // Available lane options under the selected path variable
    [HideInInspector]
    public string[] laneOptions;

    // Lane index variable
    [HideInInspector]
    public int laneOptionIdx = 0;

    // Lane segments lists
    private List<Segment> segments;

    // Parametric variable to keep track of its position within a segment
    private float t;

    // Stats variables
    private float distance;
    private float time;

    
    // Gets called when any variable is tweaked in the editor
    public void OnValidate()
    {
        laneOptions = path.getLaneOptions();
        int laneOptionsCount = laneOptions.Length;
        int segmentsCount = path.getNodesCount() - 1;
        currentSegment = Mathf.Clamp(currentSegment, 0, segmentsCount);
    }


    // Use this for pre-initialization
    protected override void Awake()
    {
        base.Awake();
    }


    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        segments = path.getSegments(laneOptionIdx);
        t = 0;

        Segment currSegment = segments[currentSegment];
        car.position = currSegment.startWayPoint.position;
        car.rotation = Quaternion.LookRotation(currSegment.startWayPoint.direction);
    }


    // Gets called once per frame
    protected override void Update()
    {
        base.Update();
        
        int currSpeed = (int)(currentSpeed * MPS_TO_KMPH);
        float distance2 = (int)(distance * 100);
        int minutes = (int)(time / 60);
        int seconds = (int)(time % 60);
        int avgSpeed = (int)((distance / time) * 3600);

        Debug.Log("Speed: " + currSpeed + " kmph    Distance: " + distance2 / 100 + " km    " + 
            "Time: " + minutes + "m " + seconds + "s    Avg Speed: " + avgSpeed + " kmph");
    }


    // Gets called at fixed time intervals
    protected override void FixedUpdate()
    {
        distance += (currentSpeed * MPS_TO_KMPH * Time.fixedDeltaTime / 3600);
        time += Time.fixedDeltaTime;

        updateParameter();
        applyParameter();
    }


    // Update the parametric point value and wrap it around the value 1 if required
    private void updateParameter()
    {
        t += Time.fixedDeltaTime * (topSpeed / SPEED_PARAMETER_FACTOR);

        if (t > 1)
        {
            proceedToNextSegment();
            t -= 1;
        }
    }


    // Increase the current segment counter by 1
    private void proceedToNextSegment()
    {
        currentSegment++;

        if (currentSegment == segments.Count)
            currentSegment = 0;
    }

    
    // Apply the parmetric point updated value on the segment curve and thus calculate 
    // the required force to apply in order for the vehicle to achieve the target.
    private void applyParameter()
    {
        // translation
        Vector3 desiredPosition = segments[currentSegment].getBiLinearParametricPoint(t) + car.up * VEHICLE_HALF_HEIGHT;
        Vector3 desiredVelocity = desiredPosition - car.position;
        body.velocity = desiredVelocity;

        // set current speed
        currentSpeed = Vector3.Dot(body.velocity, car.forward);

        // rotation
        if (currentSpeed > SPEED_TURN_THRESHOLD)
        {
            Vector3 desiredForward = Vector3.ProjectOnPlane(body.velocity, car.up);
            Vector3 newForward = car.forward * TURN_RESISTANCE_FACTOR + desiredForward * (1 - TURN_RESISTANCE_FACTOR);
            body.rotation = Quaternion.LookRotation(newForward, car.up);

            // set steering angle
            float angleDifference = Vector3.Angle(car.forward, desiredForward) * STEERING_FACTOR;
            steering = angleDifference * angleDirection(car.forward, desiredForward);
        }
        else
            steering = 0;   // set steering angle to zero if car is not rotating
    }


    // Returns a positive or negative value depending upon the  
    // direction of the vector from 'from' vector to 'to vector
    private float angleDirection(Vector3 from, Vector3 to)
    {
        Vector3 cross = Vector3.Cross(from, to);
        float dot = Vector3.Dot(cross, Vector3.up);
        return Mathf.Sign(dot);
    }
}
