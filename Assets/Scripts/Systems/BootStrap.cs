using Unity.Android.Gradle.Manifest;
using UnityEngine;
using Whisper;

/// <summary>
/// Iniates the requirments for running the game
/// </summary>
///
[RequireComponent(typeof(WhisperManager))]
public class BootStrap : MonoBehaviour
{
    private BootStrap instance;
    public string TTSurl;
    public string TTSApiKey;

    public string AIurl;
    public string AIApiKey;

    void Awake()
    {
        if(instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);
        Services.InitializeGlobals(new(AIurl, AIApiKey), new(TTSurl,TTSApiKey));
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
}
