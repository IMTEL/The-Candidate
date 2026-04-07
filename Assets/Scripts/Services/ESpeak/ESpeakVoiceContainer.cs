using System;
using Unity.InferenceEngine;
using UnityEngine;

[Serializable]
public class ESpeakVoiceContainer
{
    [SerializeField] private string name;

    private ESpeakTokenizer JSONtokenizer;

    [SerializeField] private TextAsset jsonFile;

    [SerializeField] private ModelAsset modelAsset;
    public ESpeakVoiceContainer(){}
    public ESpeakVoiceContainer(TextAsset jsonFile, ModelAsset modelAsset): this(jsonFile, modelAsset, jsonFile.name)
    {
        
    }
    public ESpeakVoiceContainer(TextAsset jsonFile, ModelAsset modelAsset, string name)
    {
        this.name = name;
        this.modelAsset = modelAsset;
        this.jsonFile = jsonFile;
        this.JSONtokenizer = new ESpeakTokenizer(jsonFile);
    }

    public ESpeakTokenizer GetTokenizer()
    {
        if (jsonFile == null)
        {
            Debug.LogError($"jsonFile is NULL on {name}");
            return null;
        }

        if (JSONtokenizer == null)
        {
            Debug.Log($"Recreating tokenizer for {name}");
            JSONtokenizer = new ESpeakTokenizer(jsonFile);
        }

        return JSONtokenizer;
    }

    public ModelAsset GetModelAsset()
    {
        return modelAsset;
    }

    public string GetName()
    {
        return name;
    }

    public TextAsset GetTextAsset()
    {
        return jsonFile;
    }
}