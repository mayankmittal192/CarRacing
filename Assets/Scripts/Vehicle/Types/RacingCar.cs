using UnityEngine;

public class RacingCar : Car
{
    // Variables 
    public float resetTime;
    private int balanceCheckCounter;
    private float resetTimer;
    
    public KeyboardController keyboardController;
    private PlayerController playerController;

    // Properties
    public float forwardAcceleration { get; private set; }
    public float sidewaysAcceleration { get; private set; }


    // Use this for pre-initialization
    protected override void Awake()
    {
        base.Awake();
        keyboardController = new KeyboardController();
        playerController = new PlayerController();
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        playerController.Setup(keyboardController);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        playerController.Poll(Time.deltaTime);
        StabilizeCar();
    }

    // FixedUpdate is called after each fixed time interval
    protected override void FixedUpdate()
    {
        // compute the vehicle accelerations in both the directions
        CalculateVehicleAcceleration();

        // consume the player input values
        throttle = playerController.Throttle();
        float steer = playerController.Steer();

        float acc = forwardAcceleration * throttle;
        steering = maxSteerAngle * steer;
        bool isReversing = Vector3.Dot(body.velocity, car.forward) < 0;

        foreach (var axle in axleInfo)
        {
            if (axle.steerable)
            {
                if (!isReversing)
                    car.Rotate(car.up, steering * Time.fixedDeltaTime * Mathf.Min(1, currentSpeed / 2));
                else
                    car.Rotate(car.up, steering * Time.fixedDeltaTime * Mathf.Max(-1, currentSpeed / 2));
            }
            if (axle.drivable)
            {
                if (isReversing && body.velocity.sqrMagnitude > 100)
                    acc = 0;

                body.AddForce(acc * car.forward, ForceMode.Acceleration);
                body.AddForce(sidewaysAcceleration * car.right, ForceMode.VelocityChange);
            }

            axle.ApplyDownForce(body);
        }
    }

    private void CalculateVehicleAcceleration()
    {
        float forwardVelocity = Vector3.Dot(body.velocity, car.forward);
        float sidewaysVelocity = Vector3.Dot(body.velocity, car.right);

        forwardAcceleration = accelerationCurve.Evaluate(1 - (forwardVelocity / topSpeed));
        sidewaysAcceleration = -sidewaysVelocity;

        currentSpeed = forwardVelocity;
        //Debug.Log("Speed: " + (int)(forwardVelocity * 2.237f) + " mph");
    }

    private void StabilizeCar()
    {
        bool[] leftWheelsGrounded, rightWheelsGrounded;
        Check_If_Car_Needs_Balancing(out leftWheelsGrounded, out rightWheelsGrounded);
        Check_If_Car_Is_In_The_Air(leftWheelsGrounded, rightWheelsGrounded);
    }

    private void Check_If_Car_Needs_Balancing(out bool[] leftWheelsGrounded, out bool[] rightWheelsGrounded)
    {
        bool needsBalancing = false;
        int axleCount = 0;

        bool[] left = { true, true };
        bool[] right = { true, true };

        foreach (AxleInfo axle in axleInfo)
        {
            bool leftGrounded, rightGrounded;
            if (!axle.IsGrounded(out leftGrounded, out rightGrounded))
            {
                needsBalancing = true;
                left[axleCount] = leftGrounded;
                right[axleCount] = rightGrounded;
            }
            axleCount++;
        }

        if (needsBalancing)
        {
            balanceCheckCounter++;
            this.BalanceCar(left, right);
        }
        else
            balanceCheckCounter = 0;

        leftWheelsGrounded = left;
        rightWheelsGrounded = right;
    }

    private void BalanceCar(bool[] left, bool[] right)
    {
        int axleCount = 0;

        foreach (AxleInfo axle in axleInfo)
        {
            if (!left[axleCount])
            {
                axle.ApplyExtraForceAt(body, true);
            }
            if (!right[axleCount])
            {
                axle.ApplyExtraForceAt(body, false);
            }

            axleCount++;
        }
    }

    private void Check_If_Car_Is_In_The_Air(bool[] leftWheelsGrounded, bool[] rightWheelsGrounded)
    {
        bool isCarGrounded = false;
        int axleCount = 0;

        foreach (AxleInfo axle in axleInfo)
        {
            if (leftWheelsGrounded[axleCount] || rightWheelsGrounded[axleCount])
            {
                isCarGrounded = true;
                break;
            }
            axleCount++;
        }

        if (!isCarGrounded)
            resetTimer += Time.deltaTime;
        else
            resetTimer = 0;

        if (resetTimer > resetTime)
            MakeCarGrounded();
    }

    private void MakeCarGrounded()
    {
        transform.rotation = Quaternion.LookRotation(transform.forward);
        transform.position += Vector3.up * 0.5f;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        resetTimer = 0;
    }
}
