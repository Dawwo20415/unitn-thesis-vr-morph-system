using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;


public class NormalMatchingBehaviour : PlayableBehaviour, IKTarget
{
    public NativeArray<Quaternion> rotation;
    public HumanBodyBones hbb;

    public void Setup(HumanBodyBones h, NativeArray<Quaternion> naq)
    {
        hbb = h;
        rotation = naq;
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        EgocentricRayCasterWrapper caster = (EgocentricRayCasterWrapper)playerData;
        rotation[0] = caster.MatchPlaneNormal(hbb);
    }

    public Vector3 GetTarget()
    {
        return Vector3.zero;
    }
}

public struct NormalMatchingJob : IAnimationJob, IKTarget
{
    private TransformStreamHandle handle;
    private NativeArray<Quaternion> rot;

    public void Setup(Animator animator, HumanBodyBones hbb, NativeArray<Quaternion> naq)
    {
        handle = animator.BindStreamTransform(animator.GetBoneTransform(hbb));
        rot = naq;
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        handle.SetRotation(stream, handle.GetRotation(stream) * QExtension.Fix(rot[0]));
    }

    public Vector3 GetTarget()
    {
        return Vector3.zero;
    }
}
