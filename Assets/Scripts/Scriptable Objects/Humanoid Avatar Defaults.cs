using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAvatarDefaults : ScriptableObject
{
    public List<Quaternion> muscleCenters;
    public List<Quaternion> tPoseOrientations_world;
    public List<Quaternion> tPoseOrientations_local;
    public List<string> names;
    
}
