using UnityEngine;

public class CivilianCar : Car
{
    // public variables for tuning
    public float topSpeed;
    public AnimationCurve accelerationCurve;
    
    // Properties
    public float forwardAcceleration { get; private set; }
    public float sidewaysAcceleration { get; private set; }


    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate()
    {
        CalculateVehicleAcceleration();

        float acc = forwardAcceleration * Input.GetAxis("Vertical");
        steering = maxSteering * Input.GetAxis("Horizontal");
        
        bool isReversing = Vector3.Dot(body.velocity, car.forward) < 0;

        foreach (var axle in axleInfo)
        {
            if (axle.steerable)
            {
                car.Rotate(car.up, steering * Time.fixedDeltaTime * (isReversing ? -1 : 1));
            }
            if (axle.drivable)
            {
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

        Debug.Log("Speed: " + (int)(forwardVelocity * 2.237f) + " mph");
    }
}