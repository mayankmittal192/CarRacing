using UnityEngine;

public class SoundToggler : MonoBehaviour
{
    
    private SoundController soundScript;
    public float fadeTime = 1.0f;

    void Start()
    {
        soundScript = GetComponent<SoundController>();
    }

    void OnTriggerEnter()
    {
        soundScript.ControlSound(true, fadeTime);
    }

    void OnTriggerExit()
    {
        soundScript.ControlSound(false, fadeTime);
    }
}
