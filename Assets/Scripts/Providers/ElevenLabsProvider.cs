using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Alchemy.Inspector;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

[CreateAssetMenu(menuName = "Services/Elevenlabs")]
public class ElevenLabsProvider : TTSserviceProvider
{
    [SerializeField] private string apiKey;


    public override TTService CreateTypedService()
        => new ElevenLabsService(apiKey);

    [Button]
    public async Task SyncVoiceProfiles()
    {
        this.voices = await getVoiceProfile();
    }

    public override IEnumerable<OptionBase> GetTTSOptions()
    {
        List<OptionBase> options = new();

        return ElevenLabsService.GetTTSOptions();
    }

    private async Task<List<VoiceProfile>> getVoiceProfile(string nextPageToken = "")
    {
        List<VoiceProfile> voiceProfiles = new();
        if(string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("The apikey needs to be set for the settings to be correct");
        }
        string queryString = "";
        if(nextPageToken != null & nextPageToken != "")
        {
            queryString = "?next_page_token="+nextPageToken;
        }
        string url = "https://api.elevenlabs.io/v2/voices"+queryString;
        UnityWebRequest request = new(url)
        {
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("xi-api-key", apiKey);
        await request.SendWebRequest();
        JObject json = JObject.Parse(DownloadHandlerBuffer.GetContent(request));
        JToken voices = json["voices"];
        foreach (JToken voice in voices)
        {
            voiceProfiles.Add(new((string?)voice["voice_id"], (string?)voice["name"]));
            Debug.Log((string)voice["settings"]);
        }
        if(string.IsNullOrWhiteSpace((string)json["next_page_token"])  && (string)json["next_page_token"] != nextPageToken)
        {
            foreach(VoiceProfile voice in await getVoiceProfile((string)json["next_page_token"])) {
                voiceProfiles.Add(voice);
            }
        }
        return voiceProfiles;
    }
}