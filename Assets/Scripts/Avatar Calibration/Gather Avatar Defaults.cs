using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GatherAvatarDefaults : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform skeletonRoot;
    public Avatar avatar;
    public string assetName = "AvatarDefault";

    [ContextMenu("Get Bome Centers")]
    public void GatherCenters()
    {
        HumanoidAvatarDefaults asset = ScriptableObject.CreateInstance<HumanoidAvatarDefaults>();
        string assetPath = "Assets/Scriptable Objects/" + assetName + ".asset";

        List<Quaternion> defaults = new List<Quaternion>(56);
        List<string> names = new List<string>(56);

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            string objName = LookUpBone(HumanTrait.BoneName[i]);
            bool found = false;
            foreach (Transform childTrn in skeletonRoot.GetComponentsInChildren<Transform>())
            {
                if (childTrn.gameObject.name == objName)
                {
                    defaults.Add(childTrn.localRotation);
                    names.Add(childTrn.gameObject.name);
                    found = true;
                }
            }
            if (!found)
            {
                defaults.Add(Quaternion.identity);
                names.Add(objName);
            }
        }

        asset.rotations = defaults;
        asset.names = names;

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
    }

    string LookUpBone(string name)
    {
        string ret = name;

        for (int i = 0; i < avatar.humanDescription.human.Length; i++)
        {
            if (avatar.humanDescription.human[i].humanName == name)
            {
                return avatar.humanDescription.human[i].boneName;
            }
        }

        if (ret == name)
            Debug.LogWarning("Selected Avatar is missing [" + name + "] in it's definition, please make sure that this is correct, the scriptable object entry for this bone will be filled with an identity quaternion");

        return ret;
    }
}
