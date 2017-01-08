using UnityEngine;
using System.Collections.Generic;

public class CivilianCar : Car
{
    // Axle Class
    [System.Serializable]
    public class AxleInfo
    {
        public Collider leftWheel;
        public Collider rightWheel;
        public bool drivable;
        public bool steerable;

        public void ApplyDownForce(Rigidbody carBody)
        {
            carBody.AddForceAtPosition(-100 * carBody.transform.up, leftWheel.transform.position, ForceMode.Force);
            carBody.AddForceAtPosition(-100 * carBody.transform.up, rightWheel.transform.position, ForceMode.Force);
        }
    }


    // public variables for tuning
    public List<AxleInfo> axleInfo;
    public float topSpeed;
    public AnimationCurve accelerationCurve;
    public float maxSteeringAngle;

    // adjusted center of mass of the vehicle
    public GameObject centerOfMass;

    // Cached components
    private Transform car;
    private Rigidbody carBody;

    // Properties
    public float acceleration { get; private set; }
    public float sidewaysAcceleration { get; private set; }


    protected override void Awake()
    {
        car = transform;
        carBody = GetComponent<Rigidbody>();
    }

    protected override void Start()
    {
        // Set the vehicle's center of mass
        carBody.centerOfMass = centerOfMass.transform.localPosition;
    }

    protected override void FixedUpdate()
    {
        CalculateVehicleAcceleration();

        float acc = acceleration * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        
        bool isReversing = Vector3.Dot(carBody.velocity, car.forward) < 0;

        foreach (AxleInfo axle in axleInfo)
        {
            if (axle.steerable)
            {
                car.Rotate(car.up, steering * Time.fixedDeltaTime * (isReversing ? -1 : 1));
            }
            if (axle.drivable)
            {
                carBody.AddForce(acc * car.forward, ForceMode.Acceleration);
                carBody.AddForce(sidewaysAcceleration * car.right, ForceMode.VelocityChange);
            }

            axle.ApplyDownForce(carBody);
        }
    }

    private void CalculateVehicleAcceleration()
    {
        float forwardVelocity = Vector3.Dot(carBody.velocity, car.forward);
        float sidewaysVelocity = Vector3.Dot(carBody.velocity, car.right);

        acceleration = accelerationCurve.Evaluate(1 - (forwardVelocity / topSpeed));
        sidewaysAcceleration = -sidewaysVelocity;

        Debug.Log("Speed: " + (int)(forwardVelocity * 2.237f) + " mph");
    }
}