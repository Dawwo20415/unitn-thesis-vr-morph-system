using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public class QuaternionHandler : PlayableBehaviour
{
    private Quaternion modification;
    private int hbb_index;
    private IHumanBodyBonesSplit behaviour;

    public QuaternionHandler()
    {
        
    }

    public void QuaternionSetup(int index, IHumanBodyBonesSplit pose, Quaternion q)
    {
        modification = q;
        hbb_index = index;
        behaviour = pose;
    }

    public void UpdateData(int index, Quaternion q)
    {
        modification = q;
        hbb_index = index;
    }

    public Quaternion GetQuaternion()
    {
        return behaviour.GetRotation(hbb_index) * modification;
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) { }
}

public class PoseConjunction : PlayableBehaviour, IHumanBodyBonesSplit
{
    private IHumanBodyBonesSplit source_pose;
    private QuaternionHandler quat;
    private int index;

    public PoseConjunction()
    {
        
    }

    public void SetupConjunction(IHumanBodyBonesSplit pose, int hbb_index, QuaternionHandler behaviour)
    {
        source_pose = pose;
        index = hbb_index;
        quat = behaviour;
    }

    public void UpdateData(int hbb_index, QuaternionHandler behaviour)
    {
        index = hbb_index;
        quat = behaviour;
    }

    public Quaternion GetRotation(int hbb_index)
    {
        if (hbb_index == index)
        {
            return quat.GetQuaternion();
        } else
        {
            return source_pose.GetRotation(hbb_index);
        }
    }

    public Vector3 GetPosition(int hbb_index)
    {
        return source_pose.GetPosition(hbb_index);
    }

    public bool GetBoneStatus(int hbb_index)
    {
        return source_pose.GetBoneStatus(hbb_index);
    }

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData) { }
}