using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A TTS Script using the ElevenLabs API TTS Service. Requires the a enviroment variable for the API key.
/// </summary>
[Serializable]
public class ElevenLabsService : ITTService
{

    [NonSerialized]
    private readonly string url = "https://api.elevenlabs.io";

    [NonSerialized]
    private static readonly FloatBoundOption Stability = new("Stability", 0.5f, 0, 1);

    [NonSerialized]
    private static readonly FloatBoundOption SimilarityBoost = new("SimilarityBoost", 0.5f, 0, 1);

    [NonSerialized]
    private static readonly FloatBoundOption Speed = new("Speed", 1, 0.1f, 2f);

    [NonSerialized]
    private static readonly PickOption<bool> SpeakerBoost =
        new("UseSpeakerBoost", new List<OptionEntry<bool>>()
            {
                new(true),
                new(false)
            });

    [SerializeField]
    private PickOption<VoiceProfile> VoiceProfile = new("Voice", new List<OptionEntry<VoiceProfile>>() { new(null) });

    [SerializeField]
    string APIKey;

    [NonSerialized]
    private readonly SemaphoreSlim semaphore = new(1);

    public ElevenLabsService(string apiKey)
    {
        this.APIKey = apiKey;
    }

    public ElevenLabsService() : this("") { }

    public async Task SetAPIKey(string apikey)
    {
        this.APIKey = apikey;
        List<VoiceProfile> voiceProfiles = await getVoiceProfile();
        lock (this)
        {
            List<OptionEntry<VoiceProfile>> optionEntries = new();
            foreach (VoiceProfile item in voiceProfiles)
            {
                optionEntries.Add(new(item, item.GetDisplayName()));
            }
            VoiceProfile = new("Voice", optionEntries);
        }

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
            UnityWebRequest request = CreateWebRequest("/v1/text-to-speech/" + voiceid);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerAudioClip(request.url, AudioType.MPEG);
            request.method = "POST";
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


    private UnityWebRequest CreateWebRequest(string path)
    {
        UnityWebRequest request = new(url + path);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", System.Environment.GetEnvironmentVariable(APIKey) ?? APIKey);
        return request;
    }

    public IEnumerable<OptionBase> GetTTSOptions()
    {
        return new List<OptionBase>()
        {
            VoiceProfile,
            Stability,
            SimilarityBoost,
            Speed,
            SpeakerBoost,
        };
    }

    public async IAsyncEnumerator<AudioClip> getAudioClip(string dialogue, OptionSet options)
    {
        options.ContainsOption(VoiceProfile, out PickOption<VoiceProfile> voices);
        AudioClip clip = await SendPostRequest(voices.GetCurrentValue().getId(), dialogue);
        yield return clip;
    }

    private async Task<List<VoiceProfile>> getVoiceProfile(string nextPageToken = "")
    {
        List<VoiceProfile> voiceProfiles = new();
        if (string.IsNullOrEmpty(APIKey))
        {
            throw new ArgumentException("The APIKey needs to be set for the settings to be correct");
        }
        string queryString = "";
        if (nextPageToken != null & nextPageToken != "")
        {
            queryString = "?next_page_token=" + nextPageToken;
        }
        UnityWebRequest request = CreateWebRequest("/v2/voices" + queryString);
        request.downloadHandler = new DownloadHandlerBuffer();
        await request.SendWebRequest();
        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        JToken voices = json["voices"];
        foreach (JToken voice in voices)
        {
            voiceProfiles.Add(new((string)voice["voice_id"], (string)voice["name"]));
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
