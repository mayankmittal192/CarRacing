using UnityEngine;

public class PlayerController {

    // Controller object (be it any kind of controller: keyboard, gamepad, or mobile)
    private IController controller;


	// Use this for initialization setup
	public void Setup()
    {
        // new controller object (can be changed by the player during runtime)
        controller = new KeyboardController();
        
        // controller setup (can be re-configured by the player during runtime)
        controller.SetThrottle(KeyCode.UpArrow);
        controller.SetBrake(KeyCode.DownArrow);
        controller.SetLeft(KeyCode.LeftArrow);
        controller.SetRight(KeyCode.RightArrow);
        controller.SetNitrous(KeyCode.LeftControl);
        controller.SetHandbrake(KeyCode.Space);
        controller.SetSteerLock(KeyCode.X);
        controller.SetThrottleReacTime(0.05f);
        controller.SetSteerReacTime(0.03f);

        // controller setup
        controller.Setup();
	}

    // Poll is called once per frame in which input polling will be done and results will be
    // analysed based on the amount of skipped time i.e deltaTime.
    public void Poll(float dt)
    {
        controller.Poll(dt);
    }

	
	// Getter Methods for fetching input
    public float Throttle()
    {
        return controller.GetThrottle();
    }

    public float Steer()
    {
        return controller.GetSteering();
    }

    public bool Handbraking()
    {
        return controller.IsHandbraking();
    }
}
