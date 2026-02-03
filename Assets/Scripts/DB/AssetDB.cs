using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DB/Texture")]
[Serializable]
public class AssetDB : ScriptableObject {
    
    [SerializeField] public List<TextureLog> entries = new();


#if UNITY_EDITOR
    private void OnValidate()
    {
        entries ??= new();
    }
#endif

    [Serializable]
    public struct TextureLog
    {
        [SerializeField] public string referenceName;
        [SerializeField] public Material material;
        [SerializeField] public int index;
    }
}