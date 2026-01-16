using UnityEngine;
using UnityEditor;
using System.IO;

public class SimpleGridAtlasGenerator
{
    [MenuItem("Tools/Generate Grid Texture Atlas")]
    static void GenerateGridAtlas()
    {
        Object[] selection = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        if (selection.Length == 0)
        {
            Debug.LogError("No textures selected!");
            return;
        }

        int texCount = selection.Length;
        Texture2D firstTex = selection[0] as Texture2D;
        int texWidth = firstTex.width;
        int texHeight = firstTex.height;

        // Calculate grid (square-ish)
        int cols = Mathf.CeilToInt(Mathf.Sqrt(texCount));
        int rows = Mathf.CeilToInt((float)texCount / cols);

        // Create atlas
        Texture2D atlas = new Texture2D(cols * texWidth, rows * texHeight, TextureFormat.RGBA32, false);

        for (int i = 0; i < texCount; i++)
        {
            Texture2D tex = selection[i] as Texture2D;

            // Ensure readable
            string path = AssetDatabase.GetAssetPath(tex);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            // Calculate position in atlas
            int col = i % cols;
            int row = i / cols;
            int x = col * texWidth;
            int y = atlas.height - (row + 1) * texHeight; // bottom-left origin

            atlas.SetPixels(x, y, texWidth, texHeight, tex.GetPixels());
        }

        atlas.Apply();

        // Save atlas
        string savePath = EditorUtility.SaveFilePanelInProject(
            "Save Grid Texture Atlas",
            "GridAtlas",
            "png",
            "Save atlas texture"
        );

        if (!string.IsNullOrEmpty(savePath))
        {
            File.WriteAllBytes(savePath, atlas.EncodeToPNG());
            AssetDatabase.Refresh();
            Debug.Log("Grid atlas saved to " + savePath);
        }
    }
}
