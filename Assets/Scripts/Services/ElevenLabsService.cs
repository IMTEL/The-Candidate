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
    
    private string url = "https://api.elevenlabs.io/v1/text-to-speech/";
    string apiKey;

    private readonly SemaphoreSlim semaphore = new(1);

    public ElevenLabsService(string apiKey, Dictionary<VoiceProfile, string> voiceDB) : base(voiceDB)
    {
        this.apiKey = apiKey;
    }

    public override Task<AudioClip> getAudioClip(VoiceProfile voice, string dialogue)
    {
        return SendPostRequest(getProfileID(voice), dialogue);
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

    public override string getDefualtVoiceId()
    {
        return "";
    }
}
