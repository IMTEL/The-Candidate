using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A TTS Script using the ElevenLabs API TTS Service. Requires the a enviroment variable for the API key.
/// </summary>
public class ElevenLabsService : ITTService
{

    private readonly string url = "https://api.elevenlabs.io";

    [SerializeField]
    string APIKey;


    private readonly SemaphoreSlim semaphore = new(1);

    public ElevenLabsService(string apiKey)
    {
        this.APIKey = apiKey;
    }

    public ElevenLabsService() : this("") { }

    public void SetAPIKey(string apikey)
    {
        this.APIKey = apikey;
    }

    public string GetAPIKey()
    {
        return APIKey;
    }

    public Task<AudioClip> getAudioClip(VoiceProfile voice, string dialogue)
    {
        return SendPostRequest(voice.getId(), dialogue);
    }

    private async Task<AudioClip> SendPostRequest(string voiceid, string dialogue)
    {
        await semaphore.WaitAsync();
        try
        {
            string jsonPayload = new ElevenLabsRequest(dialogue).ToJson();
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

            // Create request
            UnityWebRequest request = new(url + "/v1/text-to-speech/" + voiceid, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerAudioClip(url + voiceid, AudioType.MPEG)
            };
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("xi-api-key", APIKey);
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
        finally
        {
            semaphore.Release();
        }


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

    public async IAsyncEnumerator<AudioClip> getAudioClip(string dialogue, OptionSet options)
    {
        PickOption<VoiceProfile> voices = (PickOption<VoiceProfile>)options.GetOptions().First(option => option.GetName().Equals("voice"));
        AudioClip clip = await SendPostRequest(voices.GetCurrentValue().getId(), dialogue);
        yield return clip;
    }

    private async Task<List<VoiceProfile>> getVoiceProfile(string nextPageToken = "")
    {
        List<VoiceProfile> voiceProfiles = new();
        Debug.Log("Test valid?");
        if (string.IsNullOrEmpty(APIKey))
        {
            throw new ArgumentException("The apikey needs to be set for the settings to be correct");
        }
        string queryString = "";
        if (nextPageToken != null & nextPageToken != "")
        {
            queryString = "?next_page_token=" + nextPageToken;
        }
        string url = this.url + "v2/voices" + queryString;
        UnityWebRequest request = new(url)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", APIKey);
        await request.SendWebRequest();
        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        JToken voices = json["voices"];
        foreach (JToken voice in voices)
        {
            voiceProfiles.Add(new((string)voice["voice_id"], (string)voice["name"]));
            Debug.Log((string)voice["settings"]);
        }
        if (string.IsNullOrWhiteSpace((string)json["next_page_token"]) && (string)json["next_page_token"] != nextPageToken)
        {
            foreach (VoiceProfile voice in await getVoiceProfile((string)json["next_page_token"]))
            {
                voiceProfiles.Add(voice);
            }
        }
        return voiceProfiles;
    }
}
