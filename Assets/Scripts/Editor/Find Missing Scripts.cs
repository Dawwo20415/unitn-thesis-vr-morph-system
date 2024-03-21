using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public static class FindMissingScripts {

    [MenuItem("My Menu/Find Missing Scripts In Project")]
    static void FindMissingScriptsInProject()
    {
        string[] prefabPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase)).ToArray();

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            foreach (Component component in prefab.GetComponentsInChildren<Component>())
            {
                if (component == null)
                {
                    Debug.Log("Prefab found with missing script " + path, prefab);
                    break;
                }
            }
        }
    }

    [MenuItem("My Menu/Find Missing Scripts In Scene")]
    static void FindMissingScriptsInScene()
    {
        foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>(true))
        {
            foreach (Component component in gameObject.GetComponentsInChildren<Component>())
            {
                if (component == null)
                {
                    Debug.Log("GameObject found with missing script: " + gameObject.name, gameObject);
                    break;
                }
            }
        }
    }
}
