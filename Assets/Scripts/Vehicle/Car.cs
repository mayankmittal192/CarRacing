using UnityEngine;
using System.Collections.Generic;

public class Car : MonoBehaviour
{

    // Unit conversion constants:
    // 1) MPS  :-   meter per second
    // 2) KMPH :-   miles per hour
    // 3) MPH  :-   kilometer per hour
    private const float MPS_TO_KMPH = 3.6f;
    private const float MPS_TO_MPH = 2.237f;

    public float throttle { get; private set; }
    public float steer { get; private set; }

    public List<AxleInfo> axleInfo;
    public Transform centerOfMass;
    [Range(3, 6)]
    public float resetTime;
    [Range(20, 35)]
    public float maxSteerAngle;
    [Range(100, 400)]
    public float topSpeed;

    private Rigidbody rb;
    private int balanceCheckCount;
    private float resetTimer;
    private AxleInfo frontAxle;
    private AxleInfo rearAxle;
    private float wheelBase;

    public float torque;


    [System.Serializable]
    public class AxleInfo
    {
        public Transform leftWheel;
        public Transform rightWheel;
        public Transform leftTireGraphics;
        public Transform rightTireGraphics;
        public bool isFront;
        public bool drivable;
        public float leftRadius { get; private set; }
        public float rightRadius { get; private set; }
        private Rigidbody car;

        public void Setup(Rigidbody rb)
        {
            leftRadius = leftTireGraphics.GetComponent<Renderer>().bounds.size.y * 0.5f;
            rightRadius = rightTireGraphics.GetComponent<Renderer>().bounds.size.y * 0.5f;
            car = rb;
        }

        public void ApplyMotorTorque(float motorTorque)
        {
            if (drivable)
            {
                float leftMotorForce = motorTorque / leftRadius;
                float rightMotorForce = motorTorque / rightRadius;
                Vector3 leftPOC = leftWheel.position;
                Vector3 rightPOC = rightWheel.position;
                leftPOC.y -= leftRadius;
                rightPOC.y -= rightRadius;
                car.AddForceAtPosition(leftWheel.forward * leftMotorForce, leftPOC);
                car.AddForceAtPosition(rightWheel.forward * rightMotorForce, rightPOC);
            }
        }

        public void ApplySteering(float steeringAngle, float motorTorque)
        {
            if (isFront)
            {
                float leftMotorForce = motorTorque / leftRadius;
                float rightMotorForce = motorTorque / rightRadius;
                float maxMotorForce = car.mass / 4;
                leftMotorForce = Mathf.Min(leftMotorForce, maxMotorForce);
                rightMotorForce = Mathf.Min(rightMotorForce, maxMotorForce);
                Vector3 leftPOC = leftWheel.position;
                Vector3 rightPOC = rightWheel.position;
                leftPOC.y -= leftRadius;
                rightPOC.y -= rightRadius;
                car.AddForceAtPosition(leftWheel.right * Mathf.Sign(steeringAngle) * leftMotorForce, leftPOC);
                car.AddForceAtPosition(rightWheel.right * Mathf.Sign(steeringAngle) * rightMotorForce, rightPOC);
            }
        }

        public void ApplyDownForce(float downForce)
        {
            car.AddForceAtPosition(-leftWheel.up * downForce, leftWheel.position);
            car.AddForceAtPosition(-rightWheel.up * downForce, rightWheel.position);
        }

        public void UpdateWheelSteeringGraphics(float steeringAngle)
        {
            if (isFront)
            {
                leftWheel.localEulerAngles = new Vector3(0, steeringAngle, 0);
                rightWheel.localEulerAngles = new Vector3(0, steeringAngle, 0);
            }
        }

        public void UpdateWheelRotationGraphics(float velocity)
        {
            float leftRotationAngle = (velocity / leftRadius) * Mathf.Rad2Deg * Time.deltaTime;
            float rightRotationAngle = (velocity / rightRadius) * Mathf.Rad2Deg * Time.deltaTime;
            leftTireGraphics.Rotate(leftRotationAngle, 0, 0);
            rightTireGraphics.Rotate(rightRotationAngle, 0, 0);
        }

        public bool IsGrounded(out bool leftGrounded, out bool rightGrounded)
        {
            bool grounded = true;
            leftGrounded = false;
            rightGrounded = false;

            if (Physics.Raycast(leftWheel.position, -leftWheel.up, leftRadius + 0.05f))
                leftGrounded = true;
            else
                grounded = false;

            if (Physics.Raycast(rightWheel.position, -rightWheel.up, rightRadius + 0.05f))
                rightGrounded = true;
            else
                grounded = false;

            return grounded;
        }
    }


    public void Start()
    {
        rb = GetComponent<Rigidbody>();

        SetupAxles();

        SetupCenterOfMass();
    }

    public void Update()
    {
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity);

        GetInput();

        Check_If_Car_Is_Balanced();

        Check_If_Car_Is_Flipped();

        UpdateWheelGraphics(relativeVelocity);
    }

    public void FixedUpdate()
    {
        Vector3 relativeVelocity = transform.InverseTransformDirection(rb.velocity);

        ApplyThrottle();

        ApplySteering(relativeVelocity);

        ApplyDownForce(relativeVelocity);

        rb.AddTorque(transform.up * torque * 1000);
        //transform.Rotate(0, 1, 0, Space.World);
    }


    /**************************************************/
    /* Functions called from Start()                  */
    /**************************************************/

    private void SetupVariables()
    {
        throttle = 0;
        steer = 0;
        balanceCheckCount = 0;
        resetTimer = 0;
        topSpeed = topSpeed * (1 / MPS_TO_KMPH);
    }

    private void SetupAxles()
    {
        foreach (AxleInfo axle in axleInfo)
        {
            axle.Setup(rb);

            if (axle.isFront)
                frontAxle = axle;
            else
                rearAxle = axle;
        }

        wheelBase = (frontAxle.leftWheel.position - rearAxle.leftWheel.position).magnitude;
    }

    private void SetupCenterOfMass()
    {
        if (centerOfMass != null)
        {
            rb.centerOfMass = centerOfMass.localPosition;
            rb.ResetInertiaTensor();
        }
    }

    /**************************************************/
    /* Functions called from Update()                 */
    /**************************************************/

    private void GetInput()
    {
        throttle = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");
    }

    private void Check_If_Car_Is_Balanced()
    {
        bool needBalance = false;
        bool[] left = { true, true };
        bool[] right = { true, true };
        int axleCount = 0;

        foreach (AxleInfo axle in axleInfo)
        {
            bool leftGrounded, rightGrounded;
            if (!axle.IsGrounded(out leftGrounded, out rightGrounded))
            {
                needBalance = true;
                left[axleCount] = leftGrounded;
                right[axleCount] = rightGrounded;
            }
            axleCount++;
        }

        if (needBalance)
        {
            balanceCheckCount++;
            BalanceCar(left, right);
        }
        else
            balanceCheckCount = 0;
    }

    private void BalanceCar(bool[] left, bool[] right)
    {
        float errorValue = 0.1f;

        if (!left[0] || !right[0])
            transform.RotateAround(rearAxle.leftWheel.position, transform.right, errorValue);

        if (!left[1] || !right[1])
            transform.RotateAround(frontAxle.leftWheel.position, transform.right, -errorValue);

        if (!left[0] || !left[1])
            transform.RotateAround(frontAxle.rightWheel.position, transform.forward, errorValue);

        if (!right[0] || !right[1])
            transform.RotateAround(frontAxle.leftWheel.position, transform.forward, -errorValue);

        //Check_If_Car_Is_Balanced();
    }

    private void Check_If_Car_Is_Flipped()
    {
        if (transform.localEulerAngles.z > 80 && transform.localEulerAngles.z < 280)
            resetTimer += Time.deltaTime;
        else
            resetTimer = 0;

        if (resetTimer > resetTime)
            FlipCar();
    }

    private void FlipCar()
    {
        transform.rotation = Quaternion.LookRotation(transform.forward);
        transform.position += Vector3.up * 0.5f;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        resetTimer = 0;
    }

    private void UpdateWheelGraphics(Vector3 relativeVelocity)
    {
        float steeringAngle = steer * maxSteerAngle;
        float velocity = Mathf.Sign(relativeVelocity.z) * relativeVelocity.magnitude;

        foreach (AxleInfo axle in axleInfo)
        {
            axle.UpdateWheelSteeringGraphics(steeringAngle);
            axle.UpdateWheelRotationGraphics(velocity);
        }
    }


    /**************************************************/
    /* Functions called from FixedUpdate()            */
    /**************************************************/

    private void ApplyThrottle()
    {
        foreach (AxleInfo axle in axleInfo)
        {
            axle.ApplyMotorTorque(rb.mass * throttle * 5);
        }
    }

    private void ApplySteering(Vector3 relativeVelocity)
    {
        float velocity = Mathf.Sign(relativeVelocity.z) * relativeVelocity.magnitude;

        if (steer == 0 || velocity == 0)
            return;

        // Turning Logic
        Vector3 frontPivot, backPivot;

        float steeringAngle = steer * maxSteerAngle;
        float steeringAngleComp = 90 - EvaluateSpeedToTurn(Mathf.Abs(velocity));
        float turnRadius = wheelBase / Mathf.Sin((90 - steeringAngleComp) * Mathf.Deg2Rad);
        float turnAngle = velocity / turnRadius;

        frontPivot = (frontAxle.leftWheel.position + frontAxle.rightWheel.position) * 0.5f;
        transform.RotateAround(frontPivot, transform.up, -steeringAngleComp);
        backPivot = frontPivot - transform.forward * turnRadius;
        transform.RotateAround(backPivot, transform.up, turnAngle);
        frontPivot = (frontAxle.leftWheel.position + frontAxle.rightWheel.position) * 0.5f;
        transform.RotateAround(frontPivot, transform.up, steeringAngleComp);

        // Steering Forces
        //foreach (AxleInfo axle in axleInfo)
        //{
        //    axle.ApplySteering(steer * maxSteerAngle, rb.mass * throttle * 5);
        //}

        Vector3 lateralForceSin = -transform.forward * Mathf.Sign(relativeVelocity.z) * Mathf.Sin(steeringAngle * Mathf.Deg2Rad);
        Vector3 lateralForceCos = transform.right * Mathf.Sign(steer) * Mathf.Cos(steeringAngle * Mathf.Deg2Rad);
        Vector3 lateralForceDirection = (lateralForceSin + lateralForceCos) /
            (Mathf.Pow(lateralForceSin.magnitude, 2) + Mathf.Pow(lateralForceCos.magnitude, 2));
        rb.AddForce(lateralForceDirection * rb.mass * throttle * 10);
    }

    private void ApplyDownForce(Vector3 relativeVelocity)
    {
        foreach (AxleInfo axle in axleInfo)
        {
            axle.ApplyDownForce(rb.mass * relativeVelocity.magnitude / 4);
        }
    }


    /**************************************************/
    /*              Utility Functions                 */
    /**************************************************/

    private float EvaluateSpeedToTurn(float speed)
    {
        float maxTurn = steer * maxSteerAngle;
        float minTurn = steer * maxSteerAngle * 0.2f;
        float speedFactor = Mathf.Min(1, speed / topSpeed);
        float turnAngle = Mathf.Lerp(maxTurn, minTurn, speedFactor);
        return turnAngle;
    }

}
