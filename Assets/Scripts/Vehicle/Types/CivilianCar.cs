using System.Collections.Generic;
using UnityEngine;

public class CivilianCar : Car
{
    // Vehicle Constants
    private const float VEHICLE_HALF_HEIGHT = 0.8f;         // half of car's height
    private const float SPEED_TURN_THRESHOLD = 0.1f;        // minimum speed required for car to be able to make turn

    // Speed Parameter Constants
    private const float SPEED_PARAM_INC_FACTOR = 3.5f;      // how quickly speed parameter value should increase
    private const float SPEED_PARAM_DEC_FACTOR = 3.5f;      // how quickly speed parameter value should decrease
    private const float MIN_SPEED_PARAMETER = 15f;          // min speed parameter value
    private const float AVG_SPEED_PARAMETER = 45f;          // avg speed parameter value
    private const float MAX_SPEED_PARAMETER = 65f;          // max speed parameter value

    // Vehicle Motion Simulation Constants
    private const float BRAKING_FACTOR = 0.2f;              // braking force constant for car to apply deceleration

    // Wheel Motion Graphics Simulation Constants
    private const float TURN_RESISTANCE_FACTOR = 0.985f;    // how fast should car react to steering forces; greater the value, 
                                                            // more will be the resistance; value in range 0 to 1; 0 being no 
                                                            // resistance and 1 is no steering at all
    private const float STEERING_FACTOR = 5.0f;             // steering angle multiplier for wheel steering graphics simulation

    // Path variable which it will use
    public Path path;

    // Mode variable
    public Mode mode;

    // Lane variables
    [HideInInspector]
    public string[] laneOptions;        // available lane options under the selected path variable
    [HideInInspector]
    public int laneIndex = 0;           // lane index variable

    // Braking variables
    public bool braking;                // whether brakes are applied or not
    private bool handbraking;           // whether handbrakes are applied or not
    public float brakingFactor;         // how hard brakes are applied

    // Lane segment variables
    private List<Segment> segments;     // active lane segments list
    public int currentSegmentIndex;     // current segment variable index
    private Segment currentSegment;     // current segment variable from which it will start its journey
    private Segment transitionSegment1; // segment to be used while changing lane
    private Segment transitionSegment2; // segment to be used while changing lane

    // Parametric variables
    private float speedParameter;       // how quickly the position within a segment should change
    private bool incSpeedParameter;     // whether to increase the speed parameter or decrease it
    private float t;                    // keep track of the position within a segment

    // Cached top speed variable for switching from draw mode to other modes
    private int cachedTopSpeed;

    // Changing lane property to indicate whether or not this car is currently between lanes
    public bool betweenLanes { get; private set; }

    // Stats variables
    //private float distance;
    //private float time;


    // Gets called when any variable is tweaked in the editor
    public void OnValidate()
    {
        laneOptions = path.getLaneOptions();
        int laneOptionsCount = laneOptions.Length;
        int segmentsCount = path.getNodesCount() - 1;
        currentSegmentIndex = Mathf.Clamp(currentSegmentIndex, 0, segmentsCount);
    }


    // Use this for pre-initialization
    protected override void Awake()
    {
        base.Awake();
        mode = new Mode(this);
    }


    // Use this for initialization
    protected override void Start()
    {
        base.Start();

        // inititialize braking variables
        braking = false;
        handbraking = false;
        brakingFactor = 1.0f;

        // inititialize lane segment variables
        //laneIndex = Random.Range(0, laneOptions.Length - 1);
        segments = path.getSegments(laneIndex);
        //currentSegmentIndex = Random.Range(0, segments.Count - 1);
        currentSegment = segments[currentSegmentIndex];
        transitionSegment1 = null;
        transitionSegment2 = null;

        // inititialize paramteric variables
        speedParameter = AVG_SPEED_PARAMETER;
        t = 0;

        // set cached top speed value
        //topSpeed = Random.Range(20, 35);
        cachedTopSpeed = topSpeed;

        // set the between lanes property
        betweenLanes = false;

        // inititialize car position and orientation
        car.position = currentSegment.startWayPoint.position;
        car.rotation = Quaternion.LookRotation(currentSegment.startWayPoint.direction);
    }


    // Gets called once per frame
    protected override void Update()
    {
        base.Update();
        mode.update();
        stabilizeCar();

        //Debug.Log(gameObject.name);
        //Debug.Log(laneIndex);

        //distance += (currentSpeed * MPS_TO_KMPH * Time.fixedDeltaTime / 3600);
        //time += Time.fixedDeltaTime;

        //if (gameObject.name == "CivilianCar")
        //    Debug.Log("Speed: " + (int)(currentSpeed) + " mps");

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
        if (handbraking)
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


    // Check and sets the current segment to be equal to the transition segment if the car needs 
    // lane transition but if not then proceeds to the next segment in the active segment list
    private void proceedToNextSegment()
    {
        currentSegmentIndex = (currentSegmentIndex + 1) % segments.Count;

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
        {
            currentSegment = segments[currentSegmentIndex];
            betweenLanes = false;
        }
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
            incSpeedParameter = true;       // adjust speed parameter
        }
        else
        {
            if (desiredAcc < -BRAKING_FACTOR)
            {
                float maxCurrSpeedRatio = (currentSpeed - BRAKING_FACTOR) / currentSpeed;
                velocity.Scale(new Vector3(maxCurrSpeedRatio, maxCurrSpeedRatio, maxCurrSpeedRatio));
            }

            incSpeedParameter = false;      // adjust speed parameter
        }

        return velocity;
    }


    // Returns a positive or negative value depending upon the 
    // direction of the vector from 'from' vector to 'to vector
    private float angleDirection(Vector3 from, Vector3 to)
    {
        Vector3 cross = Vector3.Cross(from, to);
        float dot = Vector3.Dot(cross, Vector3.up);
        return Mathf.Sign(dot);
    }


    // Adjusts the speed parameter according to which mode the car is in
    private void adjustParameter()
    {
        switch (mode.type)
        {
            case Mode.Type.Cruise:
                {
                    if (onPassingLane())
                        speedParameterDynamicAdjustment();
                    else
                    {
                        stabilizeSpeedParameter();

                        if (isSpeedParameterStable())
                            changeLane();
                    }

                    topSpeed = cachedTopSpeed;
                    braking = false;

                    break;
                }

            case Mode.Type.Alert:
                {
                    stabilizeSpeedParameter();
                    topSpeed = cachedTopSpeed;
                    break;
                }

            case Mode.Type.PullOver:
                {
                    stabilizeSpeedParameter();
                    topSpeed = cachedTopSpeed;

                    if (isSpeedParameterStable())
                        changeLane();

                    break;
                }

            case Mode.Type.Pass:
                {
                    stabilizeSpeedParameter();
                    topSpeed = cachedTopSpeed;
                    break;
                }

            case Mode.Type.Overtake:
                {
                    stabilizeSpeedParameter();
                    topSpeed = cachedTopSpeed;
                    braking = false;
                    break;
                }

            case Mode.Type.Draw:
                {
                    stabilizeSpeedParameter();
                    topSpeed = mode.proximaCar.topSpeed + 1;

                    if (currentSpeed > mode.proximaCar.currentSpeed)
                        braking = true;
                    else
                        braking = false;

                    break;
                }

            case Mode.Type.Transition:
                {
                    stabilizeSpeedParameter();
                    topSpeed = cachedTopSpeed;
                    break;
                }
        }
    }


    // Dynamically adjusts the speed parameter value
    private void speedParameterDynamicAdjustment()
    {
        if (incSpeedParameter)
        {
            speedParameter += (currentSpeed / topSpeed) / SPEED_PARAM_INC_FACTOR;
            speedParameter = Mathf.Min(speedParameter, MAX_SPEED_PARAMETER);
        }
        else
        {
            speedParameter -= (currentSpeed / topSpeed) / SPEED_PARAM_DEC_FACTOR;
            speedParameter = Mathf.Max(speedParameter, MIN_SPEED_PARAMETER);
        }
    }


    // Stabilizes the speed parameter value
    private void stabilizeSpeedParameter()
    {
        // adjust the value to gradually bring it to the average value
        if (speedParameter < AVG_SPEED_PARAMETER)
            speedParameter += (currentSpeed / topSpeed) / SPEED_PARAM_INC_FACTOR;
        else
            speedParameter -= (currentSpeed / topSpeed) / SPEED_PARAM_DEC_FACTOR;
    }


    // Checks and return the status of whether the speed parameter value is near stable or not
    private bool isSpeedParameterStable()
    {
        if (Mathf.Abs(speedParameter - AVG_SPEED_PARAMETER) < 0.1f)
            return true;
        else
            return false;
    }


    // Stabilizes the car's orientation
    private void stabilizeCar()
    {
        if (onPrimaryRoute())
        {
            if (currentSegmentIndex > 165 && currentSegmentIndex < 170 ||
                currentSegmentIndex > 190 && currentSegmentIndex < 195 ||
                currentSegmentIndex > 210 && currentSegmentIndex < 215)
            {
                stabilizePosition();
                stabilizeOrientation();
            }
        }
        else
        {
            if (currentSegmentIndex > 35 && currentSegmentIndex < 45)
            {
                stabilizePosition();
                stabilizeOrientation();
            }
        }
    }


    // Stabilizes position of the car
    private void stabilizePosition()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;
        transform.position = new Vector3(x, y - 0.01f, z);
    }


    // Stabilizes orientation of the car
    private void stabilizeOrientation()
    {
        float xA = transform.localEulerAngles.x;
        float yA = transform.localEulerAngles.y;
        float zA = transform.localEulerAngles.z;
        transform.localEulerAngles = new Vector3(xA - 0.2f, yA, zA);
    }


    // Changes the lane to passing if driving on non-passing or vice-versa
    private void changeLane()
    {
        if (laneIndex == 0 || laneIndex == 2)   // first lane of current route
            laneIndex += 1;
        else
            laneIndex -= 1;

        Lane.WayPoint start = currentSegment.endWayPoint;
        segments = path.getSegments(laneIndex);
        int nextSegmentIdx = (currentSegmentIndex + 2) % segments.Count;
        Lane.WayPoint end = segments[nextSegmentIdx].endWayPoint;

        Vector3 diffDir = (end.position - start.position);
        Vector3 startUp = Vector3.Cross(diffDir, start.direction);
        Vector3 midDir = Vector3.Cross(start.direction, startUp);
        Lane.WayPoint mid = new Lane.WayPoint((start.position + end.position) / 2, midDir);

        transitionSegment1 = new Segment(start, mid);
        transitionSegment2 = new Segment(end, end);

        betweenLanes = true;
        mode.setToTransition();
    }


    // Returns whether or not the car is currently on a passing lane
    private bool onPassingLane()
    {
        return (laneIndex == 1 || laneIndex == 2);
    }


    // Returns whether or not the car is currently on the primary route
    private bool onPrimaryRoute()
    {
        return (laneIndex == 0 || laneIndex == 1);
    }


    // Returns whether or not the given car is currently on the same route as that of this car
    public bool onSameRoute(CivilianCar otherCar)
    {
        bool thisPrimaryRoute = onPrimaryRoute();
        bool otherPrimaryRoute = otherCar.onPrimaryRoute();

        if (thisPrimaryRoute != otherPrimaryRoute)
            return false;
        else
            return true;
    }


    // Returns the completion status of this car in terms of segment index
    private float getCompletionIndex()
    {
        int currPosSegmentIdx = (currentSegmentIndex + 1) % segments.Count;
        Vector3 segmentVector;
        float currPosDot = 0;

        do
        {
            currPosSegmentIdx = (currPosSegmentIdx + segments.Count - 1) % segments.Count;
            Segment currPosSegment = segments[currPosSegmentIdx];
            segmentVector = currPosSegment.endWayPoint.position - currPosSegment.startWayPoint.position;
            Vector3 currPosVector = car.position - currPosSegment.startWayPoint.position;
            currPosDot = Vector3.Dot(currPosVector, segmentVector);
        }
        while (currPosDot < 0);

        float segmentFraction = currPosDot / segmentVector.sqrMagnitude;

        return (currPosSegmentIdx + segmentFraction) % segments.Count;
    }


    // Return the position difference of a certain other car as compared to this car; 
    // positive value indicates that this car is in front of the other car
    // negative value indicates that the other car is in front of this car
    public float compareCompletionIndex(CivilianCar otherCar)
    {
        float index1 = getCompletionIndex();
        float index2 = otherCar.getCompletionIndex();
        float indexDiff = (index1 - index2 + (segments.Count / 2)) % segments.Count;
        indexDiff -= (segments.Count / 2);
        return indexDiff;
    }


    // Register this car in negotiation phase queue
    private void OnTriggerEnter(Collider other)
    {
        CivilianCar otherCar = other.gameObject.GetComponent<CivilianCar>();
        if (otherCar != null)
            mode.onEnter(otherCar);
    }


    // Deregister this car from negotiation phase queue
    private void OnTriggerExit(Collider other)
    {
        CivilianCar otherCar = other.gameObject.GetComponent<CivilianCar>();
        if (otherCar != null)
            mode.onExit(otherCar);
    }
}
