using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Unity.Collections;

public class EgocentricIKChainBehaviour : PlayableBehaviour
{
    /*public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        PlayableGraph graph = playable.GetGraph();
        ScriptPlayableOutput output = PlayableGraphUtility.CheckConnectedUserDataByType<TestEgocentricOutput>(graph, playable);
        TestEgocentricOutput userData = (TestEgocentricOutput)output.GetUserData();
    }*/

    public override void PrepareFrame(Playable playable, FrameData info) { }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!playerData.GetType().Equals(typeof(TestEgocentricOutput))) { return; }

        //Debug.Log("Process Frame");
    }
}

public struct DefineTargets : IAnimationJob
{
    NativeArray<Vector3> m_Targets;
    NativeArray<TransformStreamHandle> m_Handles;

    public void Setup(NativeArray<Vector3> array, Animator animator)
    {
        m_Targets = array;
        m_Handles = new NativeArray<TransformStreamHandle>(3, Allocator.Persistent);

        {
            m_Handles[0] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftShoulder));
            m_Handles[1] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
            m_Handles[2] = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
        }
    }

    public void ProcessRootMotion(AnimationStream stream) { }
    public void ProcessAnimation(AnimationStream stream)
    {
        m_Targets[(int)HumanBodyBones.LeftShoulder] = m_Handles[0].GetPosition(stream);
        m_Targets[(int)HumanBodyBones.LeftUpperArm] = m_Handles[1].GetPosition(stream);
        m_Targets[(int)HumanBodyBones.LeftLowerArm] = m_Handles[2].GetPosition(stream);
    }

    public void Dispose()
    {
        m_Handles.Dispose();
    }
}

