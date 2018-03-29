using UnityEngine;

/// <summary>
/// Controller interface designed to implement a controller object of any hardware for instance: keyboard, gamepad, 
/// tablet or mobile etc. which can be changed during runtime.
/// </summary>
public interface IController
{
    // Getter Methods
    float GetThrottle();
    float GetSteering();
    bool IsHandbraking();

    // Input Buttons Mapping
    void SetThrottle(KeyCode key);
    void SetBrake(KeyCode key);
    void SetLeft(KeyCode key);
    void SetRight(KeyCode key);
    void SetNitrous(KeyCode key);
    void SetHandbrake(KeyCode key);
    void SetSteerLock(KeyCode key);

    // Reaction Time Values
    void SetThrottleReacTime(float t);
    void SetSteerReacTime(float t);

    // Operational Methods
    void ApplyDefaultSettings();
    void Setup();
    void Poll(float dt);
}
