// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.

using UnityEngine;

public class FPSCounter : MonoBehaviour
{

    public double updateInterval = 1.0f;
    private double accum = 0.0f;        // FPS accumulated over the interval
    private double frames = 0;          // Frames drawn over the interval
    private double timeleft;            // Left time for current interval
    private double fps = 15.0f;         // Current FPS
    private double lastSample;
    private int gotIntervals = 0;

    void Start()
    {
        timeleft = updateInterval;
        lastSample = Time.realtimeSinceStartup;
    }

    void Update()
    {
        frames += 1;
        double newSample = Time.realtimeSinceStartup;
        double deltaTime = newSample - lastSample;
        lastSample = newSample;

        timeleft -= deltaTime;
        accum += 1.0 / deltaTime;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            fps = accum / frames;
            timeleft = updateInterval;
            accum = 0.0;
            frames = 0;
            gotIntervals += 1;
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(Screen.width - 160, 10, 150, 40), fps.ToString("f2") + " | QSetting: " + QualitySettings.GetQualityLevel());
    }

    private double GetFPS()
    {
        return fps;
    }

    private bool HasFPS()
    {
        return gotIntervals > 2;
    }

}
