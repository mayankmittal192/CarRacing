using UnityEngine;

/// <summary>
/// This system defines structure and feaures of a vehicle's axle 
/// along with some other useful functional & helper routines.
/// </summary>
[System.Serializable]
public class AxleInfo
{
    // Hard-Coded stabilizer constants
    private const float DOWN_FORCE_VALUE = 100.0f;
    private const float EXTRA_DOWN_FORCE_VALUE = 500.0f;

    // Stuctural components
    public Transform leftWheel;
    public Transform rightWheel;
    public Transform leftTireGraphics;
    public Transform rightTireGraphics;

    // Functional components
    public bool drivable;
    public bool steerable;

    // Data caching variables
    private float radius;
    private float steerAngle;
    private Collider leftWheelCollider;
    private Collider rightWheelCollider;


    // Axle setup
    public void Setup(Car car)
    {
        radius = leftTireGraphics.GetComponent<Renderer>().bounds.size.y * 0.5f;
        steerAngle = leftWheel.localEulerAngles.y;
    }


    // Simulate wheel steering motion graphics according to the steering applied to this vehicle
    public void ApplyWheelSteering(Car car)
    {
        if (steerable)
        {
            Vector3 wheelAngle = new Vector3(0, steerAngle + car.steering, 0);
            leftWheel.localEulerAngles = wheelAngle;
            rightWheel.localEulerAngles = wheelAngle;
        }
    }


    // Simulate wheel rotation motion graphics according to the current speed of the vehicle
    public void ApplyWheelRotation(Car car)
    {
        float rotationAngle = (car.currentSpeed / radius) * Mathf.Rad2Deg * Time.deltaTime;
        leftTireGraphics.Rotate(rotationAngle, 0, 0);
        rightTireGraphics.Rotate(rotationAngle, 0, 0);
    }


    // Downward force is applied on the wheels to stabilize the vehicle
    public void ApplyDownForce(Rigidbody body)
    {
        Vector3 downForce = DOWN_FORCE_VALUE * -body.transform.up;
        body.AddForceAtPosition(downForce, leftWheel.transform.position, ForceMode.Force);
        body.AddForceAtPosition(downForce, rightWheel.transform.position, ForceMode.Force);
    }


    // Extra downward force is applied on the wheels to stabilize the vehicle
    public void ApplyExtraForceAt(Rigidbody body, bool isLeftWheel)
    {
        Vector3 downForce = EXTRA_DOWN_FORCE_VALUE * -body.transform.up;

        if (isLeftWheel)
            body.AddForceAtPosition(downForce, leftWheel.transform.position, ForceMode.Force);
        else
            body.AddForceAtPosition(downForce, rightWheel.transform.position, ForceMode.Force);
    }


    // Returns a boolean indicating whether or not the axle's left and right wheels are touching the ground
    public bool IsGrounded(out bool leftGrounded, out bool rightGrounded)
    {
        leftGrounded = false;
        rightGrounded = false;
        float errorMargin = 0.05f;

        if (Physics.Raycast(leftWheel.position, -leftWheel.up, radius + errorMargin))
            leftGrounded = true;

        if (Physics.Raycast(rightWheel.position, -rightWheel.up, radius + errorMargin))
            rightGrounded = true;

        return leftGrounded && rightGrounded;
    }
}
