using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Car : MonoBehaviour
{
    // Axle list
    public List<AxleInfo> axleInfo;

    // Adjusted center of mass of the vehicle
    public GameObject centerOfMass;

    // Variables
    public float maxSteering;
    protected float currSpeed;

    // Cached components
    protected Transform car;
    protected Rigidbody body;

    // Properties
    public float throttle { get; protected set; }
    public float steering { get; protected set; }


    // Axle Class
    [System.Serializable]
    public class AxleInfo
    {
        public Transform leftWheel;
        public Transform rightWheel;
        public Transform leftTireGraphics;
        public Transform rightTireGraphics;
        private float leftTireRadius;
        private float rightTireRadius;
        private float leftWheelAngle;
        private float rightWheelAngle;
        private Collider leftWheelCollider;
        private Collider rightWheelCollider;
        public bool drivable;
        public bool steerable;

        public void Setup(Car car)
        {
            leftTireRadius = leftTireGraphics.GetComponent<Renderer>().bounds.size.y * 0.5f;
            rightTireRadius = rightTireGraphics.GetComponent<Renderer>().bounds.size.y * 0.5f;
            leftWheelAngle = leftWheel.localEulerAngles.y;
            rightWheelAngle = rightWheel.localEulerAngles.y;
        }

        public void ApplyWheelSteering(Car car)
        {
            if (steerable)
            {
                leftWheel.localEulerAngles = new Vector3(0, leftWheelAngle + car.steering, 0);
                rightWheel.localEulerAngles = new Vector3(0, rightWheelAngle + car.steering, 0);
            }
        }

        public void ApplyWheelRotation(Car car)
        {
            float leftRotationAngle = (car.currSpeed / leftTireRadius) * Mathf.Rad2Deg * Time.deltaTime;
            float rightRotationAngle = (car.currSpeed / rightTireRadius) * Mathf.Rad2Deg * Time.deltaTime;
            leftTireGraphics.Rotate(leftRotationAngle, 0, 0);
            rightTireGraphics.Rotate(rightRotationAngle, 0, 0);
        }

        public void ApplyDownForce(Rigidbody body)
        {
            body.AddForceAtPosition(-100 * body.transform.up, leftWheel.transform.position, ForceMode.Force);
            body.AddForceAtPosition(-100 * body.transform.up, rightWheel.transform.position, ForceMode.Force);
        }
    }


    // Use this for pre-initialization
    protected virtual void Awake()
    {
        car = transform;
        body = GetComponent<Rigidbody>();
    }

    // Use this for initialization
    protected virtual void Start ()
    {
        // Set the vehicle's center of mass
        body.centerOfMass = centerOfMass.transform.localPosition;

        // Setup axles
        SetupAxles();
    }

    // Update is called once per frame
    protected virtual void Update ()
    {
        UpdateWheelVisuals();
	}

    // FixedUpdate is called at fixed time intervals
    protected virtual void FixedUpdate()
    {

    }
    
    private void SetupAxles()
    {
        foreach (var axle in axleInfo)
        {
            axle.Setup(this);
        }
    }

    private void UpdateWheelVisuals()
    {
        foreach (var axle in axleInfo)
        {
            axle.ApplyWheelSteering(this);
            axle.ApplyWheelRotation(this);
        }
    }
}
