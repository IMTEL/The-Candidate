using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class NPC : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TTService.Voice voice;
    public Environment.Actor actor;
    public AudioSource audioSource {get; private set;}

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void talk(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }
}
