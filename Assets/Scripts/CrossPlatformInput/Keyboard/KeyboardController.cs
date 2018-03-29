using UnityEngine;

/// <summary>
/// This is keyboard input system designed with the aim of converting sudden key press actions to smooth 
/// to smooth and gradual analog actions. This is done because in real life, any driver will gradually 
/// press or release pedals instead of suddenly pressing or releasing pedals. This system approximately 
/// simulates those real life actions very well upto remarkable degree of accuracy and fluidity. The main 
/// principle involved in designing this system is based on double buffering technique along with some other 
/// modifications like slow buffer evacuation system which enhances the player's overall game fluidity and 
/// continuity experience.
/// </summary>
/// TODO: Nitro key functionality ///
[System.Serializable]
public class KeyboardController : IController
{
    // Input Key Allotment Settings
    public KeyCode throttle;
    public KeyCode brake;
    public KeyCode left;
    public KeyCode right;
    public KeyCode handbrake;
    public KeyCode nitrous;
    public KeyCode steerLock;
    public float throttleReactionTime;
    public float steeringReactionTime;

    // Input Buffer
    private float acceleration;                 // range: -1 to +1
    private float steering;                     // range: -1 to +1
    private float throttleInputBuffer;          // range: 0 to throttleResponseTime value
    private float brakeInputBuffer;             // range: 0 to throttleResponseTime value
    private float leftSteeringInputBuffer;      // range: 0 to steeringResponseTime value
    private float rightSteeringInputBuffer;     // range: 0 to steeringResponseTime value
    private float throttleBufferEvac;           // range: 0 to throttleResponseTime value
    private float brakeBufferEvac;              // range: 0 to throttleResponseTime value
    private float leftSteeringBufferEvac;       // range: 0 to steeringResponseTime value
    private float rightSteeringBufferEvac;      // range: 0 to steeringResponseTime value

    // Key state trackers
    private bool upKeyPressed;
    private bool downKeyPressed;
    private bool leftKeyPressed;
    private bool rightKeyPressed;
    private bool steerLockKeyPressed;

    // Timer Variable
    private float timeStep;

    // Numerical Constants  (fine-tune these parameters to get fluid and smooth response)
    private float ACC_INC = 0.08f;
    private float STEER_INC = 0.09f;
    private float MAX_ACC = 1;
    private float MAX_STEER = 1;
    private float IDLE_ACCELERATION_FACTOR = 0.76f;
    private float IDLE_STEERING_FACTOR = 0.72f;


    // Apply default keyboard input configuration
    public void ApplyDefaultSettings()
    {
        SetThrottle(KeyCode.UpArrow);
        SetBrake(KeyCode.DownArrow);
        SetLeft(KeyCode.LeftArrow);
        SetRight(KeyCode.RightArrow);
        SetNitrous(KeyCode.LeftControl);
        SetHandbrake(KeyCode.Space);
        SetSteerLock(KeyCode.X);
        SetThrottleReacTime(0.05f);
        SetSteerReacTime(0.03f);
    }


	// Use this for initialization setup
	public void Setup()
    {
        // Initiate acceleration and steering angle to zero
        acceleration = 0;
        steering = 0;

        // Initiate key state variables
        upKeyPressed = false;
        downKeyPressed = false;
        leftKeyPressed = false;
        rightKeyPressed = false;
        steerLockKeyPressed = false;

        // Initiate the response buffers
        throttleInputBuffer = 0;
        brakeInputBuffer = 0;
        leftSteeringInputBuffer = 0;
        rightSteeringInputBuffer = 0;
        throttleBufferEvac = 0;
        brakeBufferEvac = 0;
        leftSteeringBufferEvac = 0;
        rightSteeringBufferEvac = 0;
	}
	

	// Poll is called from time to time to poll the input keys
	public void Poll(float dt)
    {
        // Set time value
        timeStep = dt;

        // Set the input key states
        SetKeyStates();

        // Set input buffers
        SetInputBuffer();

        // Handle throttle value
        HandleThrottleValue();

        // Handle steering value
        HandleSteeringValue();
	}


    /// <summary>
    /// Inquire keyboard input system for any key presses. 
    /// Converts the keyboard action input mechanism into analog input mechanism. 
    /// Key up and down actions corresponds to the trigger values for analog booleans. 
    /// For example: throttle key down triggers the gradual gas pedal pressing action 
    /// and throttle key up triggers the slowly removing leg from the pedal action.
    /// </summary>
    private void SetKeyStates()
    {
        // Get the keyboard input states
        if (Input.GetKeyDown(throttle)) { upKeyPressed = true; }
        if (Input.GetKeyDown(brake)) { downKeyPressed = true; }
        if (Input.GetKeyDown(left)) { leftKeyPressed = true; }
        if (Input.GetKeyDown(right)) { rightKeyPressed = true; }
        if (Input.GetKeyDown(steerLock)) { steerLockKeyPressed = true; }

        if (Input.GetKeyUp(throttle)) { upKeyPressed = false; }
        if (Input.GetKeyUp(brake)) { downKeyPressed = false; }
        if (Input.GetKeyUp(left)) { leftKeyPressed = false; }
        if (Input.GetKeyUp(right)) { rightKeyPressed = false; }
        if (Input.GetKeyUp(steerLock)) { steerLockKeyPressed = false; }
    }


    /// <summary>
    /// Here the magic actually happens where quick keyboard actions are converted 
    /// to the gradual and smooth analog actions by storing impulsive keyboard input 
    /// actions into input buffers and then gradually releasing the 'input energy'. 
    /// It works by first delaying the response time specified by the reaction time of the 
    /// driver and then storing the impulsive keyboard actions into input buffer variables. 
    /// Then it sets the buffer evacuation variables when the corresponding key is released.
    /// </summary>
    private void SetInputBuffer()
    {
        // Set throttle input buffer
        if (upKeyPressed)
        {
            throttleInputBuffer += timeStep;
            if (throttleInputBuffer > throttleReactionTime)
            {
                throttleInputBuffer = throttleReactionTime;
                throttleBufferEvac += timeStep;
            }
        }
        else
        {
            if (throttleInputBuffer > throttleReactionTime) { throttleBufferEvac = throttleReactionTime; }
            throttleInputBuffer = 0;
        }

        // Set brake input buffer
        if (downKeyPressed)
        {
            brakeInputBuffer += timeStep;
            if (brakeInputBuffer > throttleReactionTime)
            {
                brakeInputBuffer = throttleReactionTime;
                brakeBufferEvac += timeStep;
            }
        }
        else
        {
            if (brakeInputBuffer > throttleReactionTime) { brakeBufferEvac = throttleReactionTime; }
            brakeInputBuffer = 0;
        }

        // Set left steering input buffer
        if (leftKeyPressed)
        {
            leftSteeringInputBuffer += timeStep;
            if (leftSteeringInputBuffer > steeringReactionTime)
            {
                leftSteeringInputBuffer = steeringReactionTime;
                leftSteeringBufferEvac += timeStep;
            }
        }
        else
        {
            if (leftSteeringInputBuffer > steeringReactionTime) { leftSteeringBufferEvac = steeringReactionTime; }
            leftSteeringInputBuffer = 0;
        }

        // Set right steering input buffer
        if (rightKeyPressed)
        {
            rightSteeringInputBuffer += timeStep;
            if (rightSteeringInputBuffer > steeringReactionTime)
            {
                rightSteeringInputBuffer = steeringReactionTime;
                rightSteeringBufferEvac += timeStep;
            }
        }
        else
        {
            if (rightSteeringInputBuffer > steeringReactionTime) { rightSteeringBufferEvac = steeringReactionTime; }
            rightSteeringInputBuffer = 0;
        }
    }


    /// <summary>
    /// Throttle value of car is determined the amount of input buffer 
    /// stored in the throttle buffer variables.
    /// </summary>
    private void HandleThrottleValue()
    {
        // Adjust the acceleration value towards zero
        if (throttleBufferEvac == 0 && brakeBufferEvac == 0)
        {
            float accelerationPolarity = FindPolarity(acceleration);
            acceleration -= ACC_INC * accelerationPolarity * IDLE_ACCELERATION_FACTOR;
            if (FindPolarity(acceleration) * accelerationPolarity == -1) { acceleration = 0; }
        }
        else
        {
            // Increase the acceleration value
            if (throttleBufferEvac > 0)
            {
                // Decrease the evac buffer value
                throttleBufferEvac -= timeStep;
                // Increase the acceleration value steadily
                acceleration += ACC_INC;
            }

            // Decrease the acceleration value
            if (brakeBufferEvac > 0)
            {
                // Decrease the evac buffer value
                brakeBufferEvac -= timeStep;
                // Decrease the acceleration value steadily
                acceleration -= ACC_INC;
            }
        }

        // Clamp the acceleration value
        if (Mathf.Abs(acceleration) > MAX_ACC) { acceleration = FindPolarity(acceleration); }
    }


    /// <summary>
    /// Steering value of car is determined the amount of input buffer 
    /// stored in the steering buffer variables.
    /// </summary>
    private void HandleSteeringValue()
    {
        // Adjust the steering angle value towards zero
        if (leftSteeringBufferEvac == 0 && rightSteeringBufferEvac == 0)
        {
            float steeringPolarity = FindPolarity(steering);
            if (!steerLockKeyPressed) { steering -= STEER_INC * steeringPolarity * IDLE_STEERING_FACTOR; }
            if (FindPolarity(steering) * steeringPolarity == -1) { steering = 0; }
        }
        else
        {
            // Decrease the steering angle value
            if (leftSteeringBufferEvac > 0)
            {
                // Decrease the evac buffer value
                leftSteeringBufferEvac -= timeStep;
                // Decrease the steering value steadily
                if (!steerLockKeyPressed) { steering -= STEER_INC; }
            }
            // Increase the steering angle value
            if (rightSteeringBufferEvac > 0)
            {
                // Decrease the evac buffer value
                rightSteeringBufferEvac -= timeStep;
                // Increase the steering value steadily
                if (!steerLockKeyPressed) { steering += STEER_INC; }
            }
        }

        // Clamp the steering angle value
        if (Mathf.Abs(steering) > MAX_STEER) { steering = FindPolarity(steering); }
    }


    // Getter Methods
    public float GetThrottle() { return acceleration; }
    
    public float GetSteering() { return steering; }
    
    public bool IsHandbraking()
    {
        // return true if the key is pressed and false if not, no need for converting 
        // it to analog since handbraking is intended to be impulsive.
        if (Input.GetKeyDown(handbrake)) { return true; }
        return false;
    }


    // Setter Methods
    public void SetThrottle(KeyCode key) { throttle = key; }

    public void SetBrake(KeyCode key) { brake = key; }

    public void SetLeft(KeyCode key) { left = key; }

    public void SetRight(KeyCode key) { right = key; }

    public void SetNitrous(KeyCode key) { nitrous = key; }

    public void SetHandbrake(KeyCode key) { handbrake = key; }

    public void SetSteerLock(KeyCode key) { steerLock = key; }

    public void SetThrottleReacTime(float t) { throttleReactionTime = t; }

    public void SetSteerReacTime(float t) { steeringReactionTime = t; }


    // Helper Method
    private int FindPolarity(float value)
    {
        int retVal = 0;
        if (value > 0) { retVal = 1; }
        else if (value < 0) { retVal = -1; }
        return retVal;
    }
}
