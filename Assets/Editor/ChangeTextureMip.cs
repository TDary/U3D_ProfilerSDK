using System.IO;
using UnityEditor;
using UnityEngine;

public class ChangeTextureMip
{
    [MenuItem("TestChangeTexture/TextureMipmaps")]
    public static void ChangeMipMaps()
    {
        string AssetPath = "Assets\\Art\\";
        DirectoryInfo dir = new DirectoryInfo(AssetPath);
        Debug.Log(dir.FullName);
        FileInfo[] files = dir.GetFiles("*");
        foreach(var item in files)
        {
            Debug.Log(item.Name);
        }


        //string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets" });

        //foreach (string guid in guids)
        //{
        //    string path = AssetDatabase.GUIDToAssetPath(guid);
        //    TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        //    if (textureImporter != null)
        //    {
        //        textureImporter.streamingMipmaps = true;
        //        //textureImporter.SaveAndReimport();
        //    }
        //}
    }
}
