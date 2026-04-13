using System;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;   //DO NOT REMOVE
using System.IO.Compression;    //DO NOT REMOVE
using Unity.InferenceEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections;


/// <summary>
/// Modified version of the PiperManager from https://github.com/skykim/piper-unity
/// </summary>
public class PiperManager : ITTService
{

    /// <summary>
    /// One out of 2 for caching the models. Keeps the order of the modelassets when they were last called
    /// </summary>
    private LinkedList<ModelAsset> modelsLastUse = new();

    /// <summary>
    /// One out of 2 for caching the models. Lookuptable for getting the models fast
    /// </summary>
    private Dictionary<ModelAsset, (Model, Worker)> modelsLookupTable = new();

    /// <summary>
    /// A value for the amount of models this can use... Arguably should be a option, which I am gonna consider
    /// </summary>
    private int modelCapacity;

    private static PiperManager instance;

    [Range(0.0f, 1.0f)] public float commaDelay = 0.1f;
    [Range(0.0f, 1.0f)] public float periodDelay = 0.5f;
    [Range(0.0f, 1.0f)] public float questionExclamationDelay = 0.6f;


    private PiperManager(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentException("Need a higher capacity then a negative number or 0");
        }
        modelCapacity = capacity;
        AMagicalCrashingFunction().Wait();
    }

    public static PiperManager GetInstance(int capacity = 3)
    {
        instance ??= new(capacity);
        return instance;
    }

    private async Task AMagicalCrashingFunction()
    {
        string espeakDataPath;
        #if UNITY_ANDROID && !UNITY_EDITOR
            espeakDataPath = Path.Combine(Application.persistentDataPath, "espeak-ng-data");

            if (!Directory.Exists(espeakDataPath))
            {
                Debug.Log("Android: eSpeak data not found in persistentDataPath. Starting copy process...");

                string zipSourcePath = Path.Combine(Application.streamingAssetsPath, "espeak-ng-data.zip");
                string zipDestPath = Path.Combine(Application.persistentDataPath, "espeak-ng-data.zip");

                using (UnityWebRequest www = UnityWebRequest.Get(zipSourcePath))
                {
                    await www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Failed to load espeak-ng-data.zip from StreamingAssets: {www.error}");
                    }

                    File.WriteAllBytes(zipDestPath, www.downloadHandler.data);

                    try
                    {
                        ZipFile.ExtractToDirectory(zipDestPath, Application.persistentDataPath);
                        Debug.Log("eSpeak data successfully unzipped to persistentDataPath.");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error unzipping eSpeak data: {e.Message}");
                    }
                    finally
                    {
                        if (File.Exists(zipDestPath))
                        {
                            File.Delete(zipDestPath);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Android: eSpeak data already exists in persistentDataPath.");
            }
        #else
            espeakDataPath = Path.Combine(Application.streamingAssetsPath, "espeak-ng-data");
            Debug.Log($"Editor/Standalone: Using eSpeak data directly from StreamingAssets: {espeakDataPath}");
        #endif
        AnotherMagicalCrashingFunction(espeakDataPath);
        Debug.Log("Piper Manager initialized.");
    }

    private void AnotherMagicalCrashingFunction(string dataPath)
    {
        int initResult = ESpeakNG.espeak_Initialize(0, 0, dataPath, 0);

        if (initResult > 0)
        {
            Debug.Log($"[PiperManager] eSpeak-ng Initialization SUCCEEDED. Data path: {dataPath}");
        }
        else
        {
            Debug.LogError($"[PiperManager] eSpeak-ng Initialization FAILED. Error code: {initResult}");
        }
    }
    
    public Task<AudioClip> getAudioClip(string dialogue, IEnumerable<OptionBase> options)
    {
        Debug.Log("Creating audioclip from piper model");
        PickOption<ESpeakVoiceContainer> important = options.OfType<PickOption<ESpeakVoiceContainer>>().First() ?? throw new ArgumentException("Oy, we are missing a model information for this one. Plz fix");
        return Synthesize(dialogue, important.GetCurrentValue().GetTokenizer(), important.GetCurrentValue().GetModelAsset());
    }

    /// <summary>
    /// Creates a TTS audioclip from the text
    /// </summary>
    /// <param name="text">Things to be said</param>
    /// <returns>A audioclip to be voiced</returns>s
    private async Task<AudioClip> Synthesize(string text, ESpeakTokenizer tokenizer, ModelAsset model)
    {
        if (tokenizer == null || string.IsNullOrEmpty(tokenizer.Voice))
        {
            Debug.LogError("[PiperManager] Tokenizer is not assigned or has no voice name.");
            throw new ArgumentException("Tokenizer is either null or is missing a voice");
        }
        string voiceName = tokenizer.Voice;
        int voiceResult = ESpeakNG.espeak_SetVoiceByName(voiceName);
        (Model, Worker) lodedModel = FetchModel(model);
        string delayPunctuationPattern = @"([,.?!;:])";
        string nonDelayPunctuationPattern = @"[^\w\s,.?!;:]";

        string[] parts = Regex.Split(text, delayPunctuationPattern);
        List<AudioClip> audioClips = new();
        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part.Trim()))
            {
                continue;
            }

            bool isDelayPunctuation = Regex.IsMatch(part, "^" + delayPunctuationPattern + "$");

            if (isDelayPunctuation)
            {
                switch (part)
                {
                    case ",":
                    case ";":
                    case ":":
                        audioClips.Add(AudioClip.Create("Silence1", (int)Math.Round(tokenizer.SampleRate * commaDelay, MidpointRounding.AwayFromZero), 1, tokenizer.SampleRate, false));
                        break;
                    case ".":
                        audioClips.Add(AudioClip.Create("Silence2", (int)Math.Round(tokenizer.SampleRate * periodDelay, MidpointRounding.AwayFromZero), 1, tokenizer.SampleRate, false));
                        break;
                    case "?":
                    case "!":
                        audioClips.Add(AudioClip.Create("Silence3", (int)Math.Round(tokenizer.SampleRate * questionExclamationDelay, MidpointRounding.AwayFromZero), 1, tokenizer.SampleRate, false));
                        break;
                }
            }
            else
            {
                string cleanedChunk = Regex.Replace(part, nonDelayPunctuationPattern, " ");
                cleanedChunk = cleanedChunk.Trim();

                if (!string.IsNullOrEmpty(cleanedChunk))
                {
                    Debug.Log($"Processing text chunk: \"{cleanedChunk}\"");
                    audioClips.Add(SynthesizeChunk(cleanedChunk, tokenizer, lodedModel));
                }
            }
        }
        return ConcatenateClips(audioClips);
    }

    /// <summary>
    /// Fetches model from the cache if it already exist
    /// If not creates the worker and model and caches them later
    /// </summary>
    /// <param name="modelAsset"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private (Model, Worker) FetchModel(ModelAsset modelAsset)
    {
        if(modelAsset == null)
        {
            throw new ArgumentException("ModelAsset is null");
        }
        if (modelsLookupTable.TryGetValue(modelAsset, out (Model, Worker) model))
        {
            modelsLastUse.Remove(modelAsset);
            modelsLastUse.AddFirst(modelAsset);
            return model;
        }
        if(modelsLastUse.Count() >= modelCapacity)
        {
            modelsLookupTable.Remove(modelsLastUse.Last.Value, out (Model, Worker) value);
            value.Item2.Dispose();
            modelsLastUse.RemoveLast();
        }
        model.Item1 = ModelLoader.Load(modelAsset);
        model.Item2 = new Worker(model.Item1, BackendType.CPU);
        modelsLastUse.AddFirst(modelAsset);
        modelsLookupTable.Add(modelAsset, model);
        return model;
    }

    /// <summary>
    /// Stitces audioclips together to one version
    /// Written by Claude since my knowledge of audio stuff is limited
    /// </summary>
    /// <param name="clips">The clips to be stitched together</param>
    /// <returns>The full audioclip to be</returns>
    private AudioClip ConcatenateClips(List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return null;

        int sampleRate = clips[0].frequency;
        int channels = clips[0].channels;

        // Total samples across all clips (silence clips included)
        int totalSamples = 0;
        foreach (var clip in clips)
            totalSamples += clip.samples * clip.channels;

        float[] combinedData = new float[totalSamples];
        int offset = 0;

        foreach (var clip in clips)
        {
            float[] clipData = new float[clip.samples * clip.channels];
            clip.GetData(clipData, 0);
            System.Array.Copy(clipData, 0, combinedData, offset, clipData.Length);
            offset += clipData.Length;
        }

        AudioClip merged = AudioClip.Create("MergedClip", totalSamples / channels, channels, sampleRate, false);
        merged.SetData(combinedData, 0);

        return merged;
    }

    /// <summary>
    /// A decorated function becouse lazyiness is such a wonder.
    /// </summary>
    /// <param name="textChunk"></param>
    /// <param name="speakTokenizer"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    private AudioClip SynthesizeChunk(string textChunk, ESpeakTokenizer speakTokenizer, (Model, Worker) model)
    {
        return SynthesizeChunk(textChunk, speakTokenizer, model.Item1, model.Item2);
    }

    private AudioClip SynthesizeChunk(string textChunk, ESpeakTokenizer speakTokenizer, Model model, Worker engine) 
    {
        string phonemeStr = Phonemize(textChunk);
        if (string.IsNullOrEmpty(phonemeStr))
        {
            Debug.LogError($"Phoneme conversion failed for chunk: \"{textChunk}\"");
            throw new InvalidDataException($"Phoneme conversion failed for chunk: \"{textChunk}\"");
        }

        string[] phonemeArray = phonemeStr.Trim().Select(c => c.ToString()).ToArray();
        int[] phonemeTokens = speakTokenizer.Tokenize(phonemeArray);

        float[] scales = speakTokenizer.GetInferenceParams();
        int[] inputLength = { phonemeTokens.Length };

        Debug.Log($"Model inputs prepared. Token count: {inputLength[0]}, Scales: [{string.Join(", ", scales)}]");

        using var phonemesTensor = new Tensor<int>(new TensorShape(1, phonemeTokens.Length), phonemeTokens);
        using var lengthTensor = new Tensor<int>(new TensorShape(1), inputLength);
        using var scalesTensor = new Tensor<float>(new TensorShape(3), scales);

        engine.SetInput("input", phonemesTensor);
        engine.SetInput("input_lengths", lengthTensor);
        engine.SetInput("scales", scalesTensor);
        if (model.inputs.Count == 4 && model.inputs[3].name == "sid")
        {
            engine.SetInput("sid", new Tensor<int>(new TensorShape(1), new int[] { 0 }));
        }

        engine.Schedule();

        using var outputTensor = (engine.PeekOutput() as Tensor<float>).ReadbackAndClone();
        float[] audioData = outputTensor.DownloadToArray();

        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("Failed to generate audio data or the data is empty.");
            throw new Exception("Failed to generate audio data or the data is empty.");
        }
        Debug.Log($"Generated audio data length: {audioData.Length}");

        int sampleRate = speakTokenizer.SampleRate;
        AudioClip clip = AudioClip.Create("GeneratedSpeech", audioData.Length, 1, sampleRate, false);
        clip.SetData(audioData, 0);

        Debug.Log($"Speech generated! AudioClip length: {clip.length:F2}s.");
        return clip;
    }

    private string Phonemize(string text)
    {
        Debug.Log($"Phonemizing text: \"{text}\"");
        IntPtr textPtr = IntPtr.Zero;
        try
        {
            Debug.Log($"[PiperManager] Cleaned text for phonemization: \"{text}\"");
            byte[] textBytes = Encoding.UTF8.GetBytes(text + "\0");
            textPtr = Marshal.AllocHGlobal(textBytes.Length);
            Marshal.Copy(textBytes, 0, textPtr, textBytes.Length);

            IntPtr pointerToText = textPtr;

            int textMode = 0; // espeakCHARS_AUTO=0
            int phonemeMode = 2; // bit 1: 0=eSpeak's ascii phoneme names, 1= International Phonetic Alphabet (as UTF-8 characters).

            IntPtr resultPtr = ESpeakNG.espeak_TextToPhonemes(ref pointerToText, textMode, phonemeMode);

            if (resultPtr != IntPtr.Zero)
            {
                string phonemeString = PtrToUtf8String(resultPtr);
                Debug.Log($"[PHONEMES] {phonemeString}");
                return phonemeString;
            }
            else
            {
                Debug.LogError("[PiperManager] Phonemize FAILED. The function returned a null pointer.");
                return null;
            }
        }
        finally
        {
            if (textPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(textPtr);
            }
        }
    }

    private static string PtrToUtf8String(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return "";
        var byteList = new List<byte>();
        for (int offset = 0; ; offset++)
        {
            byte b = Marshal.ReadByte(ptr, offset);
            if (b == 0) break;
            byteList.Add(b);
        }
        return Encoding.UTF8.GetString(byteList.ToArray());
    }



    public static IEnumerable<OptionBase> GetTTSOptions()
    {

        return new List<OptionBase>()
        {
            new FloatBoundOption("CommaDelay", Minvalue: 0, MaxValue: 3),
            new FloatBoundOption("PeriodDelay", Minvalue: 0, MaxValue: 3),
            new FloatBoundOption("QuestionExclamationDelay", Minvalue: 0, MaxValue: 3)
        };
    }
}