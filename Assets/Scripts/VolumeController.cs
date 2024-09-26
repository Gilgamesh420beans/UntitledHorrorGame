using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeController : MonoBehaviour
{
    public AudioMixer audioMixer;  // The main audio mixer
    public Slider volumeSlider;    // The volume slider

    // This method is called when the slider's value changes
    public void SetVolume(float volume)
    {
        // Convert the slider's 0 to 1 range to the logarithmic decibel scale (-80dB to 0dB)
        float volumeInDb = Mathf.Log10(volume) * 20;
        audioMixer.SetFloat("MasterVolume", volumeInDb);
    }

    void Start()
    {
        // Add listener to detect slider changes
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }
}
