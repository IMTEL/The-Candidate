using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// A TTS Script using the ElevenLabs API TTS Service. Requires the a enviroment variable for the API key.
/// </summary>
public class ElevenLabsService : TTService 
{

    private string url = "https://api.elevenlabs.io/v1/text-to-speech/";
    string apiKey;

    private class TTSRequestBody
    {
        public string text;
        public string model_id = "eleven_flash_v2_5";
        public string language_code = "no";

        public TTSRequestBody(string text, string model_id = "eleven_flash_v2_5", string language_code = "no")
        {
            this.text = text;
            this.model_id = model_id;
            this.language_code = language_code;    
        }
    }

    private readonly SemaphoreSlim semaphore = new(1);

    public ElevenLabsService(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public string getVoiceId(Voice voice)
    {
        return voice switch
        {
            Voice.Ola => "dgrgQcxISbZtq517iweJ",
            Voice.Johannes => "2dhHLsmg0MVma2t041qT",
            _ => getVoiceId(Voice.Ola),
        };
    }
    

    public override Task<AudioClip> getAudioClip(Voice voice, string dialogue)
    {
        return SendPostRequest(getVoiceId(voice), dialogue);
    }

    private async Task<AudioClip> SendPostRequest(string voiceid, string dialogue)
    {
        await semaphore.WaitAsync();
        string jsonPayload = JsonConvert.ToString(new TTSRequestBody(dialogue));
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


}
