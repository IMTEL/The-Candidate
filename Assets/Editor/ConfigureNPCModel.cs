using System.Collections.Generic;
using System.Linq;
using uLipSync;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
/// <summary>
/// Script for configuring the NPC's model (this case MakeHuman) with the right component to be used in the game.
/// This includes fititng them with Ulipsync, adding them into a prefab variant of the core NPC module, 
/// zeroing blendshapes and trying to link blendshapes to the chosen Ulipsync profile.
/// </summary>
public class ModelImporterWindow : EditorWindow
{
    private GameObject model;
    private Profile uLipSyncProfile;
    private GameObject parentPrefab;

    [MenuItem("Tools/Configure Make Human Model with Ulipsync")]
    public static void ShowWindow()
    {
        GetWindow<ModelImporterWindow>("Model Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import Model to Prefab Variant", EditorStyles.boldLabel);

        model = (GameObject)EditorGUILayout.ObjectField("Model", model, typeof(GameObject), false);
        parentPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab Parent", parentPrefab,typeof(GameObject), false);
        uLipSyncProfile = (Profile)EditorGUILayout.ObjectField("uLipSyncProfile", uLipSyncProfile, typeof(Profile), false);
            
        if (GUILayout.Button("Import and Configure") && model != null && uLipSyncProfile != null && parentPrefab != null)
        {
            ImportAndConfigure();
        }
    }

    /// <summary>
    /// Imports the NPC model into a chosen prefab parent and configures it to a usable parent
    /// </summary>
    private void ImportAndConfigure()
    {
        #region Creation of the prefab
        SkinnedMeshRenderer possibleBlendShape = model.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(renderer => renderer.sharedMesh.blendShapeCount > 0);
        if (possibleBlendShape == null)
        {
            Debug.LogError("Could not find a possible location for the blendshapes. Please import the model without helpers and transforms turned off");
            return;
        }
        
        string path = "Assets/Prefabs/NPCs/" + model.name + "_Variant.prefab";
        GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset(parentPrefab, path);
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
        
        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        possibleBlendShape = modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>().FirstOrDefault(renderer => renderer.sharedMesh.blendShapeCount > 0);
        //Reset the blendshapes to 0
        for(int i = 0; i < possibleBlendShape.sharedMesh.blendShapeCount; i++) possibleBlendShape.SetBlendShapeWeight(i,0f);

        //Create a prefab variant
        modelInstance.transform.SetParent(prefabRoot.transform);
        modelInstance.transform.position = Vector3.zero;
        NPC controller = prefabRoot.GetComponentInChildren<NPC>();
        #endregion


        #region Link NPC module ot correct areas
        //Link up the head to the NPC module
        //TODO: Can be done trought the Ulipsync Audiosource.. Need to double check if it is worth it
        controller.head = RecursiveDepthSearch(modelInstance.transform ,"head")?.gameObject;
        if (controller.head == null) Debug.LogWarning("Could not find a head to link to the NPC. Please link this yourselves");
        else controller.transform.position = controller.head.transform.position;
        #endregion


        #region Mapping blendshapes to the profile
        //Setting up Ulipsync with the correct profile 
        uLipSync.uLipSync lipsync = controller.gameObject.AddComponent<uLipSync.uLipSync>();
        lipsync.profile = uLipSyncProfile;
        
        //Add the blendshape, add the mesh renderer and add it as a listener to the lipsync
        uLipSyncBlendShape BlendShape = possibleBlendShape.gameObject.AddComponent<uLipSyncBlendShape>();
        BlendShape.skinnedMeshRenderer = possibleBlendShape;
        UnityEventTools.AddPersistentListener(lipsync.onLipSyncUpdate, BlendShape.OnLipSyncUpdate);

        string[] names = uLipSyncProfile.GetPhonemeNames();
        Mesh sharedMesh = possibleBlendShape.sharedMesh;
        foreach(string name in names)
        {
            uLipSyncBlendShape.BlendShapeInfo blendShapeInfo = new();
            blendShapeInfo.phoneme = name;
            blendShapeInfo.index = findCorrectIndex(name,sharedMesh);
            if(blendShapeInfo.index < 0) Debug.LogWarning("Could not link phenome "+name+" to any blendshape. Double check and link yourself");
            BlendShape.blendShapes.Add(blendShapeInfo);
        }
        BlendShape.ApplyBlendShapes();
        #endregion

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log($"Prefab variant created and configured at {path}");
    }

    /// <summary>
    /// Finds the correct index of the blendshape based upon the phenomen name
    /// </summary>
    /// <param name="name">The name it should search for</param>
    /// <param name="mesh">The mesh containing the blendshape</param>
    /// <returns>A identical match or a match based upon the first character</returns>
    private int findCorrectIndex(string name, Mesh mesh)
    {   
        int i = mesh.GetBlendShapeIndex(name);
        if (i > -1) return i;

        bool found = false;
        for(i = 0; !found && i < mesh.blendShapeCount; i++)
        {            
            found = mesh.GetBlendShapeName(i).ToLower()[0].Equals(name.ToLower()[0]);
        }
        return found ? i-1 : -1 ;
    }

    private Transform RecursiveDepthSearch(Transform parent, string name)
    {
        foreach(Transform child in parent)
        {
            if (child.name.ToLower().Contains(name.ToLower()))
            {
                return child;
            }
            Transform result = RecursiveDepthSearch(child, name);
            if (result != null) return result;
        }
        return null;
    }
}