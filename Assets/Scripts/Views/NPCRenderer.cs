using UnityEngine;
using static AssetDB;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class NPCRenderer : MonoBehaviour
{
    public AssetDB assetDB;
    public Material material = null;
    public int index = -1;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SkinnedMeshRenderer renderer = GetComponent<SkinnedMeshRenderer>();
        if (material == null || index < 0)
        {
            TextureLog texture = assetDB.entries.Find(entry => renderer.material.name.Contains(entry.referenceName));
            material = texture.material;
            index = texture.index;
        } 
        renderer.sharedMaterial = material;
        MaterialPropertyBlock materialPropertyBlock = new();
        materialPropertyBlock.SetInt("index", index);
        renderer.SetPropertyBlock(materialPropertyBlock);
    }
}
