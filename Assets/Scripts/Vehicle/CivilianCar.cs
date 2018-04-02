using System.Collections.Generic;
using UnityEngine;

public class CivilianCar : Car
{
    // Different modes of car for implementing different behaviours in different situations
    public enum Mode
    {
        Cruise, 
        Alert, 
        Draw, 
        Overtake, 
        CrossOvertake, 
        PullOver, 
        Brake, 
        HandBrake
    }


    // Vehicle Constants
    private const float VEHICLE_HALF_HEIGHT = 0.8f;     // half of car's height
    private const float SPEED_TURN_THRESHOLD = 0.1f;    // minimum speed required for car to be able to make turn

    // // Speed Parameter Constants
    private const float SPEED_PARAM_INC_FACTOR = 3500f; // how quickly speed parameter value should increase
    private const float SPEED_PARAM_DEC_FACTOR = 5000f; // how quickly speed parameter value should decrease
    private const float MIN_SPEED_PARAMETER = 16f;      // min speed parameter value
    private const float AVG_SPEED_PARAMETER = 36f;      // avg speed parameter value
    private const float MAX_SPEED_PARAMETER = 64f;      // max speed parameter value

    // Wheel Motion Graphics Simulation Constants
    private const float TURN_RESISTANCE_FACTOR = 0.98f; // how fast should car react to steering forces; greater the value, 
                                                        // more will be the resistance; value in range 0 to 1; 0 being no 
                                                        // resistance and 1 is no steering at all
    private const float STEERING_FACTOR = 4.0f;         // steering angle multiplier for wheel steering graphics simulation

    // Path variable which it will use
    public Path path;

    // Lane variables
    [HideInInspector]
    public string[] laneOptions;    // available lane options under the selected path variable
    [HideInInspector]
    public int laneOptionIdx = 0;   // lane index variable

    // Mode variable
    public Mode mode;

    // Braking variables
    private bool braking;           // whether brakes are applied or not
    private bool handBraking;       // whether handbrakes are applied or not
    private float brakingFactor;    // how hard brakes are applied

    // Lane segment variables
    private List<Segment> segments;     // active lane segments list
    private int currentSegmentIdx;      // current segment variable index
    private Segment currentSegment;     // current segment variable from which it will start its journey
    private Segment transitionSegment1; // segment to be used while changing lane
    private Segment transitionSegment2; // segment to be used while changing lane

    // Parametric variables
    private float speedParameter;   // how quickly the position within a segment should change
    private bool incSpeedParameter; // whether to increase the speed parameter or decrease
    private float t;                // keep track of the position within a segment

    // Stats variables
    //private float distance;
    //private float time;


    // Gets called when any variable is tweaked in the editor
    public void OnValidate()
    {
        laneOptions = path.getLaneOptions();
        int laneOptionsCount = laneOptions.Length;
        int segmentsCount = path.getNodesCount() - 1;
        currentSegmentIdx = Mathf.Clamp(currentSegmentIdx, 0, segmentsCount);
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

        // inititialize braking variables
        braking = false;
        handBraking = false;
        brakingFactor = 1.0f;

        // inititialize lane segment variables
        segments = path.getSegments(laneOptionIdx);
        currentSegmentIdx = 0;
        currentSegment = segments[currentSegmentIdx];
        transitionSegment1 = null;
        transitionSegment2 = null;

        // inititialize paramteric variables
        speedParameter = AVG_SPEED_PARAMETER;
        t = 0;

        // inititialize car position and orientation
        car.position = currentSegment.startWayPoint.position;
        car.rotation = Quaternion.LookRotation(currentSegment.startWayPoint.direction);
    }


    // Gets called once per frame
    protected override void Update()
    {
        base.Update();

        //distance += (currentSpeed * MPS_TO_KMPH * Time.fixedDeltaTime / 3600);
        //time += Time.fixedDeltaTime;

        //Debug.Log("Speed: " + (int)(currentSpeed * MPS_TO_KMPH) + " kmph");

        //int currSpeed = (int)(currentSpeed * MPS_TO_KMPH);
        //float distance2 = (int)(distance * 100);
        //int minutes = (int)(time / 60);
        //int seconds = (int)(time % 60);
        //int avgSpeed = (int)((distance / time) * 3600);

        //Debug.Log("Speed: " + currSpeed + " kmph    Distance: " + distance2 / 100 + " km    " + 
        //    "Time: " + minutes + "m " + seconds + "s    Avg Speed: " + avgSpeed + " kmph");
    }


    // Gets called at fixed time intervals
    protected override void FixedUpdate()
    {
        updateParameter();
        applyParameter();
        adjustParameter();
    }


    // Update the parametric point value and wrap it around the value 1 if required
    private void updateParameter()
    {
        // update the parametric variable's value normally
        float tInc = Time.fixedDeltaTime * (topSpeed / speedParameter);
        
        // apply brakes if braking
        if (braking)
        {
            tInc *= brakingFactor;  // gradually decrease the increment value of parametric variable
            brakingFactor = Mathf.Max(0, brakingFactor - 0.01f);    // decrease the braking factor value if braking
        }
        else
        {
            tInc *= brakingFactor;  // gradually increase the increment value of parametric variable
            brakingFactor = Mathf.Min(1, brakingFactor + 0.01f);    // increase the braking factor value if not braking
        }
        
        // apply handbrakes if handbraking
        if (handBraking)
            tInc = 0;               // don't increase the parametric variable at all
        
        t += tInc;                  // increase the parametric variable by its increment value

        // check if the parametric variable hase exceeded the value 1 
        // and proceed to the nex segment accordingly
        if (t > 1)
        {
            proceedToNextSegment();
            t -= 1;                 // adjust the parametric variable value in the next segment
        }
    }


    // Check and sets the current segment to be equal to the transition segment if the car needs lane transition 
    // but if not then proceeds to the next segment in the active segment list
    private void proceedToNextSegment()
    {
        currentSegmentIdx = (currentSegmentIdx + 1) % segments.Count;

        if (transitionSegment1 != null)
        {
            currentSegment = transitionSegment1;
            transitionSegment1 = null;
        }
        else if (transitionSegment2 != null)
        {
            currentSegment = transitionSegment2;
            transitionSegment2 = null;
        }
        else
            currentSegment = segments[currentSegmentIdx];
    }

    
    // Apply the parmetric point updated value on the segment curve and thus calculate 
    // the required force to apply in order for the vehicle to achieve the target.
    private void applyParameter()
    {
        // translation
        Vector3 desiredPosition = currentSegment.getParametricPoint(t) + car.up * VEHICLE_HALF_HEIGHT;
        Vector3 desiredVelocity = desiredPosition - car.position;
        body.velocity = limitVelocity(desiredVelocity);

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


    // Returns a velocity which should be car's current velocity according 
    // to its acceleration curve and handling parameters
    private Vector3 limitVelocity(Vector3 velocity)
    {
        float desiredSpeed = velocity.magnitude;
        float desiredAcc = desiredSpeed - currentSpeed;
        float maxAcc = accelerationCurve.Evaluate(1 - currentSpeed / topSpeed);

        if (desiredAcc > maxAcc)
        {
            float maxCurrSpeedRatio = (currentSpeed + maxAcc) / desiredSpeed;
            velocity.Scale(new Vector3(maxCurrSpeedRatio, maxCurrSpeedRatio, maxCurrSpeedRatio));
            incSpeedParameter = true;   // adjust speed parameter
        }
        else
        {
            if (mode != Mode.Alert)
            {
                if (desiredAcc < -maxAcc)
                {
                    float maxCurrSpeedRatio = (currentSpeed - maxAcc) / desiredSpeed;
                    velocity.Scale(new Vector3(maxCurrSpeedRatio, maxCurrSpeedRatio, maxCurrSpeedRatio));
                }
            }

            incSpeedParameter = false;  // adjust speed parameter
        }

        return velocity;
    }

    
    // Adjusts the speed parameter according to which mode the car is in
    private void adjustParameter()
    {
        switch (mode)
        {
            case Mode.Cruise:   // adjust the value dynamically within the min and max limit bounds
                {
                    if (incSpeedParameter)
                        speedParameter += (currentSpeed * topSpeed) / SPEED_PARAM_INC_FACTOR;
                    else
                    {
                        speedParameter -= (currentSpeed * topSpeed) / SPEED_PARAM_DEC_FACTOR;
                        speedParameter = Mathf.Max(speedParameter, MIN_SPEED_PARAMETER);
                    }

                    break;
                }

            case Mode.Alert:    // adjust the value to gradually bring it to the average value
                {
                    if (speedParameter < AVG_SPEED_PARAMETER)
                        speedParameter += (currentSpeed * topSpeed) / SPEED_PARAM_INC_FACTOR;
                    else
                        speedParameter -= (currentSpeed * topSpeed) / SPEED_PARAM_DEC_FACTOR;

                    break;
                }
        }
    }


    // Returns a positive or negative value depending upon the 
    // direction of the vector from 'from' vector to 'to vector
    private float angleDirection(Vector3 from, Vector3 to)
    {
        Vector3 cross = Vector3.Cross(from, to);
        float dot = Vector3.Dot(cross, Vector3.up);
        return Mathf.Sign(dot);
    }


    // Changes the lane to passing if driving on non-passing or vice-versa
    public void changeLane()
    {
        if (laneOptionIdx == 0 || laneOptionIdx == 2)
            laneOptionIdx += 1;
        else
            laneOptionIdx -= 1;

        Lane.WayPoint start = currentSegment.endWayPoint;
        segments = path.getSegments(laneOptionIdx);
        int nextSegmentIdx = (currentSegmentIdx + 2) % segments.Count;
        Lane.WayPoint end = segments[nextSegmentIdx].endWayPoint;

        Vector3 diffDir = (end.position - start.position);
        Vector3 startUp = Vector3.Cross(diffDir, start.direction);
        Vector3 midDir = Vector3.Cross(start.direction, startUp);
        Lane.WayPoint mid = new Lane.WayPoint((start.position + end.position) / 2, midDir);
        
        transitionSegment1 = new Segment(start, mid);
        transitionSegment2 = new Segment(end, end);
    }
}
