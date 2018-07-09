#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;

internal class ScriptableObjectManager
{
    internal static void CreateAsset<T>(string fileName) where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T>();
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        // if filepath is correct
        if (path == "")
        {
            path = "Assets";
        }

        // if extention is correct
        if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
        }

        // create scriptable object asset
        AssetDatabase.CreateAsset(asset, path +  "/New " + fileName + ".asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
#endif