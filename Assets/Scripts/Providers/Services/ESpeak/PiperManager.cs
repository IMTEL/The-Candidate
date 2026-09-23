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


/// <summary>
/// Modified version of the PiperManager from https://github.com/skykim/piper-unity
/// </summary>
public class PiperManager : ITTService
{

    /// <summary>
    /// One out of 2 for caching the models. Keeps the order of the modelassets when they were last called
    /// </summary>
    private readonly LinkedList<ModelAsset> modelsLastUse = new();

    /// <summary>
    /// One out of 2 for caching the models. Lookuptable for getting the models fast
    /// </summary>
    private readonly Dictionary<ModelAsset, (Model, Worker)> modelsLookupTable = new();

    /// <summary>
    /// A value for the amount of models this can use... Arguably should be a option, which I am gonna consider
    /// </summary>
    private readonly int modelCapacity;

    private readonly Task initilize;

    private static PiperManager instance;

    private static readonly FloatBoundOption CommaDelay = new("CommaDelay", Minvalue: 0, MaxValue: 3);
                
    private static readonly FloatBoundOption PeriodDelay = new("PeriodDelay", Minvalue: 0, MaxValue: 3);
    private static readonly FloatBoundOption QuestionExclamationDelay = new("QuestionExclamationDelay", Minvalue: 0, MaxValue: 3);



    private PiperManager(int capacity)
    {
        if (capacity < 0)
        {
            throw new ArgumentException("Need a higher capacity then a negative number or 0");
        }
        modelCapacity = capacity;
        initilize = AMagicalCrashingFunction();
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

    public async IAsyncEnumerator<AudioClip> getAudioClip(string dialogue, OptionSet options)
    {
        PickOption<ESpeakVoiceContainer> important = options.GetOptions().OfType<PickOption<ESpeakVoiceContainer>>().First() ?? throw new ArgumentException("Oy, we are missing a model information for this one. Plz fix");
        OptionSet set = GetTTSOptions().Merge(options);
        ESpeakTokenizer tokenizer = important.GetCurrentValue().GetTokenizer();
        ModelAsset model = important.GetCurrentValue().GetModelAsset();
        set.ContainsOption(PeriodDelay, out FloatBoundOption TempPeriodDelay);
        set.ContainsOption(CommaDelay, out FloatBoundOption TempCommaDelay);
        set.ContainsOption(QuestionExclamationDelay, out FloatBoundOption TempQuestionDelay);

        if (tokenizer == null || string.IsNullOrEmpty(tokenizer.Voice))
        {
            Debug.LogError("[PiperManager] Tokenizer is not assigned or has no voice name.");
            throw new ArgumentException("Tokenizer is either null or is missing a voice");
        }
        await initilize;
        string delayPunctuationPattern = @"([,.?!;:])";
        string nonDelayPunctuationPattern = @"[^\w\s,.?!;:]";
        string[] parts = Regex.Split(dialogue, delayPunctuationPattern);
        foreach (string part in parts)
        {
            _ = ESpeakNG.espeak_SetVoiceByName(tokenizer.Voice);
            (Model, Worker) lodedModel = FetchModel(model);
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
                        if(TempCommaDelay.GetValue() > 0)
                        yield return AudioClip.Create("Silence1", (int)Math.Round(tokenizer.SampleRate * TempCommaDelay.GetValue(), MidpointRounding.AwayFromZero), 1, tokenizer.SampleRate, false);
                        break;
                    case ".":
                        if(TempPeriodDelay.GetValue() > 0)
                        yield return AudioClip.Create("Silence2", (int)Math.Round(tokenizer.SampleRate * TempPeriodDelay.GetValue(), MidpointRounding.AwayFromZero), 1, tokenizer.SampleRate, false);
                        break;
                    case "?":
                    case "!":
                        if(TempQuestionDelay.GetValue() > 0)
                        yield return AudioClip.Create("Silence3", (int)Math.Round(tokenizer.SampleRate * TempQuestionDelay.GetValue(), MidpointRounding.AwayFromZero), 1, tokenizer.SampleRate, false);
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
                    yield return SynthesizeChunk(cleanedChunk, tokenizer, lodedModel);
                }
            }
        }
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
        if (modelAsset == null)
        {
            throw new ArgumentException("ModelAsset is null");
        }
        if (modelsLookupTable.TryGetValue(modelAsset, out (Model, Worker) model))
        {
            modelsLastUse.Remove(modelAsset);
            modelsLastUse.AddFirst(modelAsset);
            return model;
        }
        if (modelsLastUse.Count() >= modelCapacity)
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
    /// A decorated function because laziness is such a wonder.
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



    public static OptionSet GetTTSOptions()
    {
        OptionSet set = new();
        set.AddOption(CommaDelay);
        set.AddOption(PeriodDelay);
        set.AddOption(QuestionExclamationDelay);
        return set;    
    }


}