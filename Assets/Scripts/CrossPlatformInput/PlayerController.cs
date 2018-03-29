using UnityEngine;

/// <summary>
/// Input controller for the player. It wraps a controller object implementing IController interface which 
/// facilitates the runtime changing and initialization of controller hardware, for instance: keyboard, gamepad, 
/// tablet or mobile etc.
/// </summary>
public class PlayerController
{
    // Controller object (be it any kind of controller: keyboard, gamepad, or mobile)
    private IController controller;


    // Use this for initialization setup
    public void Setup(IController controllerPref)
    {
        // new controller object (can be changed by the player during runtime)
        controller = controllerPref;

        // default controller settings (can be re-configured by the player during runtime)
        controller.ApplyDefaultSettings();

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
