using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mechanim Bone Mapping", menuName = "ScriptableObjects/Mechanim/Bone Mapping")]
public class MechanimBoneMotionMapping : ScriptableObject
{
    public List<Vector3Int> mappings;
}
