using UnityEngine;

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
