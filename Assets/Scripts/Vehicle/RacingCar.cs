using UnityEngine;

public class RacingCar : Car
{
    // Properties
    public float forwardAcceleration { get; private set; }
    public float sidewaysAcceleration { get; private set; }


    // Use this for pre-initialization
    protected override void Awake()
    {
        base.Awake();
    }

    // Use this for initialization
    protected override void Start ()
    {
        base.Start();
    }
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();
        StabilizeCar();	
	}

    // FixedUpdate is called after each fixed time interval
    protected override void FixedUpdate()
    {
        CalculateVehicleAcceleration();

        float acc = forwardAcceleration * Input.GetAxis("Vertical");
        steering = maxSteering * Input.GetAxis("Horizontal");

        foreach (var axle in axleInfo)
        {
            if (axle.steerable)
            {
                car.Rotate(car.up, steering * Time.fixedDeltaTime * Mathf.Min(1, currSpeed / 2));
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

        currSpeed = forwardVelocity;
        Debug.Log("Speed: " + (int)(forwardVelocity * 2.237f) + " mph");
    }

    private void StabilizeCar()
    {
        foreach (var axle in axleInfo)
        {

        }
    }
}
