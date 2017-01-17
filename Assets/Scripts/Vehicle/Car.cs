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
        private Collider leftWheelCollider;
        private Collider rightWheelCollider;
        public bool drivable;
        public bool steerable;

        public void ApplyWheelSteering(Car car)
        {
            if (steerable)
            {
                leftWheel.transform.localEulerAngles = new Vector3(0, car.steering, 0);
                rightWheel.transform.localEulerAngles = new Vector3(0, car.steering, 0);
            }
        }

        public void ApplyWheelRotation(Rigidbody carBody)
        {
            
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


    private void UpdateWheelVisuals()
    {
        foreach (var axle in axleInfo)
        {
            axle.ApplyWheelSteering(this);
        }
    }
}
