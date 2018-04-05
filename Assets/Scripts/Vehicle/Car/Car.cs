using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is general implementation of arcade vehicle physics. It defines basic functionalities present in every vehicle be it a  
/// normal civilian car or a racinng car. Generally these basic features includes throttle, steering, acceleration, topSpeed etc. 
/// It also defines a reset time parameter to reset the vehicle should it get disoriented or out of control. It also takes care of 
/// axles present in the vehicle and handle the wheel motion graphics simulation.
/// </summary>
[System.Serializable]
public class Car : MonoBehaviour
{
    // Unit conversion constants:
    // 1) MPS  :-   meters per second
    // 2) KMPH :-   miles per hour
    // 3) MPH  :-   kilometers per hour
    protected const float MPS_TO_KMPH = 3.6f;
    protected const float MPS_TO_MPH = 2.237f;

    // Axle list
    public List<AxleInfo> axleInfo;

    // Adjusted center of mass of the vehicle
    public GameObject centerOfMass;

    // Basic feature variables
    public float maxSteerAngle;
    public int topSpeed;
    public AnimationCurve accelerationCurve;

    // Cached components
    protected Transform car;
    protected Rigidbody body;

    // Basic properties
    public float throttle { get; protected set; }
    public float steering { get; protected set; }

    // Speed tracking property
    public float currentSpeed { get; protected set; }


    // Use this for pre-initialization
    protected virtual void Awake()
    {
        car = transform;
        body = GetComponent<Rigidbody>();
    }


    // Use this for initialization
    protected virtual void Start ()
    {
        // set the vehicle's center of mass
        body.centerOfMass = centerOfMass.transform.localPosition;

        // setup vehicle axles
        foreach (var axle in axleInfo)
            axle.Setup(this);
    }


    // Update is called once per frame
    protected virtual void Update ()
    {
        UpdateWheelVisuals();
	}


    // FixedUpdate is called at fixed time intervals
    protected virtual void FixedUpdate() { }


    // Simulate the wheel motion graphics
    private void UpdateWheelVisuals()
    {
        foreach (var axle in axleInfo)
        {
            axle.ApplyWheelSteering(this);
            axle.ApplyWheelRotation(this);
        }
    }
}
