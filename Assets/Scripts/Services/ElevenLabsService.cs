using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A TTS Script using the ElevenLabs API TTS Service. Requires the a enviroment variable for the API key.
/// </summary>
public class ElevenLabsService : TTService 
{
    
    private readonly string url = "https://api.elevenlabs.io/v1/text-to-speech/";
    readonly string apiKey;

    private readonly SemaphoreSlim semaphore = new(1);

    public ElevenLabsService(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public Task<AudioClip> getAudioClip(VoiceProfile voice, string dialogue)
    {
        return SendPostRequest(voice.getId(), dialogue);
    }

    private async Task<AudioClip> SendPostRequest(string voiceid, string dialogue)
    {
        await semaphore.WaitAsync();
        string jsonPayload = new ElevenLabsRequest(dialogue).ToJson();
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // Create request
        UnityWebRequest request = new(url + voiceid, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerAudioClip(url + voiceid, AudioType.MPEG)
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", apiKey);
        await request.SendWebRequest();
        semaphore.Release();
        if (request.result != UnityWebRequest.Result.Success)
        {
            string errorMsg = $"Request failed: {request.responseCode} - {request.error}";
            Debug.LogError(errorMsg);
            return null;
        }
        return DownloadHandlerAudioClip.GetContent(request);

    }

    public static IEnumerable<OptionBase> GetTTSOptions()
    {
        return new List<OptionBase>()
        {
            new FloatBoundOption("Stability", 0.5f ,0,1),
            new FloatBoundOption("SimilarityBoost", 0.5f, 0, 1),
            new PickOption<bool>("UseSpeakerBoost", new List<OptionEntry<bool>>()
            {
                new(true, "true"),
                new(false, "false")
            }),
            new FloatBoundOption("Speed", 1, 0.1f, 2f)
        };
    }

}
